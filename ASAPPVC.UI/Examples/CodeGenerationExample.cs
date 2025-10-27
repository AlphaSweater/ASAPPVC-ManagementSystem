using ASAPPVC.UI.Services;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Examples
{
    /// <summary>
    /// Example usage of the new CodeGenerationService.
    /// This file demonstrates how to inject and use the service in your code.
    /// </summary>
    public class CodeGenerationExample
    {
        private readonly ICodeGenerationService _codeGenerator;

        // Inject the service via constructor
        public CodeGenerationExample(ICodeGenerationService codeGenerator)
        {
            _codeGenerator = codeGenerator;
        }

        /// <summary>
        /// Example: Generate a product code with category and version
        /// </summary>
        public async Task<string?> GenerateProductCodeAsync()
        {
            var request = new CodeGenerationRequest
            {
                Type = Models.Enums.CodeType.Product,
                Category = "WIN",  // Optional: Window category
                Version = 1        // Optional: Version number
            };

            var result = await _codeGenerator.GenerateCodeAsync(request);

            if (result.Ok)
            {
                // Success! Use the generated code
                string productCode = result.Value;  // e.g., "PRD-WIN-0001-V01-A"
                return productCode;
            }
            else
            {
                // Handle error
                Console.WriteLine($"Error: {result.Error}");
                return null;
            }
        }

        /// <summary>
        /// Example: Generate a component code
        /// </summary>
        public async Task<string?> GenerateComponentCodeAsync()
        {
            var request = new CodeGenerationRequest
            {
                Type = Models.Enums.CodeType.Component,
                Category = "DRR"  // Optional: Door category
            };

            var result = await _codeGenerator.GenerateCodeAsync(request);

            return result.Ok ? result.Value : null;  // e.g., "CMP-DRR-00001-B"
        }

        /// <summary>
        /// Example: Generate an order code (period-based)
        /// </summary>
        public async Task<string?> GenerateOrderCodeAsync()
        {
            var request = new CodeGenerationRequest
            {
                Type = Models.Enums.CodeType.Order
                // 'When' is optional - defaults to current UTC time
            };

            var result = await _codeGenerator.GenerateCodeAsync(request);

            return result.Ok ? result.Value : null;  // e.g., "ORD-202401-0042-C"
        }

        /// <summary>
        /// Example: Generate a picking slip code linked to an order
        /// </summary>
        public async Task<string?> GeneratePickingSlipCodeAsync(string orderCode)
        {
            var request = new CodeGenerationRequest
            {
                Type = Models.Enums.CodeType.PickingSlip,
                RelatedCode = orderCode,  // Required for picking slips
                Version = 1
            };

            var result = await _codeGenerator.GenerateCodeAsync(request);

            return result.Ok ? result.Value : null;  // e.g., "PSL-ORD0042-V01-D"
        }

        /// <summary>
        /// Example: Validate a code's checksum
        /// </summary>
        public bool ValidateCode(string code)
        {
            return _codeGenerator.ValidateChecksum(code);
        }

        /// <summary>
        /// Example: Generate a backfilled code with a specific date
        /// </summary>
        public async Task<string?> GenerateBackfilledOrderAsync(DateTime orderDate)
        {
            var request = new CodeGenerationRequest
            {
                Type = Models.Enums.CodeType.Order,
                When = orderDate  // Explicit timestamp for backfills
            };

            var result = await _codeGenerator.GenerateCodeAsync(request);

            return result.Ok ? result.Value : null;
        }
    }
}