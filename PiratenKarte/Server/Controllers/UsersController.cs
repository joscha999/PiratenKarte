using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.ResponseModels;
using PiratenKarte.Shared.Unions;
using UserDTO = PiratenKarte.Shared.UserDTO;

namespace PiratenKarte.Server.Controllers;
public class UsersController : CrudController<UserDTO, User> {
    public override string PermissionBaseName => "users";

    protected override RepositoryBase<User> Repository => DB.UserRepo;

    public UsersController(DB db, IMapper mapper) : base(db, mapper) { }

    [NonAction]
    public override Guid Create(UserDTO item) => throw new InvalidOperationException();
    [NonAction]
    public override void Update(UserDTO item) => throw new InvalidOperationException();

    [HttpGet]
    [EnsureLoggedIn]
    public IActionResult GetSelf() {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        return Ok(Mapper.Map<UserDTO>(DB.UserRepo.Get(userId.Value)));
    }

    [HttpPost]
    public override void Delete([FromBody] Guid id) {
        var selfId = GetUserId();
        if (selfId == null || selfId == id)
            return;

        base.Delete(id);
    }

    [HttpPost]
    [Permission("users_create")]
    public IActionResult Create(UserData request) {
        if (string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.User.Username))
            return BadRequest();
        if (DB.UserRepo.GetByUsername(request.User.Username) != null)
            return Ok(new CreateUserResponse(false, true, Guid.Empty));

        var user = Mapper.Map<User>(request.User);
        user.PasswordHash = PasswordHashser.Hash(request.Password);

        return Ok(new CreateUserResponse(true, false, DB.UserRepo.Insert(user)));
    }

    [HttpPost]
    [EnsureLoggedIn]
    public IActionResult Update(UserData request) {
        if (string.IsNullOrEmpty(request.User.Username))
            return BadRequest();
        if (!TryGetUser(out var requestUser))
            return BadRequest();

        var dbUser = DB.UserRepo.Get(request.User.Id);
        if (dbUser == null)
            return NotFound();

        if (!HasPermissionOrIsSelf(requestUser, "users_update", dbUser))
            return Unauthorized();

            // Only check whether or not the username exists if we actually want to change it
        if (dbUser.Username != request.User.Username && DB.UserRepo.GetByUsername(request.User.Username) != null)
            return Ok(new UpdateUserResponse(false, true));

        dbUser.Username = request.User.Username;

        if (!string.IsNullOrEmpty(request.Password)) {
            dbUser.PasswordHash = PasswordHashser.Hash(request.Password);

            // When updating the Password invalidate all Tokens
            DB.TokenRepo.InvalidateAllForUser(dbUser.Id);
        }

        DB.UserRepo.Update(dbUser);
        return Ok(new UpdateUserResponse(true, false));
    }

    [HttpPost]
    public LoginResult? CheckLogin(LoginData request) {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return null;

        var user = DB.UserRepo.GetByUsername(request.Username);
        if (user == null) {
            //this is a random password, it is correct but useless
            //it is only here to have a similar delay as a real user/password check
            PasswordHashser.Verify("qD&H54YH!1ZUJZsBiclFdm7!)^7ksPdM",
                "E39C8BA752D2A79608FDEB44BBF4519899DADC16D6BB2ECE50C499C9EE02C8FFFB818A815F58F4A24EB1D97" +
                "36A2BE53677DA9A6EC5B2813A50D9BE7D443D238F;B804CC39805975589A2DCFB7A0E8C29B;500000;SHA512");

            return null;
        }

        if (user.PasswordHash == null || !PasswordHashser.Verify(request.Password, user.PasswordHash))
            return null;

        var token = new Token {
            Content = new string(Enumerable.Range(0, 32)
                .Select(_ => (char)Random.Shared.Next(32, 127)).ToArray()),
            Type = TokenType.Login,
            User = user,
            ValidTill = DateTime.UtcNow.AddDays(7)
        };

        DB.TokenRepo.Insert(token);
        return new LoginResult {
            Token = token.Content,
            ValidTill = token.ValidTill,
            User = Mapper.Map<UserDTO>(user),
            Permissions = DB.UserRepo.GetUserPermissions(user.Id).Select(Mapper.Map<Shared.PermissionDTO>).ToList()
        };
    }

    [HttpGet]
    public void InvalidateToken() {
        var userId = GetUserId();
        var token = GetAuthToken();

        if (userId == null || token == null)
            return;

        DB.TokenRepo.Invalidate(token, userId.Value);
    }
}