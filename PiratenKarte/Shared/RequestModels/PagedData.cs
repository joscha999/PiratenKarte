namespace PiratenKarte.Shared.RequestModels;

public class PagedData<T> {
    public required List<T> Data { get; set; }

    public int TotalCount { get; set; }
}