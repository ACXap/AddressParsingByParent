using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressParsingByParent
{
    class ErrorAddress : Address
    {
        public string FullAddress { get; set; }
        public string Why { get; set; }
    }
}
