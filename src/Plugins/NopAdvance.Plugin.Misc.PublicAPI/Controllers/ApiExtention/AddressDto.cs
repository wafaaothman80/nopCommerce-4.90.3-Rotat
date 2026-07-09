using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopAdvance.Plugin.Misc.PublicAPI.Controllers.ApiExtention;
public record AddressDto
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string Company { get; init; }
    public string Address1 { get; init; }
    public string Address2 { get; init; }
    public string City { get; init; }                 // ensure not empty
    public int? StateProvinceId { get; init; }        // canonical region
    public string StateProvinceName { get; init; }
    public int? CountryId { get; init; }
    public string CountryName { get; init; }
    public string ZipPostalCode { get; init; }
    public string PhoneNumber { get; init; }
    public string CustomAddressAttributes { get; init; }
    
    public bool IsDefaultBilling { get; init; }
    public bool IsDefaultShipping { get; init; }
}

public record CustomerAddressListDto
{
    public IList<AddressDto> Addresses { get; init; } = new List<AddressDto>();
}
