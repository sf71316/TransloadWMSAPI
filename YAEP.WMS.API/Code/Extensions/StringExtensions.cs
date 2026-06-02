using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace YAEP.WMS.API.Code
{
    public static class StringExtensions
    {
        public static string GetFilterXSSstring(this string str)
        {
            return Sanitizer.GetSafeHtmlFragment(str);
        }
    }
}