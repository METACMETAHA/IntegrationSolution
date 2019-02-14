using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Helpers
{
    public class HeaderAttribute : Attribute
    {
        public string Description { get; set; }

        public HeaderAttribute(string description)
        {
            Description = description;
        }
    }
}
