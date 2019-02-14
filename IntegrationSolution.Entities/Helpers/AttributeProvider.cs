using IntegrationSolution.Entities.Implementations.Fuel;
using IntegrationSolution.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Helpers
{
    /// <summary>
    /// This class is static and works with attributes.
    /// Get/set value of attributes
    /// </summary>
    public static class AttributeProvider
    {
        /// <summary>
        /// It get [HeaderAttribute] value or returns null
        /// </summary>
        /// <returns>String value</returns>
        public static string GetHeaderDescription<T>(string propertyName) where T : class
        {
            try
            {
                PropertyInfo property = typeof(T).GetProperties().Where(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                return property?.GetCustomAttributes(typeof(HeaderAttribute), false).Select(o => (HeaderAttribute)o)?.First().Description;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// It set value to [HeaderAttribute] of according property
        /// </summary>
        /// <returns>true or false</returns>
        public static bool SetHeaderDescription<T>(string propertyName, string newValue) where T : class
        {
            try
            {
                var props = TypeDescriptor.GetProperties(typeof(T))[propertyName].Attributes[typeof(HeaderAttribute)] as HeaderAttribute;
                props.Description = props.Description.Replace("&replace&", newValue);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
