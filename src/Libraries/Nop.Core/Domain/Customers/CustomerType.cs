using Nop.Core;

namespace Nop.Core.Domain.Customers;

public class CustomerType : BaseEntity
{
    public string TypeName { get; set; }
    public decimal Factor { get; set; }
}
