using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class AssignedParameterConverter
    {
        public IAssignedWorkOrderCollection OutboundParameterConvert(IAssignedOutboundWorkOrderCollection parm)
        {
            AssignedWorkOrderInnerCollection collection = new AssignedWorkOrderInnerCollection();
            collection.VesselUID = parm.VesselUID;
            collection.ServiceType = parm.ServiceType;
            collection.LoadingZoneSlotUID = parm.LoadingZoneSlotUID;
            foreach (var item in parm.Items)
            {
                var e = new AssignedWorkOrderPayloadInnerModel();
                e.ItemUID = item.ItemUID;
                e.ItemGroupUID = item.ItemGroupUID;
                e.VesselMainifestUID = item.VesselMainifestUID;
                e.PayloadUID = item.PayloadUID;
                e.OnhandPayloadItems = item.OnhandPayloadItems;
                e.ReceivePackageQty = item.AllocatedQty;
                if (item.AllocateType == Constant.Enums.AllocateType.FutureAllocate)
                {
                    e.PayloadType = Constant.Enums.PayloadType.FutureAllocated;
                    e.WorkorderPayloadType = Constant.Enums.WorkOrderPayloadType.FutureAllocated;
                    e.SlotUID = item.SlotUID;
                    e.ReceivePackageUID = item.PickPackageUID;
                }
                else
                {
                    e.PayloadType = Constant.Enums.PayloadType.Allocated;
                    e.WorkorderPayloadType = Constant.Enums.WorkOrderPayloadType.Allocated;
                }
                collection.Items.Add(e);
            }
            return collection;
        }
        public IAssignedWorkOrderCollection MoveParameterConvert(IAssignedOutboundWorkOrderCollection parm)
        {
            AssignedWorkOrderInnerCollection collection = new AssignedWorkOrderInnerCollection();
            collection.VesselUID = parm.VesselUID;
            collection.ServiceType = parm.ServiceType;
            foreach (var item in parm.Items)
            {
                var e = new AssignedWorkOrderPayloadInnerModel();
                e.ItemUID = item.ItemUID;
                e.VesselMainifestUID = item.VesselMainifestUID;
                e.PayloadUID = item.PayloadUID;
                e.ReceivePackageQty = item.AllocatedQty;
                e.ReceivePackageUID = item.PickPackageUID;
                e.SlotUID = item.SlotUID;
                collection.Items.Add(e);
            }
            return collection;
        }
        public IAssignedWorkOrderCollection BulkPickParameterConvert(IAssignedOutboundWorkOrderCollection parm)
        {

            AssignedWorkOrderInnerCollection collection = new AssignedWorkOrderInnerCollection();
            collection.VesselUID = parm.VesselUID;
            collection.ServiceType = parm.ServiceType;
            foreach (var item in parm.Items)
            {
                var _item = item as IAssignedBulkPickWorkOrderPayload;
                var e = new BulkPikcAssignedWorkOrderPayload();
                e.PayloadUID = Guid.NewGuid();
                e.ItemUID = _item.ItemUID;
                e.ReceivePackageQty = _item.AllocatedQty;
                e.ReceivePackageUID = _item.PickPackageUID;
                e.SlotUID = _item.SlotUID;
                e.TargetSlotUID = _item.TargetSlotUID;
                e.OriginalPayloadUID = _item.OriginalPayloadUID;
                e.OriginalWorkOrderPayloadUID = _item.OriginalWordPayloadUID;
                collection.Items.Add(e);
            }
            return collection;
        }
    }
}
