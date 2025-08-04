using Bit.BlazorUI;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BitUiNavigation.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddBitBlazorUIServices();

            await builder.Build().RunAsync();
        }
    }
}
