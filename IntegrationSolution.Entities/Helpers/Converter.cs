using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegrationSolution.Entities.Helpers
{
    public static class Converter
    {
        // Dictionary for transform state number from latin to cyrillic
        private static Dictionary<char, char> mapTranform = new Dictionary<char, char>
        {
            { 'E', 'Е' },
            { 'T', 'Т' },
            { 'Y', 'У' },
            { 'U', 'И' },
            { 'I', 'І' },
            { 'O', 'О' },
            { 'P', 'Р' },
            { 'A', 'А' },
            { 'H', 'Н' },
            { 'K', 'К' },
            { 'X', 'Х' },
            { 'C', 'С' },
            { 'B', 'В' },
            { 'M', 'М' }
        };


        /// <summary>
        /// This function converts string, which contains state number to single format
        /// </summary>
        /// <returns>Converted string</returns>
        public static string ToStateNumber(this string StateNumber)
        {
            StringBuilder stateNum = new StringBuilder();
            foreach (var it in StateNumber?.Trim().Replace(" ", string.Empty))
            {
                if (mapTranform.ContainsKey(it))
                    stateNum.Append(mapTranform[it]);
                else
                    stateNum.Append(it);
            }
            return stateNum.ToString();
        }
    }
}
