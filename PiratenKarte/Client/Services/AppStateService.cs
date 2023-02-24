using Microsoft.JSInterop;
using PiratenKarte.Client.Models;

namespace PiratenKarte.Client.Services;

public class AppStateService {
    public const string StorageKey = "state";

    private readonly ILocalStorageService Storage;

    public AppState Current { get; set; }

    public AppStateService(ILocalStorageService storage) {
		Storage = storage;

        Current = storage.GetItem<AppState>(StorageKey) ?? new();
	}

    public void Write() {
        if (Current.StoreStateLocally) {
            Storage.SetItem(StorageKey, Current);
        } else {
            Storage.RemoveItem(StorageKey);
        }
    }
}