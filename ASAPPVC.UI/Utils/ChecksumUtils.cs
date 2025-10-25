using System.Linq;

namespace ASAPPVC.UI.Utils
{
    public static class ChecksumUtils
    {
        /// <summary>
        /// Compute a simple mod-10 checksum over letters and digits. Letters map A=1..Z=26.
        /// </summary>
        public static int ComputeChecksum(string input)
        {
            if (string.IsNullOrEmpty(input))
                return 0;

            var chars = input.ToUpper().Where(char.IsLetterOrDigit).ToArray();
            int sum = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                int value = char.IsDigit(chars[i])
                    ? chars[i] - '0'
                    : (chars[i] - 'A' + 1); // A=1, B=2, ...
                sum += value;
            }

            return sum % 10;
        }

        /// <summary>
        /// Validate a code where the last character is expected to be the checksum digit.
        /// Non-alphanumeric characters in the code are ignored when computing the checksum.
        /// </summary>
        public static bool ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 2)
                return false;

            string baseCode = code[..^1];
            int expectedChecksum = ComputeChecksum(baseCode);
            return code[^1].ToString() == expectedChecksum.ToString();
        }
    }
}
