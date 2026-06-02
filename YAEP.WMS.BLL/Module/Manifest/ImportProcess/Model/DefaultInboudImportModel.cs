using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.BLL.Module
{
    internal class DefaultInboudImportModel
    {
        public string CustId { get; set; }
        public string PoNo { get; set; }
        public string SysPon { get; set; }
        public string BOL { get; set; }
        public string ItemNo { get; set; }
        public int PC_PL { get; set; }
        public int Onhand { get; set; }
    }
}
