using Microsoft.AspNetCore.Mvc;

namespace PiratenKarte.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SettingsController : Controller {
    private readonly Settings Settings;

    public SettingsController(Settings settings) {
        Settings = settings;
    }

    [HttpGet]
    public string GetDomain() => Settings.Domain;
}