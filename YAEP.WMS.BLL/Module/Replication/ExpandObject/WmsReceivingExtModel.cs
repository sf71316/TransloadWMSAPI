using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.LittleBird.CapPBSC.Models;
using YAEP.WMS.BLL.Interfaces;

namespace YAEP.WMS.BLL.Module
{
    internal class WmsReceivingExtModel : WmsReceivingModel, IWMSReplicateExtModel
    {
        public Guid ReplicateKey { get; set; }
        public Guid ReplicateUID { get; set; }
    }
}
