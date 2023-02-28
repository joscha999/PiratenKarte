using AutoMapper;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Shared;

namespace PiratenKarte.Server.Controllers;

public class StorageDefinitionsController : CrudController<StorageDefinition, DAL.Models.StorageDefinition> {
    public override string PermissionBaseName => "storagedefinitions";

    public StorageDefinitionsController(DB db, IMapper mapper) : base(db, mapper) { }

    protected override RepositoryBase<DAL.Models.StorageDefinition> Repository => DB.StorageDefinitionRepo;
}