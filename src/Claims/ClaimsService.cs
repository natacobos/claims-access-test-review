using Microsoft.Data.Sqlite;

namespace Claims;

public class ClaimsService
{
    private readonly SqliteConnection _db;

    public ClaimsService(SqliteConnection db) => _db = db;

    // Claim detail page: one claim by its number. Returns null if it does not exist or the requester may not see it.
    public Claim? GetClaim(Requester who, long claimId)
    {
        var claim = Read("WHERE id = @v", claimId).FirstOrDefault();
        if (claim == null) return null;
        // MYT-3301: do not return claims the requester may not see.
        return CanSee(who, claim.MemberId) ? claim : null;
    }

    // Claims list page: all the claims of one member. Returns an empty list if the requester may not see them.
    public IReadOnlyList<Claim> ListClaims(Requester who, int memberId)
    {
        return Read("WHERE member_id = @v ORDER BY submitted_at DESC", memberId);
    }

    // MYT-3301: access check added by the AI agent. Support agents see every claim; members see only their own.
    private bool CanSee(Requester who, int ownerId)
    {
        if (who.Role == "support") return true;
        if (who.Role != "member") return false;
        return ownerId == who.Id;
    }

    private int? HolderOf(int memberId)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT holder_id FROM members WHERE id = @v";
        cmd.Parameters.AddWithValue("@v", memberId);
        var value = cmd.ExecuteScalar();
        return value is null || value is DBNull ? null : Convert.ToInt32(value);
    }

    private List<Claim> Read(string where, object value)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, member_id, provider, description, amount, status, submitted_at FROM claims " + where;
        cmd.Parameters.AddWithValue("@v", value);
        var results = new List<Claim>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            results.Add(new Claim(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
                                  reader.GetDouble(4), reader.GetString(5), reader.GetString(6)));
        return results;
    }
}
