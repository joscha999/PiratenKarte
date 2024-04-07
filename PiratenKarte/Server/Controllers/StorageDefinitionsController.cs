using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared;

namespace PiratenKarte.Server.Controllers;

public class StorageDefinitionsController : CrudController<StorageDefinitionDTO, DAL.Models.StorageDefinition> {
    public override string PermissionBaseName => "storagedefinitions";

    public StorageDefinitionsController(DB db, IMapper mapper) : base(db, mapper) { }

    protected override RepositoryBase<DAL.Models.StorageDefinition> Repository => DB.StorageDefinitionRepo;

    [HttpGet]
    [EnsureLoggedIn]
    public IActionResult GetForUser() {
        if (!TryGetUser(out var user))
            return BadRequest();

        return Ok(DB.StorageDefinitionRepo.GetAll()
            .Where(sd => user.GroupIds.Contains(sd.GroupId))
            .Select(Mapper.Map<StorageDefinitionDTO>));
    }
}