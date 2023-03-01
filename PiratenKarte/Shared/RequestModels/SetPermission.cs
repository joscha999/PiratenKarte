namespace PiratenKarte.Shared.RequestModels;

public class SetPermission {
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool State { get; set; }
}