using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class UserRepository : RepositoryBase<User> {
    public override string CollectionName => "Users";

    public UserRepository(DB db) : base(db) { }

    internal override ILiteQueryable<User> Includes(ILiteQueryable<User> query) => query;
    internal override ILiteCollection<User> Includes(ILiteCollection<User> query) => query;

    public User? GetByUsername(string username)
        => Col.Query().Where(u => u.Username == username).SingleOrDefault();

    public void AddDefaultUser() {
        var id = Guid.Parse("4717a5e3-f722-4cd3-b08b-bcd8c4e8ca26");
        var user = Get(id);

        if (user != null)
            return;

        user = new User {
            Id = id,
            Username = "Admin",
            PasswordHash = PasswordHashser.Hash("fmiTnJj3sV6Nf@L^^m6ykp2RVUCh)^lU")
        };

        user.Permissions = DB.PermissionRepo.GetAll().ToList();
        Insert(user);
    }

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
}