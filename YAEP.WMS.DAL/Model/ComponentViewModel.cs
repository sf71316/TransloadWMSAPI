using System;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.DAL
{
    internal class ComponentViewModel : IComponentViewModel
    { 
        public Guid UID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
    }
}
