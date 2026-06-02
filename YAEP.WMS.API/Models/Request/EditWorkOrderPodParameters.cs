using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.API.Models.Request
{
    public class EditWorkOrderPodParameters : IEditWorkOrderPodParameters
    {
        public Guid UID { get; set; }
        public string Name { get; set; }
        public int? Type { get; set; }
        public Guid? ContainerType { get; set; }
        public string OperationSuggestion { get; set; }
    }
}