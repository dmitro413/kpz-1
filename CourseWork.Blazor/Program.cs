using CourseWork.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7011/")
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSingleton<HubConnection>(sp => {
    return new HubConnectionBuilder()
        .WithUrl("https://localhost:7011/shopHub")
        .WithAutomaticReconnect()
        .Build();
});

await builder.Build().RunAsync();
