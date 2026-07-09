using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Shipping.ShipmentMergeDiscount.Services;

/// <summary>
/// Provides business logic for determining shipment-merge eligibility and calculating
/// the resulting shipping discount using the two-step formula:
///
///   AdditionalShipping = max(0, CombinedShipping − PreviousShipping)
///   Discount           = CurrentShipping − AdditionalShipping
///
/// Where:
///   PreviousShipping = shipping already paid on the previous eligible order(s)
///   CurrentShipping  = what the customer would pay if this order shipped separately
///   CombinedShipping = carrier rate for one consolidated shipment (all items together)
///
/// The max(0, ...) guard handles the edge case where the combined shipment qualifies
/// for free shipping (CombinedShipping &lt; PreviousShipping), giving the customer a full
/// discount on the current order.
/// </summary>
public interface IShipmentMergeDiscountService
{
    /// <summary>
    /// Returns true when the customer has at least the configured minimum number of
    /// pending, unshipped orders to the same shipping address.
    /// </summary>
    Task<bool> IsEligibleForMergeDiscountAsync(Customer customer, Address? shippingAddress, int storeId);

    /// <summary>
    /// Returns the count of eligible pending/unshipped orders for the customer
    /// at the given shipping address. Used for diagnostics.
    /// </summary>
    Task<int> GetEligibleOrderCountAsync(Customer customer, Address? shippingAddress, int storeId);

    /// <summary>
    /// Calculates the shipping consolidation discount.
    ///
    /// Algorithm:
    ///   1. Locate eligible previous orders (same customer, same address, not yet shipped).
    ///   2. PreviousShipping = sum of those orders' OrderShippingExclTax.
    ///   3. CurrentShipping  = <paramref name="currentShippingCost"/> (already calculated
    ///      by the normal shipping pipeline for this cart alone).
    ///   4. Build a combined package (current cart + previous-order items) and ask the
    ///      selected shipping-rate computation method for its rate → CombinedShipping.
    ///   5. AdditionalShipping = max(0, CombinedShipping − PreviousShipping)
    ///   6. Discount = CurrentShipping − AdditionalShipping
    ///
    /// Falls back to a configured percentage/fixed amount when the combined rate cannot
    /// be determined (e.g. no shipping option is selected yet).
    ///
    /// Persists the result in the customer's generic attributes for auditing.
    /// </summary>
    Task<decimal> CalculateMergeDiscountAsync(
        Customer customer,
        Address? shippingAddress,
        IList<ShoppingCartItem> currentCart,
        decimal currentShippingCost,
        int storeId);
}
