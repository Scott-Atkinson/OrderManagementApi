using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Order_Management.Application.Common.Behaviours;
using Order_Management.Application.Common.Interfaces;
using Order_Management.Application.Common.Services;

namespace Order_Management.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddScoped<IOrderNumberService, OrderNumberService>();

        return services;
    }
}
