using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class AllocatedRequest : IAllocatedRequest
    {
        public AllocatedRequest()
        {
            this.Items = new List<IAllocatedItemRequest>();
        }
        public AllocatedRequest(IAllocatedRequest clonerequest) : this()
        {
            this.UsePackingStation = clonerequest.UsePackingStation;
            ETD = clonerequest.ETD;
            RequestBy = clonerequest.RequestBy;
            WarehouseUID = clonerequest.WarehouseUID;
            RefNo = clonerequest.RefNo;
            CustomerPartyName = clonerequest.CustomerPartyName;
            ReceiverUrl = clonerequest.ReceiverUrl;
            ReceiverSecret = clonerequest.ReceiverSecret;
            ShipToAddress = clonerequest.ShipToAddress;
            ShipToZip = clonerequest.ShipToZip;
            ShipToCity = clonerequest.ShipToCity;
            ShipToState = clonerequest.ShipToState;
            ShipToCountry = clonerequest.ShipToCountry;
            OrderType = clonerequest.OrderType;
            PassPackageVersion = clonerequest.PassPackageVersion;
            AllocateMode = clonerequest.AllocateMode;

        }
        public bool UsePackingStation { get; set; }
        public DateTime ETD { get; set; }
        public string RequestBy { get; set; }
        public Guid CustomerUID { get; set; }
        public Guid WarehouseUID { get; set; }
        public string RefNo { get; set; }
        public string CustomerPartyName { get; set; }
        public string ReceiverUrl { get; set; }
        public string ReceiverSecret { get; set; }
        public string ShipToAddress { get; set; }
        public string ShipToZip { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public string ShipToCountry { get; set; }
        public int OrderType { get; set; }
        public IList<IAllocatedItemRequest> Items { get; set; }
        public string CustomerID { get; set; }
        public bool PassPackageVersion { get; set; }
        public AllocateType AllocateMode { get; set; }
        public bool IsChinaWarehouse { get; set; }
    }
}
