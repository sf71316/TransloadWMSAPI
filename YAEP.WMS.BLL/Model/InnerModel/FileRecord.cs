using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace YAEP.WMS.BLL
{
    internal class FileRecord
    {
        [Name("barcode(UPC or EAN)")]
        public string barcode { get; set; }
    }
}
