using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCatch.Core
{
    public class ParseObject
    {
        public Enums.CatchParseType SourceType { get; set; }
        public Enums.CatchParseType TargetType { get; set; }
        public string ParseString { get; set; }
        public IEnumerable<string> ParseResult { get; set; }
    }
}
