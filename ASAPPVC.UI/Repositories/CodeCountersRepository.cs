using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    /// <summary>
    /// Atomic counter repository for code generation.
    /// </summary>
    public class CodeCountersRepository(AppDbContext db) : ICodeCountersRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<int> IncrementAndGetAsync(CodeType type, string? periodKey, CancellationToken ct = default)
        {
            var codeType = type.ToString();
            var period = periodKey ?? "GLOBAL";

            // Use interpolated SQL to bind parameters safely.
            var result = await _db.Database.SqlQuery<int>(
                $@"
				INSERT INTO CodeCounters (CodeType, PeriodKey, LastNumber, UpdatedAt)
				VALUES ({codeType}, {period}, 1, CURRENT_TIMESTAMP)
				ON CONFLICT(CodeType, PeriodKey)
				DO UPDATE SET LastNumber = LastNumber + 1, UpdatedAt = CURRENT_TIMESTAMP
				RETURNING LastNumber;"
            ).SingleAsync(ct);

            return result;
        }
    }
}