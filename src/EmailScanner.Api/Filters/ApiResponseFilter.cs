using EmailScanner.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EmailScanner.Api.Filters;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } result)
        {
            var type = result.Value.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(FeatureResult<>))
            {
                var status = (int)type.GetProperty(nameof(FeatureResult<object>.StatusCode))!.GetValue(result.Value)!;
                context.Result = new ObjectResult(result.Value) { StatusCode = status };
            }
        }
        await next();
    }
}
