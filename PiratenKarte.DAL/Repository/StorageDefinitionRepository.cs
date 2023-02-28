using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class StorageDefinitionRepository : RepositoryBase<StorageDefinition> {
    public override string CollectionName => "StorageDefinitions";

    public StorageDefinitionRepository(DB db) : base(db) { }

    public override void Delete(Guid id) {
        var storage = Col.FindById(id);
        if (storage == null)
            return;

        foreach (var item in DB.MapObjectRepo.GetInStorage(storage)) {
            item.Storage = null;
            DB.MapObjectRepo.Update(item);
        }

        Col.Delete(id);
    }

    internal override ILiteQueryable<StorageDefinition> Includes(ILiteQueryable<StorageDefinition> query) => query;
    internal override ILiteCollection<StorageDefinition> Includes(ILiteCollection<StorageDefinition> query) => query;

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