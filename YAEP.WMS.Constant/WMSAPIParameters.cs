using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Constant
{
    public static class WMSAPIParameters
    {
        public const string APPLICATION_NAME = "WMS";
        public const string CONNECT_LOG_NAME = "WMS-Web-API-History";
        public const string TRACE_LOG_NAME = "WMS-Web-API-Tracing";
        public const string PALLET_UOM_KEYNAME = "Pallet";
        public const string BOX_UOM_KEYNAME = "Box";
        public const string SET_UOM_KEYNAME = "Set";
        public const string EACH_UOM_KEYNAME = "Each";
        public static readonly string[] MIN_PACKAGE_UOM =  {"each","set","pair" };
        public static readonly string[] MAX_PACKAGE_UOM = { "pallet", "crate", "carton","box" };
        //目前只確定Pallet crate 同等級，故在Receiving(Only TJ/NB) 使用這兩個包裝判斷
        public static readonly string[] MAX_PACKAGE_UOM_RECEIVICE = { "pallet", "crate"};
    }
}
