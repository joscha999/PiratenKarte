﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;
using PiratenKarte.DAL.Repository;
using PiratenKarte.Server.Authorization;
using PiratenKarte.Shared.RequestModels;

namespace PiratenKarte.Server.Controllers;

public abstract class CrudController : ControllerBase {
    public abstract string PermissionBaseName { get; }
}

[ApiController]
[Route("[controller]/[action]")]
public abstract class CrudController<TApi, TDb> : CrudController where TDb : IDbIdentifier {
    protected readonly DB DB;
    protected readonly IMapper Mapper;

    protected abstract RepositoryBase<TDb> Repository { get; }

    public CrudController(DB db, IMapper mapper) {
        DB = db;
        Mapper = mapper;
    }

    [HttpGet]
    [CrudPermission("_read")]
    public virtual IEnumerable<TApi> GetAll()
        => Repository.GetAll().Select(Mapper.Map<TDb, TApi>);
    [HttpGet]
    [CrudPermission("_read")]
    public virtual TApi Get(Guid id) => Mapper.Map<TDb, TApi>(Repository.Get(id));

    [HttpGet]
    [CrudPermission("_read")]
    public virtual PagedData<TApi> GetPaged(int page, int itemsPerPage) => new() {
        Data = Repository.GetPaged(page * itemsPerPage, itemsPerPage)
            .Select(Mapper.Map<TDb, TApi>).ToList(),
        TotalCount = Repository.Count()
    };

    [HttpPost]
    [CrudPermission("_create")]
    public virtual Guid Create(TApi item) => Repository.Insert(Mapper.Map<TApi, TDb>(item));
    [HttpPost]
    [CrudPermission("_update")]
    public virtual void Update(TApi item) => Repository.Update(Mapper.Map<TApi, TDb>(item));
    [HttpPost]
    [CrudPermission("_delete")]
    public virtual void Delete([FromBody] Guid id) => Repository.Delete(id);
}