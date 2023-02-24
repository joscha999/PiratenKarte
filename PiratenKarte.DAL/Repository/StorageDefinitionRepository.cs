using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class StorageDefinitionRepository {
    private readonly ILiteCollection<StorageDefinition> Col;
    private readonly DB DB;

    public StorageDefinitionRepository(DB db) {
        Col = db.LDB.GetCollection<StorageDefinition>("StorageDefinitions");
        Col.EnsureIndex(mo => mo.Id);

        DB = db;
    }

    public int Count() => Col.Count();

    public IEnumerable<StorageDefinition> GetAll() => Col.FindAll();
    public IEnumerable<StorageDefinition> GetPaged(int offset, int count)
        => Col.Query().Skip(offset).Limit(count).ToEnumerable();
    public StorageDefinition Get(Guid id) => Col.FindById(id);

    public Guid Insert(StorageDefinition obj) => Col.Insert(obj);
    public void Update(StorageDefinition obj) => Col.Update(obj);

    public void Delete(Guid id) {
        var storage = Col.FindById(id);
        if (storage == null)
            return;

        foreach (var item in DB.MapObjectRepo.GetInStorage(storage)) {
            item.Storage = null;
            DB.MapObjectRepo.Update(item);
        }

        Col.Delete(id);
    }

#if DEBUG
    internal void AddTestData() {
        if (Col.Count() != 0)
            return;

        Col.Insert(new StorageDefinition {
            Name = "StorageBox Hannover",
            Position = new LatitudeLongitude(52.34822, 9.70623)
        });
    }
#endif
}