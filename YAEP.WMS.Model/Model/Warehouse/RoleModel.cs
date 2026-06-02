using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAEP.Data.ORM.Attributes;

namespace YAEP.WMS.Model.Model
{
    /// <summary>
    /// 角色
    /// </summary>
    [Serializable()]
    [Dapper.Contrib.Extensions.Table("YAEP_Role")]
    [DbTable("YAEP_Role")]
    public partial class RoleModel
    {
        public RoleModel() : this(false) { }
        public RoleModel(bool IsNew)
        {
            if (IsNew)
            {
                this.UID = Guid.NewGuid();
            }
        }

        #region Properties 

        /// <summary>
        /// 識別碼
        /// </summary>
        [Dapper.Contrib.Extensions.ExplicitKey]
        [DbColumn("UID", IsPrimaryKey = true)]
        public Guid UID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Dapper.Contrib.Extensions.ExplicitKey]
        public String ID { get; set; }

        /// <summary>
        /// 名稱
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public String Description { get; set; }

        /// <summary>
        /// 角色類型：
        /// None = 0,
        /// User = 1,
        /// Group = 2
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 資料狀態：
        /// 刪除 = 0,
        /// 正常 = 1 (Default),
        /// 未審核 = 2,
        /// 停用 = 3
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public String CreatedBy { get; set; }

        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public String ModifiedBy { get; set; }

        /// <summary>
        /// 異動日期
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid? Own { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDefault { get; set; }

        #endregion
    }


    [Serializable()]
    public partial class RoleCollection : List<RoleModel>
    {
        public RoleCollection() : base() { }
        public RoleCollection(IEnumerable<RoleModel> collection) : base(collection) { }


    }
}
