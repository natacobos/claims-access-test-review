using Microsoft.Data.Sqlite;

namespace Claims;

public record Claim(long Id, int MemberId, string Provider, string Description, double Amount, string Status, string SubmittedAt);

// Who is asking. Role is "member" (a member of the website) or "support" (a support agent).
public record Requester(int Id, string Role);

// An in-memory copy of the claims database with sample data. In production this is SQL Server.
public static class Db
{
    public static SqliteConnection CreateSeeded()
    {
        var db = new SqliteConnection("Data Source=:memory:");
        db.Open();
        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            -- holder_id: the account holder of a dependent (NULL for account holders).
            CREATE TABLE members (id INTEGER PRIMARY KEY, name TEXT, holder_id INTEGER NULL);
            CREATE TABLE claims (id INTEGER PRIMARY KEY, member_id INTEGER, provider TEXT, description TEXT,
                                 amount REAL, status TEXT, submitted_at TEXT);
            INSERT INTO members VALUES
              (1, 'Ana Torres', NULL),   -- account holder
              (2, 'Leo Torres', 1),      -- Ana's son, a dependent
              (3, 'Ben Carter', NULL),   -- account holder, another family
              (4, 'Mia Carter', 3);      -- Ben's daughter, a dependent
            INSERT INTO claims VALUES
              (101, 1, 'Bright Smiles Dental', 'Dental cleaning',  120.00, 'paid',    '2026-09-02'),
              (102, 1, 'Clear View Optics',    'Eye exam',          95.50, 'paid',    '2026-09-15'),
              (201, 2, 'Kids Care Clinic',     'Pediatric visit',   80.00, 'pending', '2026-10-01'),
              (301, 3, 'Move Well Therapy',    'Physical therapy', 150.00, 'paid',    '2026-09-20'),
              (401, 4, 'Kids Care Clinic',     'Allergy test',     210.00, 'denied',  '2026-10-12');
            """;
        cmd.ExecuteNonQuery();
        return db;
    }
}
