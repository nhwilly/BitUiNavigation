using BitUiNavigation.Client.Features.UserProfile;
using BitUiNavigation.Client.Features.UserProfile.Profile;
using BitUiNavigation.Client.Features.UserProfile.Provider;
using BitUiNavigation.Client.ModalHost.Abstract;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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
        //services.AddScoped<IModalProviderSource, DefaultModalProviderSource>();
        //services.AddScoped<IModalProvider, UserModalProvider>();
        services.AddKeyedTransient<IModalProvider, UserModalProvider>("User");
        services.AddValidatorsFromAssemblyContaining<UserProfileModelValidator>();
        services.AddScoped<UserService>();
    }
}
