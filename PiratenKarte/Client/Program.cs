using FisSst.BlazorMaps.DependencyInjection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PiratenKarte.Client;
using PiratenKarte.Client.Models;
using PiratenKarte.Client.Services;
using Blazored.Modal;
using PiratenKarte.Client.Extensions;
using PiratenKarte.Shared.Unions;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorLeafletMaps();
builder.Services.AddBlazorDownloadFile();
builder.Services.AddSingleton<ParameterPassService>();
builder.Services.AddBlazoredModal();
builder.Services.AddLocalStorageServices();
builder.Services.AddGeolocationServices();

var options = new JsonSerializerOptions();
options.Converters.Add(new OneOfJsonConverter());
options.Converters.Add(new OneOfJsonConverterFactory());

HttpExtensions.JsonSerializerOptions = options;
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

builder.Services.AddSingleton<AppStateService>();
builder.Services.AddSingleton<AuthenticationStateService>();
builder.Services.AddSingleton(new AppSettings {
    Domain = await http.GetStringAsync("Settings/GetDomain")
});

await builder.Build().RunAsync();