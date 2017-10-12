using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crow
{
    class CrowOptions
    {
        public bool UseRandomTraining { get; set; }

        public CrowOptions()
        {
            this.UseRandomTraining = true;
        }

        public CrowOptions(bool useRandomOrder)
        {
            this.UseRandomTraining = useRandomOrder;
        }
    }
}
