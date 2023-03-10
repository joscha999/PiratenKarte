using System.Net;

namespace PiratenKarte.Shared.Unions;

//generic
public record struct IncompleteRequest;
public record struct Empty;

//specific
public record struct HttpStatus(HttpStatusCode Code);
public record struct Unauthorized();

public record struct UserNameTaken;
public readonly record struct UserCreated(Guid Guid);