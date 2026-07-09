using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.UI.Paging;

namespace Nop.Web.Models.Catalog;

/// <summary>
/// Represents a model to get the catalog products
/// </summary>
public partial record CatalogProductsCommand : BasePageableModel 
{
    #region Properties

    /// <summary>
    /// Gets or sets the price ('min-max' format)
    /// </summary>
    public string Price { get; set; }

    /// <summary>
    /// Gets or sets the specification attribute option ids
    /// </summary>
    public List<int> Specs { get; set; }

    /// <summary>
    /// Gets or sets the manufacturer ids
    /// </summary>
    public List<int> Ms { get; set; }

    /// <summary>
    /// Gets or sets a order by
    /// </summary>
    public int? OrderBy { get; set; }

    /// <summary>
    /// Gets or sets a product sorting
    /// </summary>
    public string ViewMode { get; set; }
    //by wafaa 25-6

   
    public bool InStockOnly { get; set; }
    public bool DiscountOnly { get; set; }


    #endregion
}