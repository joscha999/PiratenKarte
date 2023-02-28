namespace PiratenKarte.Shared.Validation;

public class ErrorBag {
    private readonly Dictionary<string, List<ErrorContainer>> Errors = new();

    public bool AnyError => Errors.Count > 0;

    public IEnumerable<ErrorContainer> Get(string key) {
        if (Errors.TryGetValue(key, out var list))
            return list;

        return Enumerable.Empty<ErrorContainer>();
    }

    public void Clear() => Errors.Clear();

    public bool IsError(string key) => Errors.ContainsKey(key);

    public void Fail(string key, string message) {
        if (!Errors.TryGetValue(key, out var list)) {
            list = new List<ErrorContainer>();
            Errors.Add(key, list);
        }

        list.Add(new ErrorContainer {
            Message = message,
            Level = "danger"
        });
    }
}

public class ErrorContainer {
    public required string Message { get; init; }
    public required string Level { get; init; }
}