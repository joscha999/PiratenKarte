using LiteDB;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;

namespace PiratenKarte.DAL;

public class DB {
    internal readonly LiteDatabase LDB;

	public readonly MapObjectRepository MapObjectRepo;
    public readonly StorageDefinitionRepository StorageDefinitionRepo;
    public readonly UserRepository UserRepo;
    public readonly TokenRepository TokenRepo;
    public readonly PermissionRepository PermissionRepo;

	public DB(string path, string? adminPassword) {
        BsonMapper.Global.Entity<MapObject>().DbRef(mo => mo.Storage, "StorageDefinitions");
        BsonMapper.Global.Entity<Token>().DbRef(t => t.User, "Users");

        BsonMapper.Global.RegisterType(
            obj => {
                var doc = new BsonDocument();
                doc["DateTime"] = obj.DateTime.Ticks;
                doc["Offset"] = obj.Offset.Ticks;
                return doc;
            },
            doc => new DateTimeOffset(doc["DateTime"].AsInt64, new TimeSpan(doc["Offset"].AsInt64))
        );

        LDB = new LiteDatabase(path);

		MapObjectRepo = new MapObjectRepository(this);
        StorageDefinitionRepo = new StorageDefinitionRepository(this);
        UserRepo = new UserRepository(this);
        TokenRepo = new TokenRepository(this);
        PermissionRepo = new PermissionRepository(this);

        PermissionRepo.AddDeaultPermissions();
        UserRepo.AddDefaultAdmin(adminPassword);

#if DEBUG
        MapObjectRepo.AddTestData();
        StorageDefinitionRepo.AddTestData();
#endif
    }

	~DB() {
		LDB.Dispose();
	}
}