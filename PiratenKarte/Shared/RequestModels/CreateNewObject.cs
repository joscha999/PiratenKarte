namespace PiratenKarte.Shared.RequestModels;

public class CreateNewObject {
    public required MapObjectDTO Object { get; set; }
    public Guid? StorageId { get; set; }
}

public class CreateNewObjectBulk {
    public required MapObjectDTO Template { get; set; }
    public Guid? StorageId { get; set; }
    public int Count { get; set; }
    public Guid GroupId { get; set; }
}