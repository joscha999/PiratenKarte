using Microsoft.JSInterop;
using PiratenKarte.Client.Models;

namespace PiratenKarte.Client.Services;

public class AppStateService {
    public const string StorageKey = "state";

    private readonly ILocalStorageService Storage;

    public AppState Current { get; set; }

    public AppStateService(ILocalStorageService storage, HttpClient http) {
		Storage = storage;

        Current = storage.GetItem<AppState>(StorageKey) ?? new();

        if (!string.IsNullOrEmpty(Current.AuthToken)) {
            http.DefaultRequestHeaders.Add("authtoken", Current.AuthToken);
            http.DefaultRequestHeaders.Add("userid", Current.User?.Id.ToString());
        }
    }

    public void Write() {
        if (Current.StoreStateLocally) {
            Storage.SetItem(StorageKey, Current.SerializableClone());
        } else {
            Storage.RemoveItem(StorageKey);
        }
    }
}