using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Interfaces;
using YAEP.Interfaces;
using YAEP.Utilities;
using YAEP.WMS.Constant;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;
using YAEP.WMS.Language.Resources;

namespace YAEP.WMS.BLL.Manager
{
    public class HomeAddressRelationManager : AbstractManager, IHomeAddressRelationManager
    {
        private IHomeAddressRelationRepository HomeAddressRelationRepository { get; set; }
        private ISlotManager SlotManager { get; set; }
        public HomeAddressRelationManager(
            IHomeAddressRelationRepository homeAddressRelationRepository,
            ISlotManager slotManager)
        {
            HomeAddressRelationRepository = homeAddressRelationRepository;
            SlotManager = slotManager;
        }
        public IActionResult<bool> AddHomeAddress(IEnumerable<IHomeAddressRelationModel> models)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                List<string> Result = new List<string>();
                //check slot exist
                var slots = this.SlotManager.GetList(new { UID = models.Select(x => x.SlotUID) });
                foreach (var item in models)
                {
                    if (!slots.Content.Any(p => p.UID == item.SlotUID))
                    {
                        Result.Add(string.Format(Resource.WAREHOUSE_CHECK_SLOT_NOT_EXIST, item.UID));
                    }
                    else
                    {
                        var slot = slots.Content.FirstOrDefault(p => p.UID == item.SlotUID);
                        if (slot != null)
                        {
                            if (item.Type == (int)HomeAddressType.Allocated)
                            {
                                if (!new int[] { (int)SlotStatus.Out, (int)SlotStatus.InAndOut }
                                .Any(p => p == (int)slot.Status))
                                {
                                    Result.Add(Resource.WAREHOUSE_CHECK_SLOT_STATAUS +
                                        $"Slot Status:{((HomeAddressType)item.Status).ToString()} not in {nameof(SlotStatus.Out)},{nameof(SlotStatus.InAndOut)} ");
                                }

                            }
                            else
                            {
                                if (!new int[] { (int)SlotStatus.In, (int)SlotStatus.InAndOut }
                                .Any(p => p == (int)slot.Status))
                                {
                                    Result.Add(Resource.WAREHOUSE_CHECK_SLOT_STATAUS +
                                        $"Slot Status:{((HomeAddressType)item.Status).ToString()} not in {nameof(SlotStatus.In)},{nameof(SlotStatus.InAndOut)} ");
                                }
                            }
                        }
                        else
                        {

                        }
                    }
                }
                if (Result.Count > 0)
                {
                    rs.Success = true;
                    rs.Content = false;
                    rs.Message = string.Join(",", Result);
                }
                else
                {
                    rs.Success = true;
                    rs = this.HomeAddressRelationRepository.Insert(models);
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
            }
            return rs;
        }

        public IActionResult<bool> DeleteHomeAddress(IEnumerable<Guid> homeAddressUID)
        {
            return this.HomeAddressRelationRepository.Delete(homeAddressUID);
        }

        public IActionResult<IEnumerable<IHomeAddressRelationAreaModel>> GetHomeAddressAreaList(Guid warehouseUID, int homeAddressType)
        {
            return this.HomeAddressRelationRepository.GetHomelocationAreaList(warehouseUID, homeAddressType);
        }

        public IActionResult<IEnumerable<IHomeAddressRelationBinModel>> GetHomeAddressBinList(Guid areaUID, int homeAddressType)
        {
            return this.HomeAddressRelationRepository.GetHomelocationBinList(areaUID, homeAddressType);
        }

        public IActionResult<IEnumerable<IHomeAddressReltationSlotModel>> GetHomeAddressSlotList(Guid binUID, int homeAddressType)
        {
            return this.HomeAddressRelationRepository.GetHomelocationSlotList(binUID, homeAddressType);
        }

        public IActionResult<IEnumerable<IHomeAddressRelationListModel>> GetHomeAddressList(IGetHomeAddressListParameters parameters)
        {
            return this.HomeAddressRelationRepository.GetHomeAddressList(parameters);
        }
    }
}
