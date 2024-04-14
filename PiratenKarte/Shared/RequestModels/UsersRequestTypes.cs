namespace PiratenKarte.Shared.RequestModels;

public record CreateUserResponse(bool Created, bool Taken, Guid Id);
public record SetUserGroupRequest(Guid UserId, Guid GroupId, bool Applied);
public record UpdateUserResponse(bool Updated, bool Taken);