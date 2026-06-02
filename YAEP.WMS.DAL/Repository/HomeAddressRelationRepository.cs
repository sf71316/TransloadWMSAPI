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
using YAEP.WMS.DAL.Model;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL.Repository
{
    public class HomeAddressRelationRepository<T> : AbstractRepository<T>, IHomeAddressRelationRepository
         where T : class, IHomeAddressRelationModel
    {
        private readonly IAuthenticationProvider _AuthenticationProvider;
        public HomeAddressRelationRepository(IRepositoryHandler<T> handler, IAuthenticationProvider authenticationInfoProvider)
            : base(handler)
        {
            this._Handler.IsAutoHandleError = false;
            this._AuthenticationProvider = authenticationInfoProvider;
        }

        public IActionResult<bool> ClearAll()
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                var query = "UPDATE WMS_HomeAddressRelation SET Status=0";
                rs.Content = this._Handler.Instance.Execute(query) > 0;
                rs.Success = rs.Content;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<bool> Delete(IEnumerable<Guid> homeAddressRelationUIDs)
        {

            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                rs.Content = this._Handler.DeleteByDynamicConditions(new { UID = homeAddressRelationUIDs });
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IEnumerable<IHomeAddressRelationModel>> GetData(IEnumerable<Guid> ItemUIDs,
            int homeAddressType, int? homeAddressOutboundType)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IHomeAddressRelationModel>>();
            try
            {
                var query = @"SELECT * FROM WMS_HomeAddressRelation WHERE Status>0 AND Type=@Type AND 
                            OutboundType=@OutboundType  AND ItemUID in @ItemUIDs ";
                rs.Content = this._Handler.Instance.Query<HomeAddressRelationInnerModel>(query,
                    new { ItemUIDs = ItemUIDs, Type = homeAddressType, OutboundType = homeAddressOutboundType });
                rs.Success = true;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IEnumerable<IHomeAddressRelationListModel>> GetHomeAddressList(IGetHomeAddressListParameters parameters)
        {

            var rs = ActionResultTemplates.Result<IEnumerable<IHomeAddressRelationListModel>>();
            try
            {
                var query = @"SELECT [HAR].UID,[Warehouse].Name WarehouseName,[Area].UID AreaUID,[Area].Name AreaName,
                [Bin].UID as BinUID,[Bin].Name BinName,
                [Slot].UID as SlotUID,[Slot].Name SlotName,
                [HAR].Type,[HAR].OutboundType,[HAR].Sequence Priority
                FROM WMS_HomeAddressRelation AS [HAR]
                INNER JOIN WMS_Slot AS [Slot] ON [Slot].UID=[HAR].SlotUID 
                INNER JOIN WMS_Area AS [Area] ON [Area].UID=[Slot].AreaUID
                INNER JOIN WMS_Bin AS [Bin] ON [Bin].UID=[Slot].BinUID
                INNER JOIN WMS_Warehouse AS [Warehouse] ON [Warehouse].UID=[Area].WarehouseUID
                WHERE [HAR].ItemUID=@ItemUID AND [HAR].Status>0
                ORDER BY [Warehouse].Name,[HAR].Type,[HAR].OutboundType,[HAR].Sequence
                 ";
                rs.Content = this._Handler.Instance.Query<HomeAddressRelationListModel>(query, parameters);
                rs.Success = true;
                foreach (var item in rs.Content)
                {
                    item.OutboundTypeName =
                        item.OutboundType.HasValue ?
                        (item.OutboundType.Value == 0) ? "" : ((HomeAddressOutboundType)item.OutboundType).ToString()
                        : "";
                    item.TypeName = ((HomeAddressType)item.Type).ToString();
                }
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }

        public IActionResult<IEnumerable<IHomeAddressRelationAreaModel>> GetHomelocationAreaList(Guid warehouseUID, int homeAddressType)
        {
            SlotStatus[] _slotStatus = null;
            if (homeAddressType == (int)HomeAddressType.Storage)
            {
                _slotStatus = new SlotStatus[] { SlotStatus.In, SlotStatus.InAndOut };
            }
            else
            {
                _slotStatus = new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut };
            }
            var result = this.GetSlotInfo(_slotStatus, warehouseUIDs: new Guid[] { warehouseUID });
            var rs = ActionResultTemplates.Result<IEnumerable<IHomeAddressRelationAreaModel>>();
            rs.Content = result?.GroupBy(p => new { AreaUID = p.AreaUID, AreaName = p.AreaName })
                .Select(x =>
                new HomeAddrAreaModel { AreaName = x.Key.AreaName, AreaUID = x.Key.AreaUID } as IHomeAddressRelationAreaModel);
            rs.Success = true;
            return rs;
        }

        public IActionResult<IEnumerable<IHomeAddressRelationBinModel>> GetHomelocationBinList(Guid areaUID, int homeAddressType)
        {
            SlotStatus[] _slotStatus = null;
            if (homeAddressType == (int)HomeAddressType.Storage)
            {
                _slotStatus = new SlotStatus[] { SlotStatus.In, SlotStatus.InAndOut };
            }
            else
            {
                _slotStatus = new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut };
            }
            var result = this.GetSlotInfo(_slotStatus, areaUIDs: new Guid[] { areaUID });
            var rs = ActionResultTemplates.Result<IEnumerable<IHomeAddressRelationBinModel>>();
            rs.Content = result?.GroupBy(p => new { BinUID = p.BinUID, BinName = p.BinName })
                .Select(x =>
                new HomeAddrBinModel { BinName = x.Key.BinName, BinUID = x.Key.BinUID } as IHomeAddressRelationBinModel);
            rs.Success = true;
            return rs;
        }

        public IActionResult<IEnumerable<IHomeAddressReltationSlotModel>> GetHomelocationSlotList(Guid binUID, int homeAddressType)
        {
            SlotStatus[] _slotStatus = null;
            if (homeAddressType == (int)HomeAddressType.Storage)
            {
                _slotStatus = new SlotStatus[] { SlotStatus.In, SlotStatus.InAndOut };
            }
            else
            {
                _slotStatus = new SlotStatus[] { SlotStatus.Out, SlotStatus.InAndOut };
            }
            var result = this.GetSlotInfo(_slotStatus, binUIDs: new Guid[] { binUID });
            var rs = ActionResultTemplates.Result<IEnumerable<IHomeAddressReltationSlotModel>>();
            rs.Content = result?.GroupBy(p => new { SlotUID = p.SlotUID, SlotName = p.SlotName })
                .Select(x =>
                new HomeAddrSlotModel { SlotName = x.Key.SlotName, SlotUID = x.Key.SlotUID } as IHomeAddressReltationSlotModel);
            rs.Success = true;
            return rs;
        }
        private IEnumerable<SlotInfoModel> GetSlotInfo(SlotStatus[] slotStatus, IEnumerable<Guid> areaUIDs = null,
            IEnumerable<Guid> binUIDs = null, IEnumerable<Guid> warehouseUIDs = null)
        {
            var rs = ActionResultTemplates.Result<IEnumerable<SlotInfoModel>>();
            try
            {
                var query = @"SELECT 
                [Slot].UID SlotUID,[Slot].Name SlotName,
                [Area].UID AreaUID,[Area].Name AreaName,
                [Bin].UID BinUID,[Bin].Name BinName
                FROM  WMS_Slot AS [Slot]
                INNER JOIN WMS_Area AS [Area] ON [Area].UID=[Slot].AreaUID
                INNER JOIN WMS_Bin AS [Bin] ON [Bin].UID=[Slot].BinUID
                INNER JOIN WMS_Warehouse AS [Warehouse] ON [Warehouse].UID=[Area].WarehouseUID
                WHERE {0} AND [Slot].Status IN @Status AND [Slot].Status>0 AND [Area].Status>0 AND [Bin].Status>0
                
                 ";
                if (areaUIDs != null)
                {
                    query = string.Format(query, "[Slot].AreaUID IN @AreaUID");
                }
                else if (binUIDs != null)
                {
                    query = string.Format(query, "[Slot].BinUID IN @BinUID");
                }
                else if (warehouseUIDs != null)
                {
                    query = string.Format(query, "[Area].WarehouseUID IN @WarehouseUID");
                }
                rs.Content = this._Handler.Instance.Query<SlotInfoModel>(query, new
                {
                    AreaUID = areaUIDs,
                    BinUID = binUIDs,
                    WarehouseUID = warehouseUIDs,
                    Status = slotStatus
                });
                rs.Success = true;

            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs.Content;
        }
        public IActionResult<bool> Insert(IEnumerable<IHomeAddressRelationModel> homeAddressRelationModels)
        {
            var rs = ActionResultTemplates.Result<bool>();
            try
            {
                this._Handler.Instance.Connection.Open();
                rs.Content = this._Handler.BatchCreateByDynamic(homeAddressRelationModels, "WMS_HomeAddressRelation");
                this._Handler.Instance.Connection.Close();
                rs.Success = rs.Content;
            }
            catch (Exception ex)
            {
                rs.Message = ex.Message;
                rs.TypeCode = FlowStatusCode.OCCUR_EXCEPTION;
                rs.Success = false;
                rs.InnerException = ex;
                this.OnExpcetion(ex);
            }
            return rs;
        }
    }
}
