using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Interfaces;

namespace YAEP.WMS.Interfaces
{
    public interface IHomeAddressRelationManager
    {

        IActionResult<bool> AddHomeAddress(IEnumerable<IHomeAddressRelationModel> model);
        IActionResult<bool> DeleteHomeAddress(IEnumerable<Guid> homeAddressUID);
        IActionResult<IEnumerable<IHomeAddressRelationAreaModel>> GetHomeAddressAreaList(Guid warehouseUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressRelationBinModel>> GetHomeAddressBinList(Guid areaUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressReltationSlotModel>> GetHomeAddressSlotList(Guid binUID, int homeAddressType);
        IActionResult<IEnumerable<IHomeAddressRelationListModel>> GetHomeAddressList(IGetHomeAddressListParameters parameters);
    }
}
