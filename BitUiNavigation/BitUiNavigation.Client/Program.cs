using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.Modal.Abstract;
using BitUiNavigation.Client.Pages.Modal.Providers;
using BitUiNavigation.Client.Pages.UserProfile;
using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.State;
namespace BitUiNavigation.Client;

public class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        ConfigureCommonServices(builder.Services);
        await builder.Build().RunAsync();
    }
    public static void ConfigureCommonServices(IServiceCollection services)
    {
        services.AddBitBlazorUIServices();
        services.AddTimeWarpState();
        services.AddScoped<IModalProviderSource, DefaultModalProviderSource>();
        //services.AddScoped<IModalProvider, UserModalProvider>();
        services.AddKeyedTransient<IModalProvider, UserModalProvider>("User");
        services.AddValidatorsFromAssemblyContaining<UserProfileModelValidator>();
        services.AddScoped<UserService>();
    }
}
