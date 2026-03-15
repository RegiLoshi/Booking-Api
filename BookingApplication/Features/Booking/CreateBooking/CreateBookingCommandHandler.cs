using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Booking.CreateBooking;

public class CreateBookingCommandHandler(
    IBookingRepository bookingRepository,
    IPropertyRepository propertyRepository)
    : IRequestHandler<CreateBookingCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateBookingDto;

        if (dto.EndDate <= dto.StartDate)
        {
            throw new ArgumentException("EndDate must be after StartDate");
        }

        var property = await propertyRepository.GetById(dto.PropertyId, cancellationToken);
        if (property == null)
        {
            throw new ArgumentException("Property not found");
        }

        if (!property.IsActive)
        {
            throw new InvalidOperationException("Property is not available for booking");
        }

        if (dto.GuestCount <= 0 || dto.GuestCount > property.MaxGuests)
        {
            throw new ArgumentException("GuestCount is invalid for this property");
        }

        var nights = (dto.EndDate.Date - dto.StartDate.Date).Days;
        if (nights <= 0)
        {
            throw new ArgumentException("Stay must be at least one night");
        }

        var cleaningFee = property.CleaningFreePerDay * nights;
        var priceForPeriod = property.PricePerDay * nights;

        decimal GetAmenitySurchargePerNight(Amenity amenity) => amenity switch
        {
            Amenity.WiFi => 0m,
            Amenity.AirConditioning => 3m,
            Amenity.Parking => 2m,
            Amenity.PetFriendly => 4m,
            Amenity.SwimmingPool => 15m,
            Amenity.Gym => 5m,
            Amenity.Spa => 20m,
            Amenity.Terrace => 6m,
            Amenity.MountainView => 8m,
            Amenity.GardenView => 4m,
            _ => 0m
        };

        var amenitiesUpChargePerNight = property.Amenities?
            .Sum(a => GetAmenitySurchargePerNight(a)) ?? 0m;

        var amenitiesUpCharge = amenitiesUpChargePerNight * nights;
        var totalPrice = cleaningFee + priceForPeriod + amenitiesUpCharge;

        var now = DateTime.UtcNow;

        var booking = new Bookings
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            GuestId = request.GuestId,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            GuestCount = dto.GuestCount,
            CleaningFee = cleaningFee,
            AmenitiesUpCharge = amenitiesUpCharge,
            PriceForPeriod = priceForPeriod,
            TotalPrice = totalPrice,
            BookingStatus = BookingStatus.Pending,
            CreatedAt = now,
            LastModifiedAt = now,
            CreatedOnUtc = now,
            ConfirmedOnUtc = now
        };

        await bookingRepository.Add(booking, cancellationToken);
        return booking.Id;
    }
}

