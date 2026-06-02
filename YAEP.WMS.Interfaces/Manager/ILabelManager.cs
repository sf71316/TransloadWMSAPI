using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ILabelManager
    {
        IActionResult<IEnumerable<ILabelGenerateViewModel>> GenerateLabel(IGenerateLabelRequest request);
        IActionResult<bool> AddLabels(ILabelModel[] Models);
        IActionResult<bool> DeleteLabel(Guid[] belongtouid);
        IActionResult<IEnumerable<ITicketLabelViewModel>> GetBelongtoBarcode(Guid[] BelongtoUID);
        IActionResult<bool> ChangeLabelStatus(Guid[] BarcodeUID, LabelStatus status);
        /// <summary>
        /// 修改Label 狀態 使用前需注意該方法無視delete 狀態一律修改
        /// </summary>
        /// <param name="BelongtoUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        IActionResult<bool> ChangeLabelStatusByBelongtoUID(IEnumerable<Guid> BelongtoUID, LabelStatus status);
        void DeleteAttachment(IEnumerable<Guid> enumerable);
        IActionResult<bool> CloneLabel(Guid sourceBelongToUID, Guid TargetBelongToUID);
        /// <summary>
        /// 複製並回傳指定belongto Label
        /// </summary>
        /// <param name="sourceBelongToUID"></param>
        /// <param name="TargetBelongToUID"></param>
        /// <returns></returns>
        IActionResult<IEnumerable<ILabelModel>> ReturnCloneLabel(Guid sourceBelongToUID, Guid TargetBelongToUID);
        IActionResult<IEnumerable<ILabelModel>> BatchReturnCloneLabel(IEnumerable<ICloneLabelModel> cloneLabelModels);
        IActionResult<bool> GenerateItemLabel(Guid itemUID, string barcode, LabelType labelType);
        IActionResult<bool> GenerateItemLabel(IEnumerable<ILabelModel> labels);
        IActionResult<bool> GenerateItemLabel(IEnumerable<dynamic> itemsparam);
        IActionResult<ILabelGenerateViewModel> GeneratePodLabel(IGeneratePalletLabelModel palletModel, Guid podUID);
        IActionResult<ILabelGenerateViewModel> GenerateGeneralLabel(string barCode, string LabelText, BarcodeType barcodeType, LabelType labelType, LabelBelongType labelBelongType, Guid belongToUID);
        IActionResult<bool> RollbackLabel(IEnumerable<Guid> TicketUIDs);
        IActionResult<bool> ClearLabelByTickets(IEnumerable<Guid> TicketUIDs);
        IActionResult<bool> AttachItemLabelAPI(IEnumerable<Guid> payloaduid);
    }
}
