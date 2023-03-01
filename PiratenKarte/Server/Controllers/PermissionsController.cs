using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

public class PermissionsController : PKController {
    private readonly DB DB;
    private readonly IMapper Mapper;

    public PermissionsController(DB db, IMapper mapper) {
        DB = db;
        Mapper = mapper;
    }

    [HttpGet]
    [Permission("permissions_update")]
    public IEnumerable<Permission> GetVisibleFor(Guid id) {
        var userId = GetUserId();
        if (userId == null)
            return Enumerable.Empty<Permission>();

        var userPermissions = DB.UserRepo.GetUserPermissions(id).ToList();
        var selfPermissions = DB.UserRepo.GetUserPermissions(userId.Value).ToList();
        var visible = new List<Permission>();

        foreach (var p in selfPermissions) {
            var mapped = Mapper.Map<Permission>(p);
            mapped.Applied = userPermissions.Any(up => up.Key == mapped.Key);
            visible.Add(mapped);
        }

        return visible;
    }

    [HttpGet]
    [EnsureLoggedIn]
    public IEnumerable<Permission> GetSelf() {
        var userId = GetUserId();
        if (userId == null)
            return Enumerable.Empty<Permission>();

        var user = DB.UserRepo.GetWithPermissions(userId.Value);
        if (user == null)
            return Enumerable.Empty<Permission>();

        return user.Permissions.Select(Mapper.Map<Permission>);
    }

    [HttpPost]
    [Permission("permissions_update")]
    public void SetPermission(SetPermission request) {
        var userId = GetUserId();
        if (userId == request.UserId)
            return;

        var permission = DB.PermissionRepo.Get(request.PermissionId);
        if (permission == null)
            return;

        var user = DB.UserRepo.GetWithPermissions(request.UserId);
        if (user == null)
            return;

        if (request.State) {
            if (user.Permissions.Any(p => p.Key == permission.Key))
                return;

            user.Permissions.Add(permission);
        } else {
            user.Permissions.RemoveAll(p => p.Key == permission.Key);
        }

        DB.UserRepo.Update(user);
    }
}