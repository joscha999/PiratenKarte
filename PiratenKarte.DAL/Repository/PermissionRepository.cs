using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class PermissionRepository : RepositoryBase<Permission> {
    public override string CollectionName => "Permissions";

    public PermissionRepository(DB db) : base(db) { }

    internal override ILiteQueryable<Permission> Includes(ILiteQueryable<Permission> query) => query;
    internal override ILiteCollection<Permission> Includes(ILiteCollection<Permission> query) => query;

    public Permission? GetByName(string name) => Col.FindOne(p => p.Key == name);

    public void AddDeaultPermissions() {
        InsertOrUpdate("objects_create", "Objekte/Erstellen");
        InsertOrUpdate("objects_read", "Objekte/Ansehen");
        InsertOrUpdate("objects_update", "Objekte/Editieren");
        InsertOrUpdate("objects_delete", "Objekte/Löschen");

        InsertOrUpdate("objects_comments_create", "Objekte/Kommentare/Erstellen");
        InsertOrUpdate("objects_comments_read", "Objekte/Kommentare/Ansehen");
        InsertOrUpdate("objects_comments_update", "Objekte/Kommentare/Editieren");
        InsertOrUpdate("objects_comments_delete", "Objekte/Kommentare/Löschen");

        InsertOrUpdate("settings_create", "Einstellungen/Erstellen");
        InsertOrUpdate("settings_update", "Einstellungen/Editieren");
        InsertOrUpdate("settings_delete", "Einstellungen/Löschen");

        InsertOrUpdate("storagedefinitions_create", "Lager/Erstellen");
        InsertOrUpdate("storagedefinitions_read", "Lager/Ansehen");
        InsertOrUpdate("storagedefinitions_update", "Lager/Editieren");
        InsertOrUpdate("storagedefinitions_delete", "Lager/Löschen");

        InsertOrUpdate("users_create", "Benutzer/Erstellen");
        InsertOrUpdate("users_read", "Benutzer/Ansehen");
        InsertOrUpdate("users_update", "Benutzer/Editieren");
        InsertOrUpdate("users_delete", "Benutzer/Löschen");

        InsertOrUpdate("permissions_update", "Berechtigung/Editieren");

        InsertOrUpdate("groups_create", "Gruppen/Erstellen");
        InsertOrUpdate("groups_read", "Gruppen/Ansehen");
        InsertOrUpdate("groups_update", "Gruppen/Editieren");
        InsertOrUpdate("groups_delete", "Gruppen/Löschen");
        InsertOrUpdate("groups_add_user", "Gruppen/Benutzer Hinzufügen");

        InsertOrUpdate("groups_add_self", "Gruppen/Selbst Hinzufügen");

        InsertOrUpdate("markerstyles_create", "Marker Stile/Eerstellen");
        InsertOrUpdate("markerstyles_read", "Marker Stile/Ansehen");
        InsertOrUpdate("markerstyles_update", "Marker Stile/Editieren");
        InsertOrUpdate("markerstyles_delete", "Marker Stile/Löschen");

        InsertOrUpdate("log_read", "Logs/Ansehen");
    }

    private void InsertOrUpdate(string name, string readableName) {
        var p = GetByName(name);

        if (p != null) {
            p.ReadableName = readableName;
            Update(p);
        } else {
            Insert(new Permission {
                Key = name,
                ReadableName = readableName
            });
        }
    }
}