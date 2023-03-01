using PiratenKarte.DAL;
using PiratenKarte.Server;
using System.Text.Json;

if (!File.Exists("settings.json")) {
	File.WriteAllText("settings.json", JsonSerializer.Serialize(Settings.Default));

	Console.WriteLine("No settings.json found, writing default. Please set values as required!");
	return;
}

var builder = WebApplication.CreateBuilder(args);

var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json")) ?? Settings.Default;

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton(new DB(settings.DbFileName, settings.AdminPassword));
builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddSingleton(settings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseWebAssemblyDebugging();
} else {
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
