using Microsoft.JSInterop;
using PiratenKarte.Client.Models;
using PiratenKarte.Shared.ResponseModels;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace PiratenKarte.Client.Services;

public class AuthenticationStateService {
    public const string StorageKey = "login";
    private readonly ILocalStorageService Storage;
    private readonly HttpClient Http;

    private readonly Dictionary<string, bool> PermissionCache = new();

    public AuthState Current { get; private set; }

    public AuthenticationStateService(ILocalStorageService storage, HttpClient http) {
        Storage = storage;
        Http = http;

        Current = storage.GetItem<AuthState>(StorageKey) ?? new();

        if (!string.IsNullOrEmpty(Current.Token)) {
            http.DefaultRequestHeaders.Add("authtoken", Current.Token);
            http.DefaultRequestHeaders.Add("userid", Current.User?.Id.ToString());
        }
    }

    public void Write() {
        Http.DefaultRequestHeaders.Add("authtoken", Current.Token);
        Http.DefaultRequestHeaders.Add("userid", Current.User?.Id.ToString());

        if (Current.KeepLoggedIn) {
            Storage.SetItem(StorageKey, Current);
        } else {
            Storage.RemoveItem(StorageKey);
        }
    }

    public async Task Invalidate() {
        await Http.GetAsync("Users/InvalidateToken");

        Http.DefaultRequestHeaders.Remove("authtoken");
        Http.DefaultRequestHeaders.Remove("userid");

        Storage.RemoveItem(StorageKey);
        Current = new();
        PermissionCache.Clear();
    }

    public LoginState GetLoginState() {
        if (string.IsNullOrEmpty(Current.Token) || Current.User == null)
            return LoginState.None;

        return Current.TokenValidTill > DateTime.UtcNow ? LoginState.LoggedIn : LoginState.TokenInvalid;
    }

    public bool HasExact(string key) {
        if (!HasInternal())
            return false;

        if (!PermissionCache.TryGetValue(key, out var has)) {
            has = Current.Permissions.Any(p => p.Key == key);
            PermissionCache[key] = has;
        }

        return has;
    }

    public bool HasPattern(string pattern) {
        if (!HasInternal())
            return false;

        if (!PermissionCache.TryGetValue(pattern, out var has)) {
            has = Current.Permissions.Any(p => Regex.IsMatch(p.Key, pattern));
            PermissionCache[pattern] = has;
        }

        return has;
    }

    private bool HasInternal() {
        if (Current.Permissions.Count == 0)
            return false;

        return GetLoginState() == LoginState.LoggedIn;
    }
}

public enum LoginState {
    None,
    LoggedIn,
    TokenInvalid
}