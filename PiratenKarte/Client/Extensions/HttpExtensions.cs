using OneOf;
using PiratenKarte.Shared.Unions;
using System.Text.Json;

namespace PiratenKarte.Client.Extensions;

public static class HttpExtensions {
    public static JsonSerializerOptions? JsonSerializerOptions { get; set; }

    public static async Task<OneOf<Empty, HttpStatus, Unauthorized, T>>ReadResultAsync<T>(
        this HttpResponseMessage message) {
        if (message.StatusCode == System.Net.HttpStatusCode.NoContent)
            return new Empty();
        if (message.StatusCode != System.Net.HttpStatusCode.OK)
            return new HttpStatus(message.StatusCode);

        var content = await message.Content.ReadAsStringAsync();
        Console.WriteLine(content);
        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions)!;
    }
}