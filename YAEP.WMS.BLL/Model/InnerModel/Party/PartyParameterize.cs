using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Core.Party.Constants;
using YAEP.Core.Party.Interfaces;

namespace YAEP.WMS.BLL.Model
{
    internal class PartyParameterize : IPartyParameterize
    {
        public PartyParameterize()
        {
        }

        public Guid? UID { get; set; }
        public Guid? GroupUID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int? Status { get; set; }
        public PartyTypeCategories? PartyTypeCategory { get; set; }
        public List<Guid> PartyTypes { get; set; } = new List<Guid>();
        public List<Guid> ListOfPartyUID { get; set; } = new List<Guid>();
        public List<Guid> ListOfGroupUID { get; set; } = new List<Guid>();
    }
}
