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
        InsertNew("objects_create", "Objekte/Erstellen");
        InsertNew("objects_read", "Objekte/Ansehen");
        InsertNew("objects_update", "Objekte/Editieren");
        InsertNew("objects_delete", "Objekte/Löschen");

        InsertNew("objects_comments_create", "Objekte/Kommentare/Erstellen");
        InsertNew("objects_comments_read", "Objekte/Kommentare/Ansehen");
        InsertNew("objects_comments_update", "Objekte/Kommentare/Editieren");
        InsertNew("objects_comments_delete", "Objekte/Kommentare/Löschen");

        InsertNew("settings_create", "Einstellungen/Erstellen");
        InsertNew("settings_update", "Einstellungen/Editieren");
        InsertNew("settings_delete", "Einstellungen/Löschen");

        InsertNew("storagedefinitions_create", "Lager/Erstellen");
        InsertNew("storagedefinitions_read", "Lager/Ansehen");
        InsertNew("storagedefinitions_update", "Lager/Editieren");
        InsertNew("storagedefinitions_delete", "Lager/Löschen");

        InsertNew("users_create", "Benutzer/Erstellen");
        InsertNew("users_read", "Benutzer/Ansehen");
        InsertNew("users_update", "Benutzer/Editieren");
        InsertNew("users_delete", "Benutzer/Löschen");

        InsertNew("permissions_update", "Berechtigung/Editieren");

        InsertNew("groups_create", "Gruppen/Erstellen");
        InsertNew("groups_read", "Gruppen/Ansehen");
        InsertNew("groups_update", "Gruppen/Editieren");
        InsertNew("groups_delete", "Gruppen/Löschen");
        InsertNew("groups_add_user", "Gruppen/Benutzer Hinzufügen");
    }

    private void InsertNew(string name, string readableName) {
        var p = GetByName(name);
        if (p != null)
            return;

        Insert(new Permission {
            Key = name,
            ReadableName = readableName
        });
    }
}