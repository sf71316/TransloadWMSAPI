using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundDefaultHomeAddressBuilder : AbstractInboundHomeAddressBuilder
    {
        public InboundDefaultHomeAddressBuilder(InboundHomeAddressBuilderInitParameters initParameters)
            : base(initParameters)
        {

        }

        public override InboundHomeAddressMap GetStorageHomeAddress(Guid warehouseUID,
            IEnumerable<Guid> itemUIDs)
        {
            Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>> mappingTable =
                new Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>>();
            var slotsUsage = this.WarehouseManger.GetSlotUsageInfoByInbound(warehouseUID);
            var homeAddressRelation = this.WarehouseManger
                .GetHomeAddressRelation(itemUIDs, HomeAddressType.Storage);
            var ItemGroup = itemUIDs.GroupBy(g => g).Select(p => p.Key);
            if (slotsUsage.Content.Count() > 0)
            {
                //1.Mapping to SlotUsage
                foreach (var itemUID in ItemGroup)
                {
                    List<HomeAddressSlotSsageInfoModel> _subhomeAddressModels = new List<HomeAddressSlotSsageInfoModel>();
                    slotsUsage.Content.ToList().ForEach(p =>
                    {
                        var e = new HomeAddressSlotSsageInfoModel(p);
                        e.ItemUID = itemUID;
                        _subhomeAddressModels.Add(e);
                    });
                    //2.Home address relation add sequence to SlotUsage
                    _subhomeAddressModels.ForEach(p =>
                    {
                        var _relation = homeAddressRelation.Content.FirstOrDefault(x =>
                        x.SlotUID == p.SlotUID && x.ItemUID == p.ItemUID);
                        if (_relation != null)
                        {
                            p.StorageSequence = _relation.Sequence;
                        }
                        else
                        {

                        }
                    });

                    mappingTable.Add(itemUID, _subhomeAddressModels);
                }

                return new InboundHomeAddressMap(mappingTable);
            }

            return null;
        }
    }
}
