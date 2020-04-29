using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api
{
    public class ScraperCreatorObject
    {
        public string ClassName { get; set; }
        public IEnumerable<string> Arguments { get; set; }

        public uint ArgumentCount { get; set; }
    }
}
