using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Helpers;

public static class AppServices
{
    public static IServiceProvider Provider { get; set; } = null!;

    public static T GetRequiredService<T>() where T : notnull
    {
        return Provider.GetRequiredService<T>();
    }
}