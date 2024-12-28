using Microsoft.AspNetCore.Authorization;
using MinimalApi_Test.Filters;
using MinimalApi_Test.Models;
using MinimalApi_Test.Services;

namespace MinimalApi_Test.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var app = endpoints;
        var productsGroup = app.MapGroup("/api/products");

        #region Get

        productsGroup.MapGet("", (ProductService service) =>
            service.GetAll());

        productsGroup.MapGet("{id:int}", GetProductById).WithName("GetProductById");

        productsGroup.MapGet("/search", (ProductService service, string? name, decimal? minPrice, decimal? maxPrice) =>
        {
            var products = service.GetAll();

            if (!string.IsNullOrEmpty(name))
                products = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            return Results.Ok(products);
        });

        #endregion

        #region Post

        productsGroup.MapPost("/", [Authorize] (Product product, ProductService service) =>
        {
            service.Add(product);
            return Results.Created($"/GetProductById/{product.Id}", product);
        }).AddEndpointFilter<ProductValidationFilter>();

        #endregion

        #region Put

        productsGroup.MapPut("{id:int}", (Product updatedProduct, ProductService service, int id) =>
        {
            var success = service.Update(id, updatedProduct);
            return success ? Results.Ok(updatedProduct) : Results.NotFound("محصول پیدا نشد");
        }).RequireAuthorization()
            .AddEndpointFilter<ProductValidationFilter>();

        #endregion

        #region Delete

        productsGroup.MapDelete("{id:int}", (ProductService service, int id) =>
        {
            var success = service.Delete(id);
            return success ? Results.Ok("محصول حذف شد") : Results.NotFound("محصول پیدا نشد");
        }).RequireAuthorization(policy => policy.RequireClaim("role", "Admin"));

        #endregion

        #region Utilities

        IResult GetProductById(ProductService service, int id)
        {
            var product = service.GetById(id);
            return product is not null ? Results.Ok(product) : Results.NotFound("محصول پیدا نشد");
        }

        #endregion
    }
}