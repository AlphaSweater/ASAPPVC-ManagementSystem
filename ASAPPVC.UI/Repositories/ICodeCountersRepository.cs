using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.Repositories
{
    public interface ICodeCountersRepository : IBaseRepository<CodeCounters>
    {
        Task<CodeCounters?> GetByTypeAndPeriodAsync(string codeType, string? periodKey, CancellationToken ct = default);
        Task<CodeCounters> AddAndSaveAsync(CodeCounters counter, CancellationToken ct = default);
    }
}
