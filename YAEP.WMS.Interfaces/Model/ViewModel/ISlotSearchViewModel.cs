using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    public interface ISlotSearchViewModel : ISlotModel
    {
        string WarehouseID { get; set; }
        /// <summary>
        /// Warehouse Name
        /// <para /><see cref="IWarehouseModel.Name"/>
        /// </summary>
        string WarehouseName { get; set; }
        /// <summary>
        /// Area ID
        /// <para /><see cref="IAreaModel.ID"/>
        /// </summary>
        string AreaID { get; set; }
        /// <summary>
        /// Area Name
        /// <para /><see cref="IAreaModel.Name"/>
        /// </summary>
        string AreaName { get; set; }
        /// <summary>
        /// Bin ID
        /// <para /><see cref="IBinModel.ID"/>
        /// </summary>
        string BinID { get; set; }
        /// <summary>
        /// Bin Name
        /// <para /><see cref="IBinModel.Name"/>
        /// </summary>
        string BinName { get; set; }
        string SlotStatusName { get; set; }
        string SlotTypeName { get; set; }
    }

}
