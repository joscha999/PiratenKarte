using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MapObjectsController : ControllerBase {
	private readonly DB DB;
	private readonly IMapper Mapper;

	public MapObjectsController(DB db, IMapper mapper) {
		DB = db;
		Mapper = mapper;
	}

	[HttpGet]
	public IEnumerable<MapObject> GetMap() => DB.MapObjectRepo.GetMap().Select(Mapper.Map<MapObject>);
    [HttpGet]
    public MapObject Get(Guid id) => Mapper.Map<MapObject>(DB.MapObjectRepo.Get(id));

    [HttpGet]
	public PagedData<MapObject> GetPaged(int page, int itemsPerPage) => new PagedData<MapObject> {
		Data = DB.MapObjectRepo.GetPaged(page * itemsPerPage, itemsPerPage).Select(Mapper.Map<MapObject>).ToList(),
		TotalCount = DB.MapObjectRepo.Count()
	};

	[HttpPost]
	public Guid Create(CreateNewObject request) {
        var obj = Mapper.Map<DAL.Models.MapObject>(request.Object);
		obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		return DB.MapObjectRepo.Insert(obj);
    }

	[HttpPost]
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
	public void Update(MapObject obj) => DB.MapObjectRepo.Update(Mapper.Map<DAL.Models.MapObject>(obj));

	[HttpPost]
	public void Delete([FromBody] Guid id) => DB.MapObjectRepo.Delete(id);
	[HttpPost]
	public void DeleteMany([FromBody] IEnumerable<Guid> ids) {
		foreach (var id in ids)
			DB.MapObjectRepo.Delete(id);
	}

	[HttpPost]
	public void AddComment(NewObjectComment comment) {
		var obj = DB.MapObjectRepo.Get(comment.ObjectId);
		if (obj.Comments == null)
			obj.Comments = new List<DAL.Models.ObjectComment>();

		obj.Comments.Add(Mapper.Map<DAL.Models.ObjectComment>(comment.Comment));
		DB.MapObjectRepo.Update(obj);
	}

	[HttpPost]
	public void DeleteComment(DeleteObjectComment request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);
        if (obj.Comments == null)
            obj.Comments = new List<DAL.Models.ObjectComment>();

		obj.Comments.RemoveAll(c => c.Id == request.CommentId);
		DB.MapObjectRepo.Update(obj);
    }

	[HttpPost]
	public void SetStorage(SetObjectStorage request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);

        obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

        DB.MapObjectRepo.Update(obj);
    }

	[HttpPost]
	public void SetStorageMany(SetObjectStorageMany request) {
		var storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		foreach (var objId in request.ObjectIds) {
			var obj = DB.MapObjectRepo.Get(objId);
			obj.Storage = storage;
			DB.MapObjectRepo.Update(obj);
		}
	}

	[HttpPost]
	public void UpdatePosition(SetObjectPosition request) {
		var obj = DB.MapObjectRepo.Get(request.ObjectId);

		obj.Storage = null;
		obj.LatLon = Mapper.Map<DAL.Models.LatitudeLongitude>(request.Position);

		DB.MapObjectRepo.Update(obj);
	}
}