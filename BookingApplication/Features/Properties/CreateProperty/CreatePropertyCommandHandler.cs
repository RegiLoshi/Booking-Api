using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.CreateProperty;

public class CreatePropertyCommandHandler(IPropertyRepository _propertyRepository, IAddressRepository _addressRepository)
    : IRequestHandler<CreatePropertyCommand, Guid>
{
    public async Task<Guid> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreatePropertyDto;

        var existingAddressId = await _addressRepository.FindIdByStreetCityZipCountry(
            dto.address.Country, dto.address.City, dto.address.Street, dto.address.ZipCode, cancellationToken);

        Guid addressId;
        if (existingAddressId.HasValue)
        {
            addressId = existingAddressId.Value;
        }
        else
        {
            var address = new Address
            {
                Id = Guid.NewGuid(),
                Country = dto.address.Country,
                City = dto.address.City,
                Street = dto.address.Street,
                ZipCode = dto.address.ZipCode
            };
            var addedAddress = await _addressRepository.Add(address, cancellationToken);
            addressId = addedAddress.Id;
        }

        var property = new BookingDomain.Entities.Properties
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Name = dto.Name,
            Description = dto.Description,
            PropertyType = dto.PropertyType,
            PricePerDay = dto.PricePerDay,
            CleaningFreePerDay = dto.CleaningFreePerDay,
            AddressId = addressId,
            MaxGuests = dto.MaxGuests,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            IsActive = dto.IsActive,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            Amenities = dto.Amenities ?? new List<Amenity>(),
            ImageUrls = dto.ImageUrls ?? new List<string>()
        };

        await _propertyRepository.Add(property, cancellationToken);
        return property.Id;
    }
}