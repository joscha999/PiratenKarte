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
    public UserDTO? GetSelf() {
        var userId = GetUserId();
        if (userId == null)
            return null;

        return Mapper.Map<UserDTO>(DB.UserRepo.Get(userId.Value));
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
    [Permission("users_update")]
    public bool Update(UserData request) {
        if (string.IsNullOrEmpty(request.User.Username))
            return false;
        if (DB.UserRepo.GetByUsername(request.User.Username) != null)
            return false;

        var user = DB.UserRepo.Get(request.User.Id);
        user.Username = request.User.Username;

        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = PasswordHashser.Hash(request.Password);

        DB.UserRepo.Update(user);
        return true;
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