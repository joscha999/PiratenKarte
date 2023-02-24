namespace PiratenKarte.Shared.RequestModels;

public class CreateNewObject {
    public required MapObject Object { get; set; }
    public Guid? StorageId { get; set; }
}

public class CreateNewObjectBulk {
    public required MapObject Template { get; set; }
    public Guid? StorageId { get; set; }
    public int Count { get; set; }
}