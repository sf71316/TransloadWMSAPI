using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YAEP.WMS.Interfaces.Model
{
    public interface IPackageExtendModel
    {
        /// <summary>
        /// 
        /// </summary>
         Guid UID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        //public string ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
         string VersionId { get; set; }
    }
}
