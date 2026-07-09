using System.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.AccountManager.Domain;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Plugin.AccountManager.Models;
using System.Collections.Generic;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;

namespace Nop.Plugin.AccountManager.Services;

/// <summary>
/// Customer service
/// </summary>
public partial class RigionService : IRigionService
{
  

    protected readonly CustomerSettings _customerSettings;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly INopDataProvider _dataProvider;
    protected readonly IRepository<Address> _customerAddressRepository;
    protected readonly IRepository<BlogComment> _blogCommentRepository;
    protected readonly IRepository<Nop.Plugin.AccountManager.Domain.Rigion> _rigionRepository;
    protected readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
    protected readonly IRepository<CountryRigionMapping> _CountryRigionMappingRepository;
    protected readonly IRepository<CustomerPassword> _customerPasswordRepository;
    protected readonly IRepository<Country> _countryRepository;
    protected readonly IRepository<ForumPost> _forumPostRepository;
    protected readonly IRepository<ForumTopic> _forumTopicRepository;
    protected readonly IRepository<GenericAttribute> _gaRepository;
    protected readonly IRepository<NewsComment> _newsCommentRepository;
    protected readonly IRepository<Order> _orderRepository;
    protected readonly IRepository<ProductReview> _productReviewRepository;
    protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    protected readonly IRepository<PollVotingRecord> _pollVotingRecordRepository;
    protected readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly ShoppingCartSettings _shoppingCartSettings;
    protected readonly TaxSettings _taxSettings;
    protected readonly ICountryService _countryService;


    public RigionService(CustomerSettings customerSettings,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        INopDataProvider dataProvider,
        IRepository<Address> customerAddressRepository,
        IRepository<BlogComment> blogCommentRepository,
        IRepository<Nop.Plugin.AccountManager.Domain.Rigion> rigionRepository,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<CountryRigionMapping> CountryRigionMappingRepository,
        IRepository<CustomerPassword> customerPasswordRepository,
        IRepository<Country> countryRepository,
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
        TaxSettings taxSettings, ICountryService countryService)
    {
        _customerSettings = customerSettings;
        _eventPublisher = eventPublisher;
        _genericAttributeService = genericAttributeService;
        _dataProvider = dataProvider;
        _customerAddressRepository = customerAddressRepository;
        _blogCommentRepository = blogCommentRepository;
        _rigionRepository = rigionRepository;
        _customerAddressMappingRepository = customerAddressMappingRepository;
        _CountryRigionMappingRepository = CountryRigionMappingRepository;
        _customerPasswordRepository = customerPasswordRepository;
        _countryRepository = countryRepository;
        _forumPostRepository = forumPostRepository;
        _forumTopicRepository = forumTopicRepository;
        _gaRepository = gaRepository;
        _newsCommentRepository = newsCommentRepository;
        _orderRepository = orderRepository;
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _pollVotingRecordRepository = pollVotingRecordRepository;
        _shoppingCartRepository = shoppingCartRepository;
        _shortTermCacheManager = shortTermCacheManager;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _shoppingCartSettings = shoppingCartSettings;
        _taxSettings = taxSettings;
        _countryService = countryService;
    }



