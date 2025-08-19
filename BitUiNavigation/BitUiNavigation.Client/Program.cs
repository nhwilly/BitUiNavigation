using Bit.BlazorUI;
using BitUiNavigation.Client.Pages.Modals;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TimeWarp.State;
using Blazored.FluentValidation;
using FluentValidation;
using BitUiNavigation.Client.Pages.UserProfile;
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
        services.AddScoped<IModalProvider, UserModalProvider>();
        //services.AddScoped<IModalProvider, WorkspaceModalProvider>();
        services.AddValidatorsFromAssemblyContaining<UserProfileModelValidator>();
        services.AddScoped<UserService>();
    }
}
