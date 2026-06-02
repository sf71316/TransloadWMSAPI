using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YAEP.Core.Party.Constants;

namespace YAEP.WMS.API.Models.Request
{
    /// <summary>
    /// 
    /// </summary>
    public class CustomerAddressRequestModel
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        public Guid UID { get; set; }
        /// <summary>
        /// Party 識別碼
        /// </summary>
        public Guid PartyUID { get; set; }
        /// <summary>
        /// 代碼
        /// </summary> 
        public string ID { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 類型
        /// </summary>
        public int? Type { get; set; } = (int)PartyAddressTypes.Unavailable;
        /// <summary>
        /// 狀態 (0:Inactive 1:Active)
        /// </summary>
        public int Status { get; set; } = (int)PartyAddressStatus.Active;
        /// <summary>
        /// 國家 (3 codes)
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 州 / 省 (2 codes)
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 區號
        /// </summary>
        public string Zip { get; set; }
        /// <summary>
        /// 地址1
        /// </summary>
        public string Address1 { get; set; }
        /// <summary>
        /// 地址2
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 地址3
        /// </summary>
        public string Address3 { get; set; }
        /// <summary>
        /// 電子郵件
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 市內電話號碼
        /// </summary>
        public string PhoneHome { get; set; }
        /// <summary>
        /// 手機電話號碼
        /// </summary>
        public string PhoneCell { get; set; }
        /// <summary>
        /// 辦公室電話號碼
        /// </summary>
        public string PhoneOffice { get; set; }
        /// <summary>
        /// 傳真
        /// </summary>
        public string FaxHome { get; set; }
        /// <summary>
        /// 傳真
        /// </summary>
        public string FaxOffice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsDefault { get; set; } = false;
    }

}