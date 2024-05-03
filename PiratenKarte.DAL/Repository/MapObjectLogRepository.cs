using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class MapObjectLogRepository : RepositoryBase<MapObjectLogEntry> {
    public override string CollectionName => "MapObjectLog";

    internal override ILiteQueryable<MapObjectLogEntry> Includes(ILiteQueryable<MapObjectLogEntry> query) => query;
    internal override ILiteCollection<MapObjectLogEntry> Includes(ILiteCollection<MapObjectLogEntry> query) => query;

    public MapObjectLogRepository(DB db) : base(db) { }

    public void Insert(Guid objId, string str) => Col.Insert(new MapObjectLogEntry {
            MapObjectId = objId,
            Entry = str
        });

    public IEnumerable<MapObjectLogEntry> GetForObject(Guid objId)
        => Col.Query().Where(l => l.MapObjectId == objId).ToEnumerable();
}