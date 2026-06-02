using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Package.Interfaces.Models;
using YAEP.WMS.BLL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class InboundHomeAddressMap
    {
        Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>> _Source;
        public InboundHomeAddressMap(Dictionary<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>> source)
        {
            this._Source = source;
        }
        public HomeAddressSlotSsageInfoModel FindSlot(Guid itemUID, int qty, IPackageModel package, bool isCalculateSpace = true)
        {
            var prdMgr = new ProductUtility();
            var itemMapCollection = this._Source.FirstOrDefault(p => p.Key == itemUID);
            if (!itemMapCollection.Equals(default(KeyValuePair<Guid, IEnumerable<HomeAddressSlotSsageInfoModel>>)))
            {
                var collection = itemMapCollection.Value.OrderBy(o => o.StorageSequence).ThenBy(o2=>o2.SlotID);
                //判斷該產品入庫後是否超過該Slot負荷上限
                foreach (var item in collection)
                {
                    if (isCalculateSpace)
                    {
                        var currentVolume = prdMgr.CalculateCUFT(package, qty);
                        var currentWeight = prdMgr.CaculateTTLWeight(package, qty);
                        if (item.Volume + currentVolume <= item.VolumeLimit &&
                         item.Weight + currentWeight <= item.WeightLimit)
                        {
                            AddSpaceToMap(item.SlotUID, currentVolume, currentWeight);
                            return item;
                        } 
                    }
                    else
                    {
                        return item;
                    }
                }
            }
            return null;

        }

        private void AddSpaceToMap(Guid slotUID, decimal currentVolume, decimal currentWeight)
        {
            foreach (var itemMap in _Source)
            {
                var homeAddressModels = itemMap.Value.Where(p => p.SlotUID == slotUID);
                foreach (var hm in homeAddressModels)
                {
                    hm.Volume += currentVolume;
                    hm.Weight += currentWeight;
                }
            }
        }
    }
}
