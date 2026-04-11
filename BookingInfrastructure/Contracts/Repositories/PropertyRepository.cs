namespace BookingInfrastructure.Contracts.Repositories;

using BookingDomain.Entities;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Features.Properties.GetPropertyDetails;
using BookingApplication.Features.Properties.SearchProperties;
using Microsoft.EntityFrameworkCore;

public class PropertyRepository : IPropertyRepository
{
    private readonly BookingDbContext _context;

    public PropertyRepository(BookingDbContext context)
    {
        _context = context;
    }
    
    // IRepository<Properties> methods
    public async Task<Properties?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Properties.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Properties> Add(Properties entity, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Properties> Update(Properties entity, CancellationToken cancellationToken = default)
    {
        _context.Properties.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Properties entity, CancellationToken cancellationToken = default)
    {
        _context.Properties.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<SearchPropertiesResponse> Search(SearchPropertiesRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var startDate = request.StartDate?.Date;
        var endDate = request.EndDate?.Date;
        var hasDates = startDate.HasValue && endDate.HasValue;

        var amenityInts = request.Amenities?.Where(a => a > 0).Distinct().ToList() ?? new List<int>();

        IQueryable<Properties> baseQuery = _context.Properties
            .AsNoTracking()
            .Include(p => p.Address);

        // Only return publicly visible properties in search.
        baseQuery = baseQuery.Where(p => p.IsActive && p.IsApproved);

        if (!string.IsNullOrWhiteSpace(request.PropertyType))
            baseQuery = baseQuery.Where(p => p.PropertyType == request.PropertyType);

        if (request.Guests.HasValue)
            baseQuery = baseQuery.Where(p => p.MaxGuests >= request.Guests.Value);

        if (request.MinPricePerDay.HasValue)
            baseQuery = baseQuery.Where(p => p.PricePerDay >= request.MinPricePerDay.Value);

        if (request.MaxPricePerDay.HasValue)
            baseQuery = baseQuery.Where(p => p.PricePerDay <= request.MaxPricePerDay.Value);

        if (!string.IsNullOrWhiteSpace(request.Country))
            baseQuery = baseQuery.Where(p => p.Address.Country.Contains(request.Country));

        if (!string.IsNullOrWhiteSpace(request.City))
            baseQuery = baseQuery.Where(p => p.Address.City.Contains(request.City));

        if (!string.IsNullOrWhiteSpace(request.ZipCode))
            baseQuery = baseQuery.Where(p => p.Address.ZipCode.Contains(request.ZipCode));

        if (amenityInts.Count > 0)
        {
            baseQuery = baseQuery.Where(p => p.Amenities.Any(a => amenityInts.Contains((int)a)));
        }

        if (hasDates)
        {
            var s = startDate!.Value;
            var e = endDate!.Value;

            baseQuery = baseQuery.Where(p => !_context.Bookings.Any(b =>
                b.PropertyId == p.Id &&
                (b.BookingStatus == BookingStatus.Pending || b.BookingStatus == BookingStatus.Confirmed) &&
                s < b.EndDate && e > b.StartDate));
        }

        // Rating aggregation (live)
        var ratingsQuery =
            from r in _context.Reviews.AsNoTracking()
            join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
            group r by b.PropertyId
            into g
            select new
            {
                PropertyId = g.Key,
                AvgRating = g.Average(x => (double)x.Rating),
                ReviewCount = g.Count()
            };

        var query =
            from p in baseQuery
            join ra in ratingsQuery on p.Id equals ra.PropertyId into raGroup
            from ra in raGroup.DefaultIfEmpty()
            select new
            {
                Property = p,
                AvgRating = (double?)ra.AvgRating,
                ReviewCount = ra == null ? 0 : ra.ReviewCount
            };

        if (request.MinRating.HasValue)
        {
            var min = request.MinRating.Value;
            query = query.Where(x => x.AvgRating.HasValue && x.AvgRating.Value >= min);
        }

        query = request.Sort switch
        {
            PropertySearchSort.PriceDesc => query.OrderByDescending(x => x.Property.PricePerDay).ThenBy(x => x.Property.Id),
            PropertySearchSort.RatingDesc => query
                .OrderByDescending(x => x.AvgRating ?? -1)
                .ThenByDescending(x => x.ReviewCount)
                .ThenBy(x => x.Property.PricePerDay)
                .ThenBy(x => x.Property.Id),
            PropertySearchSort.PopularityDesc => query
                .OrderByDescending(x => x.Property.LastBookedOnUtc ?? DateTime.MinValue)
                .ThenBy(x => x.Property.Id),
            _ => query.OrderBy(x => x.Property.PricePerDay).ThenBy(x => x.Property.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PropertySearchResultDto
            {
                Id = x.Property.Id,
                Name = x.Property.Name,
                PropertyType = x.Property.PropertyType,
                PricePerDay = x.Property.PricePerDay,
                CleaningFreePerDay = x.Property.CleaningFreePerDay,
                MaxGuests = x.Property.MaxGuests,
                IsActive = x.Property.IsActive,
                Country = x.Property.Address.Country,
                City = x.Property.Address.City,
                ZipCode = x.Property.Address.ZipCode,
                Amenities = x.Property.Amenities,
                ImageUrls = x.Property.ImageUrls,
                AverageRating = x.AvgRating,
                ReviewCount = x.ReviewCount
            })
            .ToListAsync(cancellationToken);

        return new SearchPropertiesResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<PropertyDetailsDto?> GetDetails(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var property = await _context.Properties
            .AsNoTracking()
            .Include(p => p.Address)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive && p.IsApproved, cancellationToken);

        if (property == null)
            return null;

        var ratingAgg =
            await (from r in _context.Reviews.AsNoTracking()
                   join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
                   where b.PropertyId == propertyId
                   group r by b.PropertyId
                into g
                   select new
                   {
                       AvgRating = (double?)g.Average(x => (double)x.Rating),
                       ReviewCount = g.Count()
                   }).FirstOrDefaultAsync(cancellationToken);

        var bookedRanges = await _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.PropertyId == propertyId &&
                (b.BookingStatus == BookingStatus.Pending || b.BookingStatus == BookingStatus.Confirmed))
            .OrderBy(b => b.StartDate)
            .Select(b => new PropertyDetailsDto.BookedRangeDto
            {
                StartDate = b.StartDate.Date,
                EndDate = b.EndDate.Date,
                BookingStatus = b.BookingStatus
            })
            .ToListAsync(cancellationToken);

        var recentReviews =
            await (from r in _context.Reviews.AsNoTracking()
                   join b in _context.Bookings.AsNoTracking() on r.BookingId equals b.Id
                   join u in _context.Users.AsNoTracking() on r.GuestId equals u.Id
                   where b.PropertyId == propertyId
                   orderby r.CreatedAt descending
                   select new PropertyDetailsDto.ReviewPreviewDto
                   {
                       Rating = r.Rating,
                       Comment = r.Comment,
                       CreatedAt = r.CreatedAt,
                       GuestId = r.GuestId,
                       GuestName = (u.FirstName + " " + u.LastName).Trim()
                   })
                .Take(10)
                .ToListAsync(cancellationToken);

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

        var amenitiesUpChargePerNight = property.Amenities?.Sum(GetAmenitySurchargePerNight) ?? 0m;

        return new PropertyDetailsDto
        {
            Id = property.Id,
            OwnerId = property.OwnerId,
            Name = property.Name,
            Description = property.Description,
            PropertyType = property.PropertyType,
            PricePerDay = property.PricePerDay,
            CleaningFreePerDay = property.CleaningFreePerDay,
            MaxGuests = property.MaxGuests,
            CheckInTime = property.CheckInTime,
            CheckOutTime = property.CheckOutTime,
            IsActive = property.IsActive,
            Address = new PropertyDetailsDto.AddressDto
            {
                Country = property.Address.Country,
                City = property.Address.City,
                Street = property.Address.Street,
                ZipCode = property.Address.ZipCode
            },
            Amenities = property.Amenities,
            ImageUrls = property.ImageUrls,
            AverageRating = ratingAgg?.AvgRating,
            ReviewCount = ratingAgg?.ReviewCount ?? 0,
            BookedRanges = bookedRanges,
            RecentReviews = recentReviews,
            PricingRules = new PropertyDetailsDto.PricingRulesDto
            {
                PricePerDay = property.PricePerDay,
                CleaningFeePerDay = property.CleaningFreePerDay,
                AmenitiesUpChargePerNight = amenitiesUpChargePerNight
            }
        };
    }
}
