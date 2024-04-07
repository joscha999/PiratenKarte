using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

public class GroupController : CrudController<GroupDTO, Group> {
    public override string PermissionBaseName => "groups";
    protected override RepositoryBase<Group> Repository => DB.GroupRepo;

    public GroupController(DB db, IMapper mapper) : base(db, mapper) { }

    [HttpPost]
    [EnsureLoggedIn]
    public IActionResult GetForUser([FromBody] Guid userId) {
        var user = DB.UserRepo.Get(userId);
        if (user == null)
            return BadRequest();

        var groups = DB.GroupRepo.GetForUser(user);
        return Ok(groups);
    }

    [NonAction]
    public override GroupDTO Get(Guid id) => throw new NotImplementedException();

    [HttpPost]
    [EnsureLoggedIn]
    public IActionResult GetSingle([FromBody] Guid id) {
        if (!TryGetUser(out var user))
            return BadRequest();
        if (!IsUserInGroup(user, id))
            return Unauthorized();

        var group = DB.GroupRepo.Get(id);
        return group == null ? NotFound() : Ok(Mapper.Map<GroupDTO>(group));
    }

    [HttpPost]
    [Permission("groups_add_user")]
    public IActionResult GetForEdit([FromBody] Guid editUserId) {
        var userId = GetUserId();
        if (userId == null)
            return BadRequest();

        var editingUser = DB.UserRepo.Get(userId.Value);
        if (editingUser == null)
            return BadRequest();

        var editUser = DB.UserRepo.Get(editUserId);
        if (editUser == null)
            return BadRequest();

        var userGroups = DB.GroupRepo.GetForUser(editUser).ToList();
        var selfGroups = DB.GroupRepo.GetForUser(editingUser).ToList();
        var visible = new List<GroupDTO>();

        foreach (var g in selfGroups) {
            var mapped = Mapper.Map<GroupDTO>(g);
            mapped.Applied = userGroups.Any(ug => ug.Id == mapped.Id);
            visible.Add(mapped);
        }

        return Ok(visible);
    }

    [HttpPost]
    [Permission("groups_add_user")]
    public IActionResult SetUserGroup(SetUserGroupRequest request) {
        var user = DB.UserRepo.Get(request.UserId);
        if (user == null)
            return BadRequest();

        var group = DB.GroupRepo.Get(request.GroupId);
        if (group == null)
            return BadRequest();

        if (request.Applied) {
            user.GroupIds.Add(group.Id);
        } else {
            user.GroupIds.Remove(group.Id);
        }

        DB.UserRepo.Update(user);
        return Ok();
    }
}