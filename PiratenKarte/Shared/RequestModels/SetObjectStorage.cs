namespace PiratenKarte.Shared.RequestModels;

public class SetObjectStorage {
    public Guid ObjectId { get; set; }
    public Guid? StorageId { get; set; }
}

public class SetObjectStorageMany {
    public required List<Guid> ObjectIds { get; set; }
    public Guid? StorageId { get; set; }
}