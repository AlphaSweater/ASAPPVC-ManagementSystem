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
                        // Check for ValidateWithViewAttribute first
                        var viewName = ctx.ActionDescriptor.EndpointMetadata
                            .OfType<ValidateWithViewAttribute>()
                            .FirstOrDefault()?.ViewName;

                        // Fallback to TempData or action name
                        viewName ??= c.TempData["ValidationViewName"] as string
                            ?? ctx.RouteData.Values["action"]?.ToString();

                        ctx.Result = c.View(viewName, arg);
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

    /// <summary>
    /// Specifies the view name to use when validation fails in an action.
    /// Used by ValidationActionFilter to return the correct view with model errors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateWithViewAttribute : Attribute
    {
        public string ViewName { get; }

        public ValidateWithViewAttribute(string viewName)
        {
            ViewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
        }
    }
}