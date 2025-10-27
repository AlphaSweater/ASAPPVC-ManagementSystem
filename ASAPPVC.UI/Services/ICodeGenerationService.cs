using ASAPPVC.UI.Models;
using ASAPPVC.UI.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace ASAPPVC.UI.Services
{
    /// <summary>
    /// Service contract for generating unique, checksummed codes for various entity types.
    /// Provides business logic for sequential code generation with configurable formats and period-based resets.
    /// </summary>
    public interface ICodeGenerationService
    {
        /// <summary>
        /// Generates a new unique code based on the provided request parameters.
        /// The generated code format varies by type (Product, Component, Order, PickingSlip)
        /// and includes an automatic checksum suffix for validation.
        /// </summary>
        /// <param name="request">
        /// A <see cref="CodeGenerationRequest"/> specifying the type, optional category, version,
        /// related code, and timestamp for code generation. Must not be null.
        /// </param>
        /// <param name="ct">Cancellation token for async operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the generated code string on success,
        /// or an error message on failure (e.g., validation error, database error).
        /// </returns>
        /// <example>
        /// Product code: PRD-WIN-0001-V01-A
        /// Component code: CMP-DRR-00001-B
        /// Order code: ORD-202401-0042-C
        /// PickingSlip code: PSL-ORD0042-V01-D
        /// </example>
        Task<Result<string>> GenerateCodeAsync(CodeGenerationRequest request, CancellationToken ct = default);

        /// <summary>
        /// Validates a code's checksum to verify its integrity.
        /// </summary>
        /// <param name="code">The complete code including checksum to validate.</param>
        /// <returns>
        /// True if the checksum is valid, false otherwise.
        /// </returns>
        bool ValidateChecksum(string code);
    }
}