using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ShipCarrierPalletInfo : IShipCarrierPalletInfo
    {
        public Guid UID { get; set; }
        public Guid CarrierPalletUID { get; set; }
        public Guid ShipviaUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string CustPON { get; set; }
        public string CustID { get; set; }
        public string Syspon { get; set; }
        public string BolNo { get; set; }
        public string TrackingNo { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