    /// <summary>
    /// Gets a dictionary of all customer roles mapped by ID.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation and contains a dictionary of all customer roles mapped by ID.
    /// </returns>
    protected virtual async Task<IDictionary<int, Country>> GetAllCountryRigionDictionaryAsync()
    {
        return await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Country>.AllCacheKey),
            async () => await _countryRepository.Table.ToDictionaryAsync(cr => cr.Id));
    }



    


    public virtual async Task PrepareModelCountryRigionAsync<TModel>(TModel model) where TModel : ISupportedRigionModel
    {
        ArgumentNullException.ThrowIfNull(model);

       // var countries = await _countryService.GetAllCountriesAsync()
        var availableRoles = GetAllCountriessAsync(showHidden: true);
        model.AvailableRigionCountries = availableRoles.Select(country => new SelectListItem
        {
            Text = country.Name,
            Value = country.Id.ToString(),
            Selected = model.SelectedCountryIds.Contains(country.Id)
        }).ToList();
    }

  


    public virtual async Task<IPagedList<Nop.Plugin.AccountManager.Domain.Rigion>> GetAllRigionsAsync(int[] countryIds = null,
        string RigionName = null,

        int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
    {
        var rigions = await _rigionRepository.GetAllPagedAsync(query =>
        {
           

          

            if (countryIds != null && countryIds.Length > 0)
            {
                query = query.Join(_CountryRigionMappingRepository.Table, x => x.Id, y => y.RigionId,
                        (x, y) => new { Rigirn = x, Mapping = y })
                    .Where(z => countryIds.Contains(z.Mapping.CountryId))
                    .Select(z => z.Rigirn)
                    .Distinct();
            }

            if (!string.IsNullOrWhiteSpace(RigionName))
                query = query.Where(c => c.RigionName.Contains(RigionName));
          
           

           

            

            query = query.OrderByDescending(c => c.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return rigions;
    }




    public virtual async Task DeleteRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion)
    {
        ArgumentNullException.ThrowIfNull(rigion);



        rigion.Active = false;

       

        await _rigionRepository.UpdateAsync(rigion, false);
        await _rigionRepository.DeleteAsync(rigion);
    }

   
    public virtual async Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByIdAsync(int rigionId)
    {
        return await _rigionRepository.GetByIdAsync(rigionId, cache => default, useShortTermCache: true);
    }

    public virtual async Task<Nop.Plugin.AccountManager.Domain.Rigion> GetRigionByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _rigionRepository.Table
                    orderby c.Id
                    where c.RigionName == name
                    select c;
        var _Rigion = await query.FirstOrDefaultAsync();

        return _Rigion;
    }



    public virtual async Task<IList<Nop.Plugin.AccountManager.Domain.Rigion>> GetRigionsByIdsAsync(int[] rigionIds)
    {
        return await _rigionRepository.GetByIdsAsync(rigionIds, includeDeleted: false);
    }

    public virtual async Task InsertRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion)
    {
        await _rigionRepository.InsertAsync(rigion);
    }

    public virtual async Task UpdateRigionAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion)
    {
        await _rigionRepository.UpdateAsync(rigion);
    }

   
 

    #region manager region

    public async Task AddrigionMappingAsync(CountryRigionMapping roleMapping)
    {
        await _CountryRigionMappingRepository.InsertAsync(roleMapping);
    }

   
    public async Task RemoverigionMappingAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, Country country)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        ArgumentNullException.ThrowIfNull(country);

        var mapping = await _CountryRigionMappingRepository.Table
            .SingleOrDefaultAsync(ccrm => ccrm.RigionId == rigion.Id && ccrm.CountryId == country.Id);

        if (mapping != null)
            await _CountryRigionMappingRepository.DeleteAsync(mapping);
    }
  



    public virtual async Task <List<int>> GetCountryIdsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        return (await GetCountriesAsync(rigion, showHidden: showHidden))
            .Select(cr => cr.Id).ToList();
            //.ToArray();
    }



    public virtual async Task<IList<Country>> GetCountriesAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        var allRolesById = await GetAllCountriesDictionaryAsync();

        var mappings = await _CountryRigionMappingRepository.GetAllAsync(query => query.Where(crm => crm.RigionId == rigion.Id));

        return mappings.Select(mapping => allRolesById.TryGetValue(mapping.CountryId, out var country) ? country : null)
            .Where(cr => cr != null && (showHidden || cr.Published))
            .ToList();
    }



    public virtual async Task<int[]> GetCountryRigionsIdsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        return (await GetCountryRigionsAsync(rigion, showHidden: showHidden))
            .Select(cr => cr.Id)
            .ToArray();
    }


  
    public virtual async Task<IList<Country>> GetCountryRigionsAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        var allCountriesById = await GetAllCountriesDictionaryAsync();

        var mappings = await _CountryRigionMappingRepository.GetAllAsync(query => query.Where(crm => crm.RigionId == rigion.Id));

        return mappings.Select(mapping => allCountriesById.TryGetValue(mapping.CountryId, out var rigion) ? rigion : null)
            .Where(cr => cr != null && (showHidden || cr.Published))
            .ToList();
    }




    protected virtual async Task<IDictionary<int, Country>> GetAllCountriesDictionaryAsync()
    {
        return await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<Country>.AllCacheKey),
            async () => await _countryRepository.Table.ToDictionaryAsync(cr => cr.Id));
    }



    //public async Task RemoverigionMappingAsync(Account_Manager accountManager, Rigion rigion)
    //{
    //    ArgumentNullException.ThrowIfNull(accountManager);

    //    ArgumentNullException.ThrowIfNull(rigion);

    //    var mapping = await _CountryRigionMappingRepository.Table
    //        .SingleOrDefaultAsync(ccrm => ccrm.AccountManagerId == accountManager.Id && ccrm.RigionId == rigion.Id);

    //    if (mapping != null)
    //        await _CountryRigionMappingRepository.DeleteAsync(mapping);
    //}
    public  Task RemoveCountryRigionMappingAsync(Nop.Plugin.AccountManager.Domain.Rigion rigion, Country country)
    {
        ArgumentNullException.ThrowIfNull(rigion);

        ArgumentNullException.ThrowIfNull(country);

        var mapping =  _CountryRigionMappingRepository.Table
            .SingleOrDefaultAsync(ccrm => ccrm.RigionId == rigion.Id && ccrm.CountryId == country.Id);

        if (mapping != null)
             _CountryRigionMappingRepository.DeleteAsync(mapping.Result);
        return Task.CompletedTask;
    }



    //public Task DeleteRigionAsync(Rigion rigion) => _countryRepository.DeleteAsync(rigion);


   
    //public Task<Rigion> GetRigionByIdAsync(int rigionId)
    //{
    //    var allRolesById =  GetAllRigionsDictionaryAsync();

    //    return (Task<Rigion>)allRolesById.Result.Where(x=>x.Key== rigionId);
    //}

 


    public IList<Country> GetAllCountriessAsync(bool showHidden = false)
    {
        IList<Country> allCountrysById = GetAllCountriesDictionaryAsync().Result.Values.Where(x => x.Published == true).ToList();

        return allCountrysById;
    }
   
    
    //  public Task InsertRigionAsync(Rigion rigion) => _countryRepository.InsertAsync(rigion);





    // public Task UpdateRigionAsync(Rigion rigion) => _countryRepository.UpdateAsync(rigion);



    //public virtual async Task<Account_Manager> GetAccountManagerByEmailAsync(string email)
    //{
    //    if (string.IsNullOrWhiteSpace(email))
    //        return null;

    //    var query = from c in _rigionRepository.Table
    //                orderby c.Id
    //                where c.Email == email
    //                select c;
    //    var _accountManager = await query.FirstOrDefaultAsync();

    //    return _accountManager;
    //}




    #endregion






}