using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI.Repositories
{
    public class CodeCountersRepository(AppDbContext db) : BaseRepository<CodeCounters>(db), ICodeCountersRepository
    {
        public Task<CodeCounters?> GetByTypeAndPeriodAsync(string codeType, string? periodKey, CancellationToken ct = default)
        {
            return _set.FirstOrDefaultAsync(c => c.CodeType == codeType && c.PeriodKey == periodKey, ct);
        }

        public async Task<CodeCounters> AddAndSaveAsync(CodeCounters counter, CancellationToken ct = default)
        {
            // Reuse BaseRepository.AddAsync which returns the entity (with identity if generated)
            var entity = await base.AddAsync(counter, ct);
            await base.SaveAsync(ct);
            return entity;
        }
    }
}