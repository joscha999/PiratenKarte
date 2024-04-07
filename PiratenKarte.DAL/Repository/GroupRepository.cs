using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class GroupRepository : RepositoryBase<Group> {
    public override string CollectionName => "Groups";

    public GroupRepository(DB db) : base(db) { }

    internal override ILiteQueryable<Group> Includes(ILiteQueryable<Group> query) => query;
    internal override ILiteCollection<Group> Includes(ILiteCollection<Group> query) => query;

    public IEnumerable<Group> GetForUser(User user)
        => Col.Query().Where(g => user.GroupIds.Contains(g.Id)).ToEnumerable();

    public Group? GetByName(string name) => Col.Query().Where(g => g.Name == name).FirstOrDefault();

    public void AddDefaultGroups() {
        InsertNew("Bundesverband");
        InsertNew("Landesverband Baden-Württemberg");
        InsertNew("Landesverband Bayern");
        InsertNew("Landesverband Berlin");
        InsertNew("Landesverband Brandeburg");
        InsertNew("Landesverband Bremen");
        InsertNew("Landesverband Hamburg");
        InsertNew("Landesverband Hessen");
        InsertNew("Landesverband Mecklenburg-Vorpommern");
        InsertNew("Landesverband Niedersachsen");
        InsertNew("Landesverband Nordrhein-Westfalen");
        InsertNew("Landesverband Rheinland-Pfalz");
        InsertNew("Landesverband Saarland");
        InsertNew("Landesverband Sachsen");
        InsertNew("Landesverband Sachsen-Anhalt");
        InsertNew("Landesverband Schleswig-Holstein");
        InsertNew("Landesverband Thüringen");
    }

    private void InsertNew(string name) {
        var p = GetByName(name);
        if (p != null)
            return;

        Insert(new Group {
            Id = Guid.NewGuid(),
            Name = name
        });
    }
}