using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;
using YAEP.WMS.Interfaces;

namespace YAEP.WMS.Model
{
    [Serializable()]
    [Table("WMS_Api")]
    [DbTable("WMS_Api")]
    public class ApiModel : IApiModel
    {
        public Guid UID {get;set;}
        public Guid WarehouseUID {get;set;}
        public string ApiKey {get;set;}
        public bool IsEnable {get;set;}
        public bool Https {get;set;}
        public string CreatedBy {get;set;}
        public DateTime CreatedOn {get;set;}
        public string ModifiedBy {get;set;}
        public DateTime ModifiedOn {get;set;}
    }
}
