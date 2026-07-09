using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using NopStation.Plugin.Misc.Core.Models;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Controllers;

public class Select2Controller : NopStationAdminController
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IManufacturerService _manufacturerService;
    private readonly INopStationCustomerService _customerService;

    public Select2Controller(ICategoryService categoryService,
        IProductService productService,
        IManufacturerService manufacturerService,
        INopStationCustomerService customerService)
    {
        _categoryService = categoryService;
        _productService = productService;
        _manufacturerService = manufacturerService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Products(string q, int page = 1)
    {
        var products = await _productService.SearchProductsAsync(
            showHidden: true,
            keywords: q,
            pageIndex: page - 1,
            pageSize: 10);

        var response = new Select2ResponseModel();
        foreach (var product in products)
        {
            response.Results.Add(new Select2ResponseModel.Select2Item
            {
                Id = product.Id,
                Text = product.Name,
            });
        }

        response.Pagination.More = products.HasNextPage;

        return Json(response);
    }

    public async Task<IActionResult> Categories(string q, int page = 1)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(
            showHidden: true,
            categoryName: q,
            pageIndex: page - 1,
            pageSize: 10);

        var response = new Select2ResponseModel();
        foreach (var category in categories)
        {
            response.Results.Add(new Select2ResponseModel.Select2Item
            {
                Id = category.Id,
                Text = await _categoryService.GetFormattedBreadCrumbAsync(category),
            });
        }

        response.Pagination.More = categories.HasNextPage;

        return Json(response);
    }

    public async Task<IActionResult> Manufacturers(string q, int page = 1)
    {
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(
            showHidden: true,
            manufacturerName: q,
            pageIndex: page - 1,
            pageSize: 10);

        var response = new Select2ResponseModel();
        foreach (var manufacturer in manufacturers)
        {
            response.Results.Add(new Select2ResponseModel.Select2Item
            {
                Id = manufacturer.Id,
                Text = manufacturer.Name,
            });
        }

        response.Pagination.More = manufacturers.HasNextPage;

        return Json(response);
    }

    public async Task<IActionResult> Customers(string q, int page = 1)
    {
        var customers = await _customerService.GetCustomersAsync(
            showHidden: true,
            q: q,
            pageIndex: page - 1,
            pageSize: 10);

        var response = new Select2ResponseModel();
        foreach (var customer in customers)
        {
            response.Results.Add(new Select2ResponseModel.Select2Item
            {
                Id = customer.Id,
                Text = await _customerService.FormatCustomerNameAsync(customer),
            });
        }

        response.Pagination.More = customers.HasNextPage;

        return Json(response);
    }
}
