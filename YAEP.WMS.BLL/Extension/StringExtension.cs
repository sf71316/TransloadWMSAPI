using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Extension
{
    internal static class StringExtension
    {
        public static DbString ToVarchar(this string targetString)
        {
            return new DbString()
            {
                IsAnsi = true,
                IsFixedLength = false,
                Value = targetString,
                Length = targetString.Length
            };
        }
        public static DbString ToNvarchar(this string targetString)
        {
            return new DbString()
            {
                IsAnsi = false,
                IsFixedLength = false,
                Value = targetString,
                Length = targetString.Length
            };
        }
    }
}
