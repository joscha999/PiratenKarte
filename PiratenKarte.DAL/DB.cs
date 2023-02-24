using LiteDB;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;

namespace PiratenKarte.DAL;

public class DB {
    internal readonly LiteDatabase LDB;

	public readonly MapObjectRepository MapObjectRepo;
    public readonly StorageDefinitionRepository StorageDefinitionRepo;

	public DB(string path) {
        BsonMapper.Global.Entity<MapObject>().DbRef(mo => mo.Storage, "StorageDefinitions");

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

#if DEBUG
        MapObjectRepo.AddTestData();
        StorageDefinitionRepo.AddTestData();
#endif
    }

	~DB() {
		LDB.Dispose();
	}
}