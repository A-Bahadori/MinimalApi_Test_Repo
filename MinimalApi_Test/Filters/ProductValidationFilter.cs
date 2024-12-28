using MinimalApi_Test.Models;

namespace MinimalApi_Test.Filters
{
    public class ProductValidationFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var product = context.GetArgument<Product>(0);
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                return await Task.FromResult(Results.BadRequest("نام محصول اجباری است"));
            }

            return await next(context);
        }
    }
}
