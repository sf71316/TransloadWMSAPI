using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface IAppConfigure
    {
        string PalletLabelRdlcPath { get; }
        string BoxLabelRdlcPath { get; }
        string SlotLabelRdlcPath { get; }
        bool IsFixFailureByMoveTicket { get; }
        bool IsChangeFromPayload { get; }
        bool IsAllowNegativeOnhandByChangeFromPayload { get; }
        bool IsAllowNegativeOnhandByFixFailure { get; }
        bool EnableLocalLogging { get; }
        bool EnableTrace { get; }
        string ShippingManagementWebServiceUrl { get; }
        string JaegerServiceName { get; }
        string JaegerServiceIP { get; }
        int JaegerServicePort { get; }
    }
}
