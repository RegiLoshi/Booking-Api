using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using BookingApplication.Abstractions.Contracts.Email;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApp.Hubs;
using BookingDomain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BookingApp.Middleware;

public class BookingStatusEmailMiddleware(
    IBookingRepository bookingRepository,
    IPropertyRepository propertyRepository,
    IUserRepository userRepository,
    IEmailSender emailSender,
    IHubContext<BookingHub> hubContext,
    ILogger<BookingStatusEmailMiddleware> logger)
    : IMiddleware
{
    private static readonly Regex BookingRouteRegex = new Regex(
        @"^/v1/booking/(?<id>[0-9a-fA-F\-]{36})/(?<action>confirm|reject|cancel|complete)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);

        if (context.Request.Method != HttpMethods.Post)
            return;

        if (context.Response.StatusCode != (int)HttpStatusCode.OK)
            return;

        var path = context.Request.Path.Value ?? string.Empty;
        var match = BookingRouteRegex.Match(path);
        if (!match.Success)
            return;

        var bookingId = Guid.Parse(match.Groups["id"].Value);
        var action = match.Groups["action"].Value;

        var newStatus = action switch
        {
            "confirm" => BookingStatus.Confirmed,
            "reject" => BookingStatus.Rejected,
            "cancel" => BookingStatus.Cancelled,
            "complete" => BookingStatus.Completed,
            _ => (BookingStatus?)null
        };

        if (newStatus == null)
            return;

        try
        {
            var booking = await bookingRepository.GetById(bookingId, context.RequestAborted);
            if (booking == null)
                return;

            var property = await propertyRepository.GetById(booking.PropertyId, context.RequestAborted);
            if (property == null)
                return;

            var guestUser = await userRepository.GetUserById(booking.GuestId, context.RequestAborted);
            var ownerUser = await userRepository.GetUserById(property.OwnerId, context.RequestAborted);

            // Real-time notification
            var payload = new
            {
                bookingId = booking.Id,
                propertyId = property.Id,
                propertyName = property.Name,
                status = newStatus.Value.ToString(),
                startDate = booking.StartDate.Date,
                endDate = booking.EndDate.Date,
                nights = Math.Max(0, (booking.EndDate.Date - booking.StartDate.Date).Days),
                guestCount = booking.GuestCount,
                totalPrice = booking.TotalPrice
            };

            await hubContext.Clients.User(booking.GuestId.ToString())
                .SendAsync("BookingStatusChanged", payload, context.RequestAborted);

            await hubContext.Clients.User(property.OwnerId.ToString())
                .SendAsync("BookingStatusChanged", payload, context.RequestAborted);

            await SendStatusEmail(
                guestUser,
                ownerUser,
                booking,
                property,
                newStatus.Value,
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send booking status email. BookingId={BookingId}, Action={Action}", bookingId, action);
        }
    }

    private async Task SendStatusEmail(
        Users? guestUser,
        Users? ownerUser,
        Bookings booking,
        Properties property,
        BookingStatus newStatus,
        CancellationToken cancellationToken)
    {
        var startDate = booking.StartDate.Date;
        var endDate = booking.EndDate.Date;
        var start = startDate.ToString("yyyy-MM-dd");
        var end = endDate.ToString("yyyy-MM-dd");
        var nights = Math.Max(0, (endDate - startDate).Days);
        var propertyName = string.IsNullOrWhiteSpace(property.Name) ? "Property" : property.Name;
        var statusLabel = newStatus.ToString();

        (string guestSubject, string ownerSubject, string guestPlain, string ownerPlain, string? guestHtml, string? ownerHtml) data =
            newStatus switch
            {
                BookingStatus.Confirmed => (
                    guestSubject: $"Booking confirmed • {propertyName}",
                    ownerSubject: $"You confirmed a booking • {propertyName}",
                    guestPlain: BuildPlainTextEmail(
                        title: "Booking confirmed",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerPlain: BuildPlainTextEmail(
                        title: "Booking confirmed",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    guestHtml: BuildBookingEmailHtml(
                        title: "Booking confirmed",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerHtml: BuildBookingEmailHtml(
                        title: "Booking confirmed",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice)
                ),
                BookingStatus.Rejected => (
                    guestSubject: $"Booking rejected • {propertyName}",
                    ownerSubject: $"You rejected a booking • {propertyName}",
                    guestPlain: BuildPlainTextEmail(
                        title: "Booking rejected",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerPlain: BuildPlainTextEmail(
                        title: "Booking rejected",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    guestHtml: BuildBookingEmailHtml(
                        title: "Booking rejected",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerHtml: BuildBookingEmailHtml(
                        title: "Booking rejected",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice)
                ),
                BookingStatus.Cancelled => (
                    guestSubject: $"Booking cancelled • {propertyName}",
                    ownerSubject: $"A booking was cancelled • {propertyName}",
                    guestPlain: BuildPlainTextEmail(
                        title: "Booking cancelled",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerPlain: BuildPlainTextEmail(
                        title: "Booking cancelled",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    guestHtml: BuildBookingEmailHtml(
                        title: "Booking cancelled",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerHtml: BuildBookingEmailHtml(
                        title: "Booking cancelled",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice)
                ),
                BookingStatus.Completed => (
                    guestSubject: $"Booking completed • {propertyName}",
                    ownerSubject: $"Booking completed • {propertyName}",
                    guestPlain: BuildPlainTextEmail(
                        title: "Booking completed",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerPlain: BuildPlainTextEmail(
                        title: "Booking completed",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        statusLabel: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    guestHtml: BuildBookingEmailHtml(
                        title: "Booking completed",
                        greetingName: guestUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice),
                    ownerHtml: BuildBookingEmailHtml(
                        title: "Booking completed",
                        greetingName: ownerUser?.FirstName,
                        propertyName: propertyName,
                        status: statusLabel,
                        start: start,
                        end: end,
                        nights: nights,
                        guestCount: booking.GuestCount,
                        totalPrice: booking.TotalPrice)
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Unsupported booking status")
            };

        if (guestUser != null)
        {
            await emailSender.SendAsync(
                guestUser.Email,
                data.guestSubject,
                data.guestPlain,
                htmlContent: data.guestHtml,
                cancellationToken: cancellationToken);
        }

        if (ownerUser != null)
        {
            await emailSender.SendAsync(
                ownerUser.Email,
                data.ownerSubject,
                data.ownerPlain,
                htmlContent: data.ownerHtml,
                cancellationToken: cancellationToken);
        }
    }

    private static string BuildPlainTextEmail(
        string title,
        string? greetingName,
        string propertyName,
        string statusLabel,
        string start,
        string end,
        int nights,
        int guestCount,
        decimal totalPrice)
    {
        var name = string.IsNullOrWhiteSpace(greetingName) ? "there" : greetingName;
        var price = totalPrice.ToString("0.00");

        return
$@"{title}

Hi {name},

Property: {propertyName}
Dates: {start} → {end} ({nights} night{(nights == 1 ? "" : "s")})
Guests: {guestCount}
Status: {statusLabel}
Total: {price}

— BookingApi";
    }

    private static string BuildBookingEmailHtml(
        string title,
        string? greetingName,
        string propertyName,
        string status,
        string start,
        string end,
        int nights,
        int guestCount,
        decimal totalPrice)
    {
        var name = string.IsNullOrWhiteSpace(greetingName) ? "there" : greetingName;
        var price = totalPrice.ToString("C");

        var (accentColor, badgeBg, badgeText, statusIcon, statusMessage) = status switch
        {
            "Confirmed" => ("#16a34a", "#dcfce7", "#15803d", "&#10003;",  "Your reservation is confirmed. We look forward to welcoming you!"),
            "Rejected"  => ("#dc2626", "#fee2e2", "#b91c1c", "&#10007;",  "Unfortunately your booking request was not accepted. Please try different dates or another property."),
            "Cancelled" => ("#6b7280", "#f3f4f6", "#374151", "&#8854;",   "This booking has been cancelled. We hope to see you again soon."),
            "Completed" => ("#2563eb", "#dbeafe", "#1d4ed8", "&#9733;",   "Your stay is complete. Thank you for booking with us!"),
            _           => ("#0f172a", "#f1f5f9", "#0f172a", "&#8226;",   "Your booking status has been updated.")
        };

        return $"""
               <!doctype html>
               <html lang="en">
               <head>
                 <meta charset="UTF-8" />
                 <meta name="viewport" content="width=device-width, initial-scale=1" />
                 <title>{WebUtility.HtmlEncode(title)}</title>
               </head>
               <body style="margin:0; padding:0; background:#f1f5f9; -webkit-font-smoothing:antialiased;">

                 <!--[if mso]><table width="100%" cellpadding="0" cellspacing="0"><tr><td><![endif]-->
                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9; margin:0; padding:0;">
                   <tr>
                     <td align="center" style="padding:40px 16px;">

                       <!-- Card -->
                       <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;">

                         <!-- Logo / brand bar -->
                         <tr>
                           <td style="padding-bottom:20px;">
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                               <tr>
                                 <td style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:18px; font-weight:800; color:#0f172a; letter-spacing:-0.3px;">
                                   &#127968;&nbsp; BookingApp
                                 </td>
                                 <td align="right" style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:12px; color:#64748b;">
                                   Booking notification
                                 </td>
                               </tr>
                             </table>
                           </td>
                         </tr>

                         <!-- Accent header -->
                         <tr>
                           <td style="background:{WebUtility.HtmlEncode(accentColor)}; border-radius:16px 16px 0 0; padding:28px 32px 24px 32px;">
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                               <tr>
                                 <td>
                                   <!-- Status badge -->
                                   <span style="display:inline-block; background:rgba(255,255,255,0.22); color:#fff; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:700; letter-spacing:0.08em; text-transform:uppercase; padding:4px 12px; border-radius:999px;">
                                     {statusIcon}&nbsp;&nbsp;{WebUtility.HtmlEncode(status)}
                                   </span>
                                   <h1 style="margin:14px 0 6px 0; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:26px; font-weight:800; color:#ffffff; line-height:1.2; letter-spacing:-0.4px;">
                                     {WebUtility.HtmlEncode(title)}
                                   </h1>
                                   <p style="margin:0; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:15px; color:rgba(255,255,255,0.85); line-height:1.5;">
                                     Hi {WebUtility.HtmlEncode(name)}, {WebUtility.HtmlEncode(statusMessage)}
                                   </p>
                                 </td>
                               </tr>
                             </table>
                           </td>
                         </tr>

                         <!-- White card body -->
                         <tr>
                           <td style="background:#ffffff; border-radius:0 0 16px 16px; padding:28px 32px 32px 32px; box-shadow:0 8px 32px rgba(15,23,42,0.10);">

                             <!-- Property highlight -->
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                               <tr>
                                 <td style="background:#f8fafc; border-left:4px solid {WebUtility.HtmlEncode(accentColor)}; border-radius:0 12px 12px 0; padding:16px 20px;">
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:600; letter-spacing:0.08em; text-transform:uppercase; color:#94a3b8; margin-bottom:4px;">Property</div>
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:18px; font-weight:800; color:#0f172a;">
                                     {WebUtility.HtmlEncode(propertyName)}
                                   </div>
                                 </td>
                               </tr>
                             </table>

                             <!-- Details grid -->
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                               <tr>
                                 <!-- Check-in -->
                                 <td width="48%" style="background:#f8fafc; border-radius:12px; padding:16px 18px; vertical-align:top;">
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:600; letter-spacing:0.08em; text-transform:uppercase; color:#94a3b8; margin-bottom:6px;">&#128197;&nbsp; Check-in</div>
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:16px; font-weight:700; color:#0f172a;">{WebUtility.HtmlEncode(start)}</div>
                                 </td>
                                 <td width="4%"></td>
                                 <!-- Check-out -->
                                 <td width="48%" style="background:#f8fafc; border-radius:12px; padding:16px 18px; vertical-align:top;">
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:600; letter-spacing:0.08em; text-transform:uppercase; color:#94a3b8; margin-bottom:6px;">&#128197;&nbsp; Check-out</div>
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:16px; font-weight:700; color:#0f172a;">{WebUtility.HtmlEncode(end)}</div>
                                 </td>
                               </tr>
                             </table>

                             <!-- Nights & guests row -->
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:24px;">
                               <tr>
                                 <td width="48%" style="background:#f8fafc; border-radius:12px; padding:16px 18px; vertical-align:top;">
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:600; letter-spacing:0.08em; text-transform:uppercase; color:#94a3b8; margin-bottom:6px;">&#127769;&nbsp; Duration</div>
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:16px; font-weight:700; color:#0f172a;">{nights} night{(nights == 1 ? "" : "s")}</div>
                                 </td>
                                 <td width="4%"></td>
                                 <td width="48%" style="background:#f8fafc; border-radius:12px; padding:16px 18px; vertical-align:top;">
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:11px; font-weight:600; letter-spacing:0.08em; text-transform:uppercase; color:#94a3b8; margin-bottom:6px;">&#128101;&nbsp; Guests</div>
                                   <div style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:16px; font-weight:700; color:#0f172a;">{guestCount} guest{(guestCount == 1 ? "" : "s")}</div>
                                 </td>
                               </tr>
                             </table>

                             <!-- Divider -->
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-bottom:20px;">
                               <tr><td style="border-top:1px solid #e2e8f0; font-size:0; line-height:0;">&nbsp;</td></tr>
                             </table>

                             <!-- Total price -->
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                               <tr>
                                 <td style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:14px; color:#64748b; vertical-align:middle;">
                                   Total amount
                                 </td>
                                 <td align="right">
                                   <span style="display:inline-block; background:{WebUtility.HtmlEncode(badgeBg)}; color:{WebUtility.HtmlEncode(badgeText)}; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:22px; font-weight:900; padding:8px 18px; border-radius:10px; letter-spacing:-0.5px;">
                                     {WebUtility.HtmlEncode(price)}
                                   </span>
                                 </td>
                               </tr>
                             </table>

                           </td>
                         </tr>

                         <!-- Footer -->
                         <tr>
                           <td align="center" style="padding-top:24px; font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:12px; color:#94a3b8; line-height:1.6;">
                             This is an automated message — please do not reply to this email.<br/>
                             &copy; {DateTime.UtcNow.Year} BookingApp. All rights reserved.
                           </td>
                         </tr>

                       </table>
                     </td>
                   </tr>
                 </table>
                 <!--[if mso]></td></tr></table><![endif]-->

               </body>
               </html>
               """;
    }
}