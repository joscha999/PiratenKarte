using LiteDB;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public abstract class RepositoryBase<T> where T : IDbIdentifier {
    protected readonly ILiteCollection<T> Col;
    protected readonly DB DB;

    public abstract string CollectionName { get; }

    protected RepositoryBase(DB db) {
        Col = db.LDB.GetCollection<T>(CollectionName);
        Col.EnsureIndex(t => t.Id);
        Setup();

        DB = db;
    }

    protected virtual void Setup() { }

    internal abstract ILiteQueryable<T> Includes(ILiteQueryable<T> query);
    internal abstract ILiteCollection<T> Includes(ILiteCollection<T> query);

    public virtual int Count() => Col.Count();

    public virtual IEnumerable<T> GetAll() => Col.FindAll();
    public virtual IEnumerable<T> GetPaged(int offset, int count)
        => Col.Query().Includes(this).Skip(offset).Limit(count).ToEnumerable();

    public virtual T Get(Guid id) => Col.Includes(this).FindById(id);

    public virtual Guid Insert(T obj) => Col.Insert(obj);

    public virtual void Update(T obj) => Col.Update(obj);

    public virtual void Delete(Guid id) => Col.Delete(id);
}

file static class ILiteQueryableExtensions {
    public static ILiteCollection<T> Includes<T>(this ILiteCollection<T> col, RepositoryBase<T> repo)
        where T : IDbIdentifier => repo.Includes(col);
    public static ILiteQueryable<T> Includes<T>(this ILiteQueryable<T> query, RepositoryBase<T> repo)
        where T : IDbIdentifier => repo.Includes(query);
}