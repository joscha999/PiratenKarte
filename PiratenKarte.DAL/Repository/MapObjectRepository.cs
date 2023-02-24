using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class MapObjectRepository {
	private readonly ILiteCollection<MapObject> Col;
	private readonly DB DB;

	public MapObjectRepository(DB db) {
		Col = db.LDB.GetCollection<MapObject>("MapObjects");
		Col.EnsureIndex(mo => mo.Id);

		DB = db;
	}

	public int Count() => Col.Count();

	public IEnumerable<MapObject> GetAll() => Col.FindAll();
	public IEnumerable<MapObject> GetPaged(int offset, int count)
		=> Col.Query().Include(mo => mo.Storage).Skip(offset).Limit(count).ToEnumerable();
	public IEnumerable<MapObject> GetMap()
		=> Col.Query().Include(mo => mo.Storage).Where(mo => mo.Storage == null).ToEnumerable();
	public IEnumerable<MapObject> GetInStorage(StorageDefinition storage)
		=> Col.Query().Include(mo => mo.Storage).Where(mo => mo.Storage == storage).ToEnumerable();

	public MapObject Get(Guid id) => Col.Include(mo => mo.Storage).FindById(id);

	public Guid Insert(MapObject obj) => Col.Insert(obj);

	public void Update(MapObject obj) => Col.Update(obj);

	public void Delete(Guid id) => Col.Delete(id);

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