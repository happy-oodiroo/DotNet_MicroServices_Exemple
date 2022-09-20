using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StudentWebPortal.Client;

namespace StudentWebPortal.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddScoped(sp => new StudentInfo.ApiClient.StudentInfoApiServices("http://localhost:8080", sp.GetRequiredService<HttpClient>()));
            builder.Services.AddScoped(sp => new DiplomaAndCertification.ApiClient.DiplomaAndCertificationApiServices("http://localhost:8080", sp.GetRequiredService<HttpClient>()));
            builder.Services.AddLocalStorageServices();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthentificationStateProvider>();
            builder.Services.AddAuthorizationCore();
            var app = builder.Build();
            await app.RunAsync();
        }
    }
}