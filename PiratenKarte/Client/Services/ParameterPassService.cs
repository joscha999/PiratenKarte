using System.Diagnostics.CodeAnalysis;

namespace PiratenKarte.Client.Services;

public class ParameterPassService {
    private readonly Dictionary<string, object?> Data = new Dictionary<string, object?>();

    public void Put<T>(string key, T value) => Data[key] = value;

    public bool TryTake<T>(string key, [NotNullWhen(true)] out T? value) {
        if (!Data.TryGetValue(key, out var obj)) {
            value = default;
            return false;
        }

        if (obj is not T t) {
            value = default;
            return false;
        }

        Data.Remove(key);
        value = t;
        return true;
    }

    public bool Has<T>(string key) => Data.TryGetValue(key, out var obj) && obj is T;
}