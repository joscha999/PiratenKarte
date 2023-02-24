using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageDefinitionsController : Controller {
    private readonly DB DB;
    private readonly IMapper Mapper;

    public StorageDefinitionsController(DB db, IMapper mapper) {
        DB = db;
        Mapper = mapper;
    }

    [HttpGet]
    public IEnumerable<StorageDefinition> GetAll()
        => DB.StorageDefinitionRepo.GetAll().Select(Mapper.Map<StorageDefinition>);
    [HttpGet]
    public StorageDefinition Get(Guid id) => Mapper.Map<StorageDefinition>(DB.StorageDefinitionRepo.Get(id));

    [HttpGet]
    public PagedData<StorageDefinition> GetPaged(int page, int itemsPerPage) => new() {
        Data = DB.StorageDefinitionRepo.GetPaged(page * itemsPerPage, itemsPerPage)
            .Select(Mapper.Map<StorageDefinition>).ToList(),
        TotalCount = DB.StorageDefinitionRepo.Count()
    };

    [HttpPost]
    public Guid Create(StorageDefinition definition)
        => DB.StorageDefinitionRepo.Insert(Mapper.Map<DAL.Models.StorageDefinition>(definition));

    [HttpPost]
    public void Update(StorageDefinition definition)
        => DB.StorageDefinitionRepo.Update(Mapper.Map<DAL.Models.StorageDefinition>(definition));

    [HttpPost]
    public void Delete([FromBody] Guid id) => DB.StorageDefinitionRepo.Delete(id);
}
