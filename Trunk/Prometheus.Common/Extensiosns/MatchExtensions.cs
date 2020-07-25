using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Prometheus.Common.Extensiosns
{
    public static class MatchExtensions
    {
        public static bool ExactMatch(this string input, string match)
        {
            return Regex.IsMatch(input, match);
        }
    }
}
