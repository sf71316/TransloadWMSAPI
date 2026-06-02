using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface ILabelRepository
    {

        IActionResult<bool> AddLabelCollection(ILabelModel[] Models);
        IActionResult<bool> DeleteLabel(Guid[] uid);
        IActionResult<IEnumerable<ILabelModel>> GetLabels(Guid[] belongtoUIDs);
        IActionResult<IEnumerable<ILabelModel>> GetLabels(object conditions);
        IActionResult<bool> ExistByBarcode(string barcodeContent, int LabelType);
        IActionResult<bool> ChangeLabelStatus(Guid[] BarcodeUID, LabelStatus status);
        IActionResult<bool> ChangeLabelStatus(string[] Barcode, LabelStatus status);
        IActionResult<bool> ChangeLabelStatus(Guid BelongToUID, string[] Barcode, LabelStatus status);
        IActionResult<bool> ChangeLabelStatus(Guid[] BarcodeUID, LabelStatus status, string description);
        IActionResult<bool> ChangeLabelBelongToUID(Guid[] BarcodeUID, Guid belongToUID);
        IActionResult<IEnumerable<ILabelModel>> GetLabelsByTicket(IEnumerable<Guid> TicketUIDs);
        /// <summary>
        /// 修改Label 狀態 使用前需注意該方法無視delete 狀態一律修改
        /// </summary>
        /// <param name="BelongToUID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        IActionResult<bool> ChangeLabelStatusByBelongToUID(IEnumerable<Guid> BelongToUID, LabelStatus status, string modifiedBy = "");
        IActionResult<IEnumerable<ILabelModel>> BatchReturnCloneLabel(IEnumerable<ICloneLabelModel> cloneLabelModels);
    }
}
