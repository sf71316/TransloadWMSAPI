using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IComponentViewModel
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        Guid UID { get; set; }
        /// <summary>
        /// 顯示代碼
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// 顯示名稱
        /// </summary>
        string Name { get; set; }
    }
}
