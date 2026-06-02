using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class DefaultAppConfigure : IAppConfigure
    {
        private IAppSettings _AppSettings;
        public DefaultAppConfigure(IAppSettings appSettings)
        {
            _AppSettings = appSettings;

        }
        public bool EnableLocalLogging => _AppSettings.AppSettings["YAEP.WMS.EnableLocalLogging"] == bool.TrueString;
        public string PalletLabelRdlcPath => _AppSettings.AppSettings["YAEP.WMS.API.PalletLabel_Path"];
        public bool EnableTrace => _AppSettings.AppSettings["YAEP.WMS.EnableTrace"] == bool.TrueString;

        public string BoxLabelRdlcPath => _AppSettings.AppSettings["YAEP.WMS.API.BoxLabel_Path"];

        public string SlotLabelRdlcPath => _AppSettings.AppSettings["YAEP.WMS.API.SlotLabel_Path"];

        public bool IsFixFailureByMoveTicket => _AppSettings.AppSettings["YAEP.WMS.API.IsFixFailureByMoveTicket"] == bool.TrueString;

        public bool IsChangeFromPayload => _AppSettings.AppSettings["YAEP.WMS.API.IsChangeFromPayload"] == bool.TrueString;

        public bool IsAllowNegativeOnhandByChangeFromPayload => _AppSettings.AppSettings["YAEP.WMS.API.IsAllowNegativeOnhandByChangeFromPayload"] == bool.TrueString;

        public bool IsAllowNegativeOnhandByFixFailure => _AppSettings.AppSettings["YAEP.WMS.API.IsAllowNegativeOnhandByFixFailure"] == bool.TrueString;

        public string ShippingManagementWebServiceUrl => _AppSettings.AppSettings["YAEP.WMS.API.ShippingManagementWebServiceUrl"];

        public string JaegerServiceName => _AppSettings.AppSettings["JaegerServiceName"];

        public string JaegerServiceIP => _AppSettings.AppSettings["JaegerServiceIP"];

        public int JaegerServicePort => Convert.ToInt32(_AppSettings.AppSettings["JaegerServicePort"]);
    }
}
