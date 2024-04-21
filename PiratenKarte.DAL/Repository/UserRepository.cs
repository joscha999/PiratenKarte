using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class UserRepository : RepositoryBase<User> {
    private readonly Guid AdminUserGuid = Guid.Parse("4717a5e3-f722-4cd3-b08b-bcd8c4e8ca26");

    public override string CollectionName => "Users";

    public UserRepository(DB db) : base(db) { }

    internal override ILiteQueryable<User> Includes(ILiteQueryable<User> query) => query;
    internal override ILiteCollection<User> Includes(ILiteCollection<User> query) => query;

    public User? GetWithPermissions(Guid id) => Col.Include(u => u.Permissions).FindById(id);

    public User? GetByUsername(string username)
        => Col.Query().Where(u => u.Username == username).SingleOrDefault();

    public IEnumerable<Permission> GetUserPermissions(Guid userId) {
        var user = Col.Include(u => u.Permissions).FindById(userId);
        if (user == null)
            yield break;

        foreach (var p in user.Permissions)
            yield return p;
    }

    public bool HasPermission(Guid userId, string name) {
        var permission = DB.PermissionRepo.GetByName(name);
        if (permission == null)
            return false;

        return Col.FindOne(u => u.Id == userId && u.Permissions.Contains(permission)) != null;
    }

    internal void AddDefaultAdmin(string? password) {
        var id = AdminUserGuid;
        var user = Get(id);

        if (user != null)
            return;

        user = new User {
            Id = id,
            Username = "Admin",
            PasswordHash = password == null ? null : PasswordHashser.Hash(password)
        };

        user.Permissions = DB.PermissionRepo.GetAll().ToList();
        user.GroupIds = DB.GroupRepo.GetAll().Select(g => g.Id).ToList();
        Insert(user);
    }

    internal void UpdateAdminPermissionsAndGroups() {
        var user = Get(AdminUserGuid);
        if (user == null)
            return;

        user.Permissions.Clear();
        user.Permissions.AddRange(DB.PermissionRepo.GetAll());

        user.GroupIds.Clear();
        user.GroupIds.AddRange(DB.GroupRepo.GetAll().Select(g => g.Id));

        Update(user);
    }
}