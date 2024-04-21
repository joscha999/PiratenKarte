using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class MarkerStyleRepository : RepositoryBase<MarkerStyle> {
    public static readonly Guid DefaultMarkerGuid = Guid.Parse("7f227e77-1a92-42df-aa14-cf844da3ae3c");

    public override string CollectionName => "MarkerStyles";

    public MarkerStyleRepository(DB db) : base(db) { }

    internal override ILiteQueryable<MarkerStyle> Includes(ILiteQueryable<MarkerStyle> query) => query;
    internal override ILiteCollection<MarkerStyle> Includes(ILiteCollection<MarkerStyle> query) => query;

    public void AddGroupIdToStyle(Guid groupId, Guid markerId) {
        var marker = Col.FindById(markerId);
        if (marker == null)
            return;

        marker.GroupIds.Add(groupId);
        Update(marker);
    }

    internal void AddDefaultStyles() {
        InsertNew(new MarkerStyle {
            DefaultHeightPx = 14,
            DefaultWidthPx = 14,
            BorderRadiusPercent = 100,
            BackgroundColor = "#f80",
            FontSizePercent = 100,
            Id = DefaultMarkerGuid,
            StyleName = "Default Punkt"
        });
    }

    private void InsertNew(MarkerStyle style) {
        var dbStyle = Col.FindById(style.Id);
        if (dbStyle != null)
            return;

        foreach (var group in DB.GroupRepo.GetAll())
            style.GroupIds.Add(group.Id);

        Insert(style);
    }
}