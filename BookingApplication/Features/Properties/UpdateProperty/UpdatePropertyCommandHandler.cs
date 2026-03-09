using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingDomain.Entities;

namespace BookingApplication.Features.Properties.UpdateProperty;

public class UpdatePropertyCommandHandler(
    IPropertyRepository propertyRepository,
    IAddressRepository addressRepository)
    : IRequestHandler<UpdatePropertyCommand, bool>
{
    public async Task<bool> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await propertyRepository.GetById(request.PropertyId, cancellationToken);
        if (property == null)
            return false;

        if (property.OwnerId != request.OwnerId)
            return false;

        var dto = request.UpdatePropertyDto;

        property.Name = dto.Name;
        property.Description = dto.Description;
        property.PropertyType = dto.PropertyType;
        property.MaxGuests = dto.MaxGuests;
        property.CheckInTime = dto.CheckInTime;
        property.CheckOutTime = dto.CheckOutTime;
        property.IsActive = dto.IsActive;
        property.Amenities = dto.Amenities ?? new List<Amenity>();
        property.ImageUrls = dto.ImageUrls ?? new List<string>();
        property.LastModifiedAt = DateTime.UtcNow;

        if (dto.Address != null)
        {
            var existingAddressId = await addressRepository.FindIdByStreetCityZipCountry(
                dto.Address.Country, dto.Address.City, dto.Address.Street, dto.Address.ZipCode, cancellationToken);

            if (existingAddressId.HasValue)
            {
                property.AddressId = existingAddressId.Value;
            }
            else
            {
                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    Country = dto.Address.Country,
                    City = dto.Address.City,
                    Street = dto.Address.Street,
                    ZipCode = dto.Address.ZipCode
                };
                var added = await addressRepository.Add(address, cancellationToken);
                property.AddressId = added.Id;
            }
        }

        await propertyRepository.Update(property, cancellationToken);
        return true;
    }
}
