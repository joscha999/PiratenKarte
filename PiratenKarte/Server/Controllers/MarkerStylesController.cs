using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;

namespace PiratenKarte.Server.Controllers;

public class MarkerStylesController : CrudController<MarkerStyleDTO, MarkerStyle> {
    protected override RepositoryBase<MarkerStyle> Repository => DB.MarkerStyleRepo;

    public override string PermissionBaseName => "markerstyles";

    public MarkerStylesController(DB db, IMapper mapper) : base(db, mapper) { }

    [HttpPost]
    [EnsureLoggedIn]
    public IActionResult GetForDisplay([FromBody] List<Guid> styleIds) {
        var styles = new List<MarkerStyleDTO>();

        foreach (var id in styleIds)
            styles.Add(Mapper.Map<MarkerStyleDTO>(DB.MarkerStyleRepo.Get(id)));

        return Ok(styles);
    }

    [HttpGet]
    [Permission("markerstyles_read")]
    public override IEnumerable<MarkerStyleDTO> GetAll() {
        if (!TryGetUser(out var user))
            return [];

        var results = new List<MarkerStyleDTO>();
        foreach (var style in Repository.GetAll()) {
            foreach (var id in style.GroupIds) {
                if (user.GroupIds.Contains(id)) {
                    results.Add(Mapper.Map<MarkerStyleDTO>(style));
                    break;
                }
            }
        }

        return results;
    }

    [HttpPost]
    [Permission("markerstyles_create")]
    public override Guid Create([FromBody] MarkerStyleDTO marker) {
        if (!TryGetUser(out var user))
            return Guid.Empty;

        foreach (var groupId in marker.GroupIds) {
            if (!user.GroupIds.Contains(groupId))
                return Guid.Empty;
        }

        return Repository.Insert(Mapper.Map<MarkerStyle>(marker));
    }

    [HttpPost]
    [Permission("markerstyles_read")]
    public IActionResult GetSingle([FromBody] Guid id) {
        var style = Repository.Get(id);
        if (style == null)
            return NotFound();

        return Ok(style);
    }
}