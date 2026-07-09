using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Customers;

namespace NopStation.Plugin.Misc.Core.Services;

public class NopStationCustomerService : CustomerService, INopStationCustomerService
{
    public NopStationCustomerService(
    CustomerSettings customerSettings,
    IEventPublisher eventPublisher,
    IGenericAttributeService genericAttributeService,
    INopDataProvider dataProvider,
    IRepository<Address> customerAddressRepository,
    IRepository<BlogComment> blogCommentRepository,
    IRepository<Customer> customerRepository,
    IRepository<CustomerAddressMapping> customerAddressMappingRepository,
    IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
    IRepository<CustomerPassword> customerPasswordRepository,
    IRepository<CustomerRole> customerRoleRepository,
    IRepository<ForumPost> forumPostRepository,
    IRepository<ForumTopic> forumTopicRepository,
    IRepository<GenericAttribute> gaRepository,
    IRepository<NewsComment> newsCommentRepository,
    IRepository<Order> orderRepository,
    IRepository<ProductReview> productReviewRepository,
    IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
    IRepository<PollVotingRecord> pollVotingRecordRepository,
    IRepository<ShoppingCartItem> shoppingCartRepository,
    IShortTermCacheManager shortTermCacheManager,
    IStaticCacheManager staticCacheManager,
    IStoreContext storeContext,
    ShoppingCartSettings shoppingCartSettings,
    TaxSettings taxSettings) : base(
        customerSettings,
        eventPublisher,
        genericAttributeService,
        dataProvider,
        customerAddressRepository,
        blogCommentRepository,
        customerRepository,
        customerAddressMappingRepository,
        customerCustomerRoleMappingRepository,
        customerPasswordRepository,
        customerRoleRepository,
        forumPostRepository,
        forumTopicRepository,
        gaRepository,
        newsCommentRepository,
        orderRepository,
        productReviewRepository,
        productReviewHelpfulnessRepository,
        pollVotingRecordRepository,
        shoppingCartRepository,
        shortTermCacheManager,
        staticCacheManager,
        storeContext,
        shoppingCartSettings,
        taxSettings
    )
    {
    }

    public Task<string> FormatCustomerNameAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var name = string.Empty;
        if (!string.IsNullOrEmpty(customer.FirstName))
            name = customer.FirstName;

        if (!string.IsNullOrEmpty(customer.LastName))
        {
            if (!string.IsNullOrEmpty(name))
                name += " ";

            name += customer.LastName;
        }

        if (!string.IsNullOrEmpty(customer.Email))
        {
            if (!string.IsNullOrEmpty(name))
                name += " ~ ";

            name += customer.Email;
        }

        return Task.FromResult(name);
    }

    public async Task<IPagedList<Customer>> GetCustomersAsync(string q = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var registeredRole = await GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName) ?? throw new NopException("'Registered' role could not be loaded");

        var query = from c in _customerRepository.Table
                    join cr in _customerCustomerRoleMappingRepository.Table on c.Id equals cr.CustomerId
                    where cr.CustomerRoleId == registeredRole.Id
                    where !c.Deleted && (showHidden || c.Active) &&
                        (string.IsNullOrEmpty(q) || c.Email.Contains(q) || c.FirstName.Contains(q) || c.LastName.Contains(q) || (c.FirstName + " " + c.LastName).Contains(q))
                    select c;

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }
}
