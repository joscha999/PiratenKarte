using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

public class MapObjectsController : CrudController<MapObjectDTO, DAL.Models.MapObject> {
	protected override RepositoryBase<DAL.Models.MapObject> Repository => DB.MapObjectRepo;

	public override string PermissionBaseName => "objects";

    public MapObjectsController(DB db, IMapper mapper) : base(db, mapper) { }

    [HttpGet]
    [Permission("objects_read")]
    public IActionResult View(Guid id) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var obj = DB.MapObjectRepo.Get(id);
        if (obj == null)
            return NotFound();

        var mappedObj = Mapper.Map<MapObjectDTO>(obj);

        if (HasPermission(user, "objects_comments_read")) {
            foreach (var comment in obj.Comments) {
                var commentUser = DB.UserRepo.Get(comment.UserId);

                mappedObj.Comments.Add(new ObjectCommentDTO {
                    Content = comment.Content,
                    Id = comment.Id,
                    UserId = comment.UserId,
                    InsertionTime = comment.InsertionTime,
                    Username = commentUser?.Username ?? "Geist"
                });
            }
        }

        return Ok(mappedObj);
    }

    [HttpGet]
    [Permission("objects_read")]
    public IActionResult GetMap() {
		if (!TryGetUser(out var user))
			return BadRequest();

		var onMap = DB.MapObjectRepo.GetMap();
		var filtered = new List<MapObjectDTO>();

		foreach (var mo in onMap) {
			if (user.GroupIds.Contains(mo.GroupId))
				filtered.Add(Mapper.Map<MapObjectDTO>(mo));
		}

		return Ok(filtered);
    }

    [NonAction]
    public override PagedData<MapObjectDTO> GetPaged(int page, int itemsPerPage) => throw new NotImplementedException();

    public IActionResult GetPagedEx(int page, int itemsPerPage) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var pagedData = new PagedData<MapObjectDTO>() {
            Data = [],
            TotalCount = 0
        };

        pagedData.Data = DB.MapObjectRepo.GetAll()
            .Where(mo => user.GroupIds.Contains(mo.GroupId))
            .Skip(page * itemsPerPage).Take(itemsPerPage)
            .Select(Mapper.Map<MapObjectDTO>).ToList();
        pagedData.TotalCount = DB.MapObjectRepo.CountVisible(user);

        return Ok(pagedData);
    }

    [NonAction]
    public override Guid Create(MapObjectDTO item) => throw new NotSupportedException();

    [HttpPost]
    [Permission("objects_create")]
    public IActionResult CreateSingle(CreateNewObject request) {
		if (!TryGetUser(out var user))
			return BadRequest();

        var obj = Mapper.Map<DAL.Models.MapObject>(request.Object);
		obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

		if (!IsUserInGroup(user, obj.GroupId))
			return Unauthorized();

		return Ok(DB.MapObjectRepo.Insert(obj));
    }

	[HttpPost]
    [Permission("objects_create")]
    public IActionResult CreateMany(CreateNewObjectBulk request) {
        if (!TryGetUser(out var user))
            return BadRequest();
        if (!IsUserInGroup(user, request.GroupId))
            return Unauthorized();

        var storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);
		for (int i = 0; i < request.Count; i++) {
            var obj = Mapper.Map<DAL.Models.MapObject>(request.Template);
            obj.Name = string.Format(obj.Name, i);
			obj.Storage = storage;
			DB.MapObjectRepo.Insert(obj);
		}

		return Ok();
	}

	[HttpPost]
    [Permission("objects_delete")]
    public IActionResult DeleteMany([FromBody] IEnumerable<Guid> ids) {
        if (!TryGetUser(out var user))
            return BadRequest();

        foreach (var id in ids) {
            var obj = DB.MapObjectRepo.Get(id);
            if (!IsUserInGroup(user, obj.GroupId))
                continue;

            DB.MapObjectRepo.Delete(id);
        }

        return Ok();
    }

	[HttpPost]
    [Permission("objects_comments_create")]
    public IActionResult AddComment(NewObjectComment comment) {
        if (!TryGetUser(out var user))
            return BadRequest();

		var obj = DB.MapObjectRepo.Get(comment.ObjectId);
        obj.Comments ??= [];

        var mappedComment = Mapper.Map<DAL.Models.ObjectComment>(comment.Comment);
        mappedComment.UserId = user.Id;
        mappedComment.Id = Guid.NewGuid();
        obj.Comments.Add(mappedComment);

		DB.MapObjectRepo.Update(obj);
        return Ok();
	}

	[HttpPost]
    public IActionResult DeleteComment(DeleteObjectComment request) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var obj = DB.MapObjectRepo.Get(request.ObjectId);
        obj.Comments ??= [];

        var comment = obj.Comments.Find(c => c.Id == request.CommentId);
        if (comment == null)
            return Ok();

        if (!HasPermissionOrIsSelf(user, "objects_comments_delete", comment.UserId))
            return Unauthorized();

        obj.Comments.Remove(comment);
		DB.MapObjectRepo.Update(obj);
        return Ok();
    }

	[HttpPost]
    [Permission("objects_update")]
    public IActionResult SetStorage(SetObjectStorage request) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var obj = DB.MapObjectRepo.Get(request.ObjectId);
        if (!IsUserInGroup(user, obj.GroupId))
            return Unauthorized();

        obj.Storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);

        DB.MapObjectRepo.Update(obj);
		return Ok();
    }

	[HttpPost]
    [Permission("objects_update")]
    public IActionResult SetStorageMany(SetObjectStorageMany request) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var storage = request.StorageId == null ? null : DB.StorageDefinitionRepo.Get(request.StorageId.Value);
        if (storage == null || !IsUserInGroup(user, storage.GroupId))
            return Unauthorized();

        foreach (var objId in request.ObjectIds) {
			var obj = DB.MapObjectRepo.Get(objId);
			if (!IsUserInGroup(user, obj.GroupId))
				continue;

			obj.Storage = storage;
			DB.MapObjectRepo.Update(obj);
		}

		return Ok();
	}

	[HttpPost]
    [Permission("objects_update")]
    public IActionResult UpdatePosition(SetObjectPosition request) {
        if (!TryGetUser(out var user))
            return BadRequest();

        var obj = DB.MapObjectRepo.Get(request.ObjectId);
		if (!IsUserInGroup(user, obj.GroupId))
			return Unauthorized();

		obj.Storage = null;
		obj.LatLon = Mapper.Map<DAL.Models.LatitudeLongitude>(request.Position);

		DB.MapObjectRepo.Update(obj);
		return Ok();
	}
}