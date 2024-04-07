namespace PiratenKarte.Shared.RequestModels;

public record CreateGroupResponse(bool NameTaken, Guid? Id);