using System.Net.Http.Json;

namespace PiratenKarte.Client.Extensions;

public static class HttpExtensions {
    public static async Task<T?> ReadResultAsync<T>(this HttpResponseMessage message) {
        if (message.StatusCode == System.Net.HttpStatusCode.NoContent)
            return default;

        return await message.Content.ReadFromJsonAsync<T>();
    }
}