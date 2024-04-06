using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

public class MapObjectsController : CrudController<MapObject, DAL.Models.MapObject> {
	protected override RepositoryBase<DAL.Models.MapObject> Repository => DB.MapObjectRepo;

	public override string PermissionBaseName => "objects";

    public MapObjectsController(DB db, IMapper mapper) : base(db, mapper) { }

    [HttpGet]
	[Permission("objects_read")]
    public IEnumerable<MapObject> GetMap() => DB.MapObjectRepo.GetMap().Select(Mapper.Map<MapObject>);

    [HttpPost]
    public override Guid Create(MapObject item) {
		throw new NotSupportedException();
    }

    [HttpPost]
    [Permission("objects_create")]
    public Guid CreateSingle(CreateNewObject request) {
        var obj = Mapper.Map<DAL.Models.MapObject>(request.Object);
		obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		return DB.MapObjectRepo.Insert(obj);
    }

	[HttpPost]
    [Permission("objects_create")]
    public void CreateMany(CreateNewObjectBulk request) {
		var storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		for (int i = 0; i < request.Count; i++) {
			var obj = Mapper.Map<DAL.Models.MapObject>(request.Template);
			obj.Name = string.Format(obj.Name, i);
			obj.Storage = storage;
			DB.MapObjectRepo.Insert(obj);
		}
	}

	[HttpPost]
    [Permission("objects_delete")]
    public void DeleteMany([FromBody] IEnumerable<Guid> ids) {
		foreach (var id in ids)
			DB.MapObjectRepo.Delete(id);
	}

	[HttpPost]
    [Permission("objects_comments_create")]
    public void AddComment(NewObjectComment comment) {
		var obj = DB.MapObjectRepo.Get(comment.ObjectId);
		if (obj.Comments == null)
			obj.Comments = new List<DAL.Models.ObjectComment>();

		obj.Comments.Add(Mapper.Map<DAL.Models.ObjectComment>(comment.Comment));
		DB.MapObjectRepo.Update(obj);
	}

	[HttpPost]
    [Permission("objects_comments_delete")]
    public void DeleteComment(DeleteObjectComment request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);
        if (obj.Comments == null)
            obj.Comments = new List<DAL.Models.ObjectComment>();

		obj.Comments.RemoveAll(c => c.Id == request.CommentId);
		DB.MapObjectRepo.Update(obj);
    }

	[HttpPost]
    [Permission("objects_update")]
    public void SetStorage(SetObjectStorage request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);

        obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

        DB.MapObjectRepo.Update(obj);
    }

	[HttpPost]
    [Permission("objects_update")]
    public void SetStorageMany(SetObjectStorageMany request) {
		var storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		foreach (var objId in request.ObjectIds) {
			var obj = DB.MapObjectRepo.Get(objId);
			obj.Storage = storage;
			DB.MapObjectRepo.Update(obj);
		}
	}

	[HttpPost]
    [Permission("objects_update")]
    public void UpdatePosition(SetObjectPosition request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);

		obj.Storage = null;
		obj.LatLon = Mapper.Map<DAL.Models.LatitudeLongitude>(request.Position);

		DB.MapObjectRepo.Update(obj);
	}
}