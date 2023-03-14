using Microsoft.Extensions.DependencyInjection;

namespace BlogDesk.Helpers;

public static  class ServiceExtensions
{
    public static void AddEditorFactory<TEditor>(this IServiceCollection services)
        where TEditor : class
    {
        services.AddTransient<TEditor>();
        services.AddSingleton<Func<TEditor>>(x => () => x.GetService<TEditor>()!);
        services.AddSingleton<IAbstractFactory<TEditor>, AbstractFactory<TEditor>>();
    }
}
