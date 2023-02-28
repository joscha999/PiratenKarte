using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class MapObjectRepository : RepositoryBase<MapObject> {
    public override string CollectionName => "MapObjects";

    public MapObjectRepository(DB db) : base(db) { }

	public IEnumerable<MapObject> GetMap()
		=> Col.Query().Include(mo => mo.Storage).Where(mo => mo.Storage == null).ToEnumerable();
	public IEnumerable<MapObject> GetInStorage(StorageDefinition storage)
		=> Col.Query().Include(mo => mo.Storage).Where(mo => mo.Storage == storage).ToEnumerable();

    internal override ILiteQueryable<MapObject> Includes(ILiteQueryable<MapObject> query)
        => query.Include(mo => mo.Storage);

    internal override ILiteCollection<MapObject> Includes(ILiteCollection<MapObject> query)
        => query.Include(mo => mo.Storage);

#if DEBUG
    internal void AddTestData() {
        for (var i = Col.Count(); i < 25; i++) {
            Col.Insert(new MapObject {
                Name = "Test-" + i
            });
        }
    }
#endif
}