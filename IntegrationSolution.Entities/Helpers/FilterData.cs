using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Helpers
{
    public static class FilterData
    {
        /// <summary>
        /// Remove headers (in list for search) which object is not contain
        /// </summary>
        /// <return>IEnumerable of checked headers</return>
        public static IEnumerable<string> CheckHeadersFromObject<T>(IEnumerable<string> headers) where T : class
        {
            var list_props = typeof(T).GetProperties().Select(x => x.Name);
            return headers.Intersect(list_props);
        }

        /// <summary>
        /// Converts IEnumerable to IDictionary and gets values from dictionary parameter
        /// </summary>
        /// <return>IEnumerable of checked headers</return>
        public static IDictionary<string, T> ToIntersectedDictionary<T>(this IEnumerable<string> data, IDictionary<string, T> forClean) where T : class
        {
            for (int i = 0; i < forClean.Count(); i++)
            {
                var tmp = forClean.ElementAt(i);
                if (!data.Contains(tmp.Key))
                    forClean.Remove(tmp);
            }            
            return forClean;
        }
    }
}
