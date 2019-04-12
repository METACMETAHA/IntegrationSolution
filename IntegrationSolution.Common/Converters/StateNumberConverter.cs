using IntegrationSolution.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Converters
{
    public static class StateNumberConverter
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


        /// <summary>
        /// Related to car name from Wialon
        /// This function converts string, which contains state number and car name to single format
        /// </summary>
        /// <returns>Converted string</returns>
        public static string ToStateNumberWialon(this string StateNumber)
        {
            StringBuilder stateNum = new StringBuilder();

            StateNumber = StateNumber.Replace('(', ' ').Replace(')', ' ');

            byte[] bytes = Encoding.Default.GetBytes(StateNumber);
            var encodedState = Encoding.UTF8.GetString(bytes);

            var collection = encodedState?.Trim().Split(' ');           
            
            //выкинуть из коллекции выше названиемашин и оставить номера...пройтись по спискуу и проверить
            foreach (var it in collection)
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
