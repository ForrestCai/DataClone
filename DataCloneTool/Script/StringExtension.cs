using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloneTool
{
    public static class StringExtension
    {
        public static string RaplaceQuote(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Replace("'", "''");
        }

        public static string AppendQuote(this string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return string.Format("'{0}'", value);
        }
    }
}
