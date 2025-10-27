using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Repositories
{
    public interface ICodeCountersRepository
    {
        /// <summary>
        /// Atomically increments and returns the new LastNumber for (type, periodKey).
        /// </summary>
        Task<int> IncrementAndGetAsync(CodeType type, string? periodKey, CancellationToken ct = default);
    }
}