using PiratenKarte.DAL.Models;

namespace PiratenKarte.DAL.Repository;

public class TokenRepository : RepositoryBase<Token> {
    public override string CollectionName => "Tokens";

    public TokenRepository(DB db) : base(db) { }

    internal override LiteDB.ILiteQueryable<Token> Includes(LiteDB.ILiteQueryable<Token> query)
        => query.Include(t => t.User);

    internal override LiteDB.ILiteCollection<Token> Includes(LiteDB.ILiteCollection<Token> query)
        => query.Include(t => t.User);

    public bool CheckToken(string content, Guid userId, TokenType type) {
        var token = Col.FindOne(t => t.Content == content);
        if (token == null)
            return false;
        if (token.User == null || token.User.Id != userId)
            return false;
        if (token.ValidTill < DateTime.UtcNow)
            return false;

        return token.Type == type;
    }

    public void Invalidate(string token, Guid userId)
        => Col.DeleteMany(t => t.User!.Id == userId && t.Content == token);

    public void InvalidateAllForUser(Guid userId)
        => Col.DeleteMany(t => t.User!.Id == userId);
}