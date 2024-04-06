using OneOf;

namespace PiratenKarte.Shared.Unions;

[GenerateOneOf]
public partial class UserCreateResponse : OneOfBase<IncompleteRequest, UserNameTaken, UserCreated>;