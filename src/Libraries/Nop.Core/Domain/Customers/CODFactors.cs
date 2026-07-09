using Nop.Core;

namespace Nop.Core.Domain.Customers;

public class CODFactors : BaseEntity
{
    public int CountryID { get; set; }
    public string Name { get; set; }
    public decimal CODFactor { get; set; }
    public int FactorID { get; set; }
}
