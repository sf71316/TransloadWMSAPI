using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;
using YAEP.WMS.Constant.Enums;

namespace YAEP.WMS.Interfaces
{
    public interface IHomeAddressRelationRepository
    {
        IActionResult<IEnumerable<IHomeAddressRelationModel>> GetData(IEnumerable<Guid> ItemUIDs,
            int homeAddressType, int? homeAddressOutboundType);
        IActionResult<bool> Insert(IEnumerable<IHomeAddressRelationModel> homeAddressRelationModels);
        IActionResult<bool> Delete(IEnumerable<Guid> homeAddressRelationUIDs);
        IActionResult<bool> ClearAll();
        IActionResult<IEnumerable<IHomeAddressRelationAreaModel>> GetHomelocationAreaList(Guid warehouseUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressRelationBinModel>> GetHomelocationBinList(Guid areaUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressReltationSlotModel>> GetHomelocationSlotList(Guid binUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressRelationListModel>> GetHomeAddressList(IGetHomeAddressListParameters parameters);
    }
}
