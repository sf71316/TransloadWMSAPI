using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace YAEP.WMS.API
{
    public partial class RecoveryAllocatedData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.password == "896fcbv!")
            {
                Process();
                Response.Write("OK");
            }
            else
                Response.Write("Failure");
        }

        private void Process()
        {
            var warehouseuid = "35C8D835-D584-41FD-8669-B48623196B8F";
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            using (var tran = new CommittableTransaction())
            {
                using (var cn = new SqlConnection(cs))
                {

                    cn.Open();
                    var cmd = cn.CreateCommand();
                    cmd.CommandText = @"
update WMS_PayLoad set Type=1 where UID in (
select
 wp.UID
from WMS_PayLoad wp
left join WMS_WorkOrder_Payload wwopl on wp.UID=wwopl.PayloadUID
left join WMS_WorkOrder wwo  on wwo.UID=wwopl.WorkOrderUID
left join WMS_Manifest wm on wm.UID=wwo.ManifestUID
LEFT join WMS_Vessel wv on wwo.VesselUID=wv.UID
LEFT join WMS_TicketInfo wti on wti.WorkOrderPayloadUID=wwopl.UID
inner join YAEP_Item yi on yi.UID=wp.ItemUID
inner join WMS_Slot ws on ws.UID=wp.SlotUID and ws.WarehouseUID=@warehouseUID
where 
wp.Type=2 and wp.Status =500 and (wm.UID is null  or wv.UID is null)
)
update WMS_WorkOrder_Payload set Status=0 where WorkOrderUID in (
select
 wwo.UID
from WMS_PayLoad wp
left join WMS_WorkOrder_Payload wwopl on wp.UID=wwopl.PayloadUID
left join WMS_WorkOrder wwo  on wwo.UID=wwopl.WorkOrderUID
left join WMS_Manifest wm on wm.UID=wwo.ManifestUID
LEFT join WMS_Vessel wv on wwo.VesselUID=wv.UID
LEFT join WMS_TicketInfo wti on wti.WorkOrderPayloadUID=wwopl.UID
inner join YAEP_Item yi on yi.UID=wp.ItemUID
inner join WMS_Slot ws on ws.UID=wp.SlotUID and ws.WarehouseUID=@warehouseUID
where 
wp.Type=2 and wp.Status =500 and (wm.UID is null  or wv.UID is null)
)

update WMS_WorkOrder_Pod set Status=0 where WorkOrderUID in (
select
 wwo.UID
from WMS_PayLoad wp
left join WMS_WorkOrder_Payload wwopl on wp.UID=wwopl.PayloadUID
left join WMS_WorkOrder wwo  on wwo.UID=wwopl.WorkOrderUID
left join WMS_Manifest wm on wm.UID=wwo.ManifestUID
LEFT join WMS_Vessel wv on wwo.VesselUID=wv.UID
LEFT join WMS_TicketInfo wti on wti.WorkOrderPayloadUID=wwopl.UID
inner join YAEP_Item yi on yi.UID=wp.ItemUID
inner join WMS_Slot ws on ws.UID=wp.SlotUID and ws.WarehouseUID=@warehouseUID
where 
wp.Type=2 and wp.Status =500 and (wm.UID is null  or wv.UID is null)
)
update WMS_WorkOrder set Status=0 where uid in (
select
 wwo.UID
from WMS_PayLoad wp
left join WMS_WorkOrder_Payload wwopl on wp.UID=wwopl.PayloadUID
left join WMS_WorkOrder wwo  on wwo.UID=wwopl.WorkOrderUID
left join WMS_Manifest wm on wm.UID=wwo.ManifestUID
LEFT join WMS_Vessel wv on wwo.VesselUID=wv.UID
LEFT join WMS_TicketInfo wti on wti.WorkOrderPayloadUID=wwopl.UID
inner join YAEP_Item yi on yi.UID=wp.ItemUID
inner join WMS_Slot ws on ws.UID=wp.SlotUID and ws.WarehouseUID=@warehouseUID
where 
wp.Type=2 and wp.Status =500 and (wm.UID is null  or wv.UID is null)
)
                    ";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "warehouseUID";
                    param.Value = warehouseuid;
                    param.DbType = System.Data.DbType.Guid;
                    cmd.Parameters.Add(param);
                    var dr = cmd.ExecuteNonQuery();
                    Response.Write("<li>Effect count:</li>");
                    Response.Write("<li>" + dr + "</li>");
                }
            }
        }

        public string password
        {
            get
            {
                return Request.QueryString["pd"];

            }
        }
    }
}