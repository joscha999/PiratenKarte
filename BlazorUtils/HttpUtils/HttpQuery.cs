using System.Net;
using System.Net.Http.Json;

namespace BlazorUtils.HttpUtils;

public class HttpQuery<T> {
    protected readonly HttpClient Http;

    private HttpMethod Method = HttpMethod.Get;
    protected string? Url;

    private Action<HttpStatusCode>? StatusCodeAction;
    private Action? UnauthorizedAction;
    private Action? SuccessAction;
    private Action? BeforeExecuteAction;
    private Action? AfterExecuteAction;
    private Action<T?>? ModelAction;

    private IRequestValueHandler RequestValueHandler = new EmptyRequestValueHandler();
    private IResponseValueHandler<T> ResponseValueHandler = (IResponseValueHandler<T>)new EmptyResponseValueHandler();

    internal HttpQuery(HttpClient http) {
        Http = http;
    }

    public async Task ExecuteAsync() {
        if (string.IsNullOrEmpty(Url))
            throw new ArgumentNullException(nameof(Url));

        BeforeExecuteAction?.Invoke();

        var request = new HttpRequestMessage(Method, Url);
        if (RequestValueHandler != null)
            request.Content = RequestValueHandler.BuildContent();

        var response = await Http.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.OK && ResponseValueHandler != null && ModelAction != null) {
            ModelAction.Invoke(await ResponseValueHandler.GetResponseAsync(response.Content));
            AfterExecuteAction?.Invoke();
            return;
        }

        if (response.StatusCode == HttpStatusCode.OK && SuccessAction != null) {
            SuccessAction.Invoke();
            AfterExecuteAction?.Invoke();
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized && UnauthorizedAction != null) {
            UnauthorizedAction.Invoke();
            AfterExecuteAction?.Invoke();
            return;
        }

        if (response.StatusCode != HttpStatusCode.OK && StatusCodeAction != null) {
            StatusCodeAction.Invoke(response.StatusCode);
            AfterExecuteAction?.Invoke();
            return;
        }

        throw new InvalidOperationException("No valid target for StatusCode " + response.StatusCode);
    }

    public HttpQuery<T> AsMethod(HttpMethod method) {
        Method = method;
        return this;
    }

    public HttpQuery<T> To(string url) {
        Url = url;
        return this;
    }

    public HttpQuery<T> WithJsonRequestValue<TRequest>(TRequest value) {
        RequestValueHandler = new JsonRequestValueHandler<TRequest>(value);
        return this;
    }

    public HttpQuery<T> WithJsonResponseValue() {
        ResponseValueHandler = new JsonResponseValueHandler<T>();
        return this;
    }

    public HttpQuery<T> OnStatusCode(Action<HttpStatusCode> action) {
        StatusCodeAction = action;
        return this;
    }

    public HttpQuery<T> OnUnauthorized(Action action) {
        UnauthorizedAction = action;
        return this;
    }

    public HttpQuery<T> OnSuccess(Action action) {
        SuccessAction = action;
        return this;
    }

    public HttpQuery<T> OnModel(Action<T?> action) {
        ModelAction = action;
        return this;
    }

    public HttpQuery<T> WithBeforeExecute(Action action) {
        BeforeExecuteAction = action;
        return this;
    }

    public HttpQuery<T> WithAfterExecute(Action action) {
        AfterExecuteAction = action;
        return this;
    }
}

public static class HttpQueryExtensions {
    public static HttpQuery<EmptyHttpResult> CreateGet(this HttpClient client)
        => new HttpQuery<EmptyHttpResult>(client);

    public static HttpQuery<TResponse> CreateGetJson<TResponse>(this HttpClient client)
        => new HttpQuery<TResponse>(client).WithJsonResponseValue();

    public static HttpQuery<EmptyHttpResult> CreatePost(this HttpClient client)
        => new HttpQuery<EmptyHttpResult>(client).AsMethod(HttpMethod.Post);

    public static HttpQuery<TResponse> CreatePostJson<TResponse>(this HttpClient client)
        => new HttpQuery<TResponse>(client).AsMethod(HttpMethod.Post).WithJsonResponseValue();

    public static HttpQuery<TResponse> CreatePostJson<TResponse, TRequest>(this HttpClient client, TRequest value)
        => new HttpQuery<TResponse>(client).AsMethod(HttpMethod.Post).WithJsonResponseValue().WithJsonRequestValue(value);
}

public sealed class EmptyHttpResult {
    public static EmptyHttpResult Instance { get; } = new();
    private EmptyHttpResult() { }
}

public interface IRequestValueHandler {
    HttpContent? BuildContent();
}

public class EmptyRequestValueHandler : IRequestValueHandler {
    public HttpContent? BuildContent() => null;
}

public class JsonRequestValueHandler<T> : IRequestValueHandler {
    private readonly T Value;

    public JsonRequestValueHandler(T value) {
        Value = value;
    }

    public HttpContent? BuildContent() => JsonContent.Create(Value);
}

public interface IResponseValueHandler<T> {
    Task<T?> GetResponseAsync(HttpContent content);
}

public class EmptyResponseValueHandler : IResponseValueHandler<EmptyHttpResult> {
    public Task<EmptyHttpResult?> GetResponseAsync(HttpContent content)
        => Task.FromResult<EmptyHttpResult?>(EmptyHttpResult.Instance);
}

public class JsonResponseValueHandler<T> : IResponseValueHandler<T> {
    public async Task<T?> GetResponseAsync(HttpContent content)
        => await content.ReadFromJsonAsync<T>();
}