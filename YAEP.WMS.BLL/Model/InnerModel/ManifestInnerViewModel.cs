using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.WMS.Constant.Enums;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class ManifestInnerViewModel : IManifestViewModel
    {
        public ManifestInnerViewModel(IManifestModel original)
        {
            if (original != null)
            {
                this.UID = original.UID;
                this.CreatedBy = original.CreatedBy;
                this.CreatedOn = original.CreatedOn;
                this.Description = original.Description;
                this.ID = original.ID;
                this.ModifiedBy = original.ModifiedBy;
                this.ModifiedOn = original.ModifiedOn;
                this.Name = original.Name;
                this.PartyUID = original.PartyUID;
                this.RefNo = original.RefNo;
                this.Status = original.Status;
                this.Type = original.Type;
                this.Volume = original.Volume;
                this.WarehouseUID = original.WarehouseUID;
                this.Weight = original.Weight;
            }
        }
        public string StatusName { get; set; }
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public Guid WarehouseUID { get; set; }
        public Guid PartyUID { get; set; }
        public string RefNo { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Weight { get; set; }
        public ManifestStatus Status { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
