using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASAPPVC.App.Models.Validation
{
    /// Runs any IValidator<T> for action arguments automatically.
    public sealed class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _services;

        public ValidationActionFilter(IServiceProvider services)
        {
            _services = services;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
        {
            foreach (var arg in ctx.ActionArguments.Values)
            {
                if (arg is null)
                    continue;

                var vType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                if (_services.GetService(vType) is not IValidator validator)
                    continue;

                var vCtxType = typeof(FluentValidation.ValidationContext<>).MakeGenericType(arg.GetType());
                var vCtx = (IValidationContext)Activator.CreateInstance(vCtxType, arg)!;

                var result = await validator.ValidateAsync(vCtx, ctx.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    foreach (var e in result.Errors)
                        ctx.ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

                    if (ctx.Controller is Controller c)
                    {
                        ctx.Result = c.View(ctx.RouteData.Values["action"]?.ToString(), arg);
                    }
                    else
                    {
                        ctx.Result = new BadRequestObjectResult(ctx.ModelState);
                    }
                    return;
                }
            }

            await next();
        }
    }
}