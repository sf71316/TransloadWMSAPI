using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;

namespace YAEP.WMS.API
{
    public partial class TranTest : System.Web.UI.Page
    {
        const string expireTime = "999911161200";
        protected void Page_Load(object sender, EventArgs e)
        {


            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            //using (var tx = new TransactionScope())
            //{
            //    ExecQuery(cs);
            //    //經實測，11.2 連線字串相同的兩條連線也觸發分散式交易
            //    //12.1 連線字串要故意做出差異才會進入分散式交易模式
            //    cs += "";
            //    ExecQuery(cs);
            //    Response.Write(
            //        "<li>TransId=" +
            //        Transaction.Current.TransactionInformation.DistributedIdentifier);
            //}
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                var tran = cn.BeginTransaction();
                try
                {

                    List<Guid> cc = new List<Guid>();
                    var sqlquery = "INSERT INTO [temp]4 (UID) values(@UID)";
                    for (int i = 0; i < 3000; i++)
                    {
                        cc.Add(Guid.NewGuid());
                    }
                    var rs = cn.Execute(sqlquery, new { UID = cc }, transaction: tran);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                }
            }
        }
        void ExecQuery(string cs)
        {
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "select newid() ";
                var dr = cmd.ExecuteReader();
                dr.Read();
                Response.Write("<li>" + new Guid(dr[0].ToString()));
            }
        }
        protected void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                //if (DateTime.Now.ToString("yyyyMMddHHmm").CompareTo(expireTime) > 0)
                //    throw new Exception("Tool Expired");
                if (this.RadioButtonList1.SelectedValue == "1")
                    TestDtc();
                else
                    TestCtc();
            }
            catch (Exception ex)
            {
                preDisplay.Attributes["class"] = "err";
                preDisplay.InnerHtml = "**ERROR**\n" + ex.ToString();
            }
        }

        private void TestCtc()
        {
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            using (var tran = new CommittableTransaction())
            {
                using (var cn = new SqlConnection(cs))
                {

                    cn.Open();
                    var cmd = cn.CreateCommand();
                    cmd.CommandText = "select newid() ";
                    var dr = cmd.ExecuteReader();
                    dr.Read();
                    Response.Write("<li>" + new Guid(dr[0].ToString()));
                }
            }
        }

        void TestDtc()
        {
            //Action<string, string> validate =
            //    (v, n) => { if (string.IsNullOrEmpty(v)) throw new ArgumentException(n + " is null or empty."); };
            //validate(dataSrc, "Data Source");
            //validate(uid, "User Id");
            //validate(pwd, "Password");
            //var scsb = new SqlConnectionStringBuilder();
            //scsb.DataSource = dataSrc;
            //scsb.UserID = uid;
            //scsb.Password = pwd;
            var cs = System.Configuration.ConfigurationManager.ConnectionStrings["YAEP.WMS.ConnectString"].ConnectionString;
            TestDtc(cs);
        }
        void TestDtc(string cnStr)
        {
            using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromSeconds(10)))
            {
                var css = "normal";
                var sb = new StringBuilder();
                sb.AppendFormat("Test DTC at {0:HH:mm:ss.fff}\n", DateTime.Now);
                sb.AppendLine(querySqlServer(cnStr));
                sb.AppendLine(querySqlServer(cnStr));
                var txInfo = Transaction.Current.TransactionInformation;
                sb.AppendLine("Local Id = " + txInfo.LocalIdentifier);
                sb.AppendLine("Distributed Id = " + txInfo.DistributedIdentifier);
                if (txInfo.DistributedIdentifier != Guid.Empty)
                {
                    sb.AppendLine("*** TEST PASSED ****");
                    css = "pass";
                }
                preDisplay.Attributes["class"] = css;
                preDisplay.InnerHtml = sb.ToString();
                tx.Complete();
                //using (var cn = new SqlConnection(cnStr))
                //{

                //    cn.Open();
                //    var cmd = cn.CreateCommand();
                //    cmd.CommandTimeout = 1;
                //    cmd.CommandText = $"INSERT INTO TEST (Value) VALUES('{DateTime.Now.ToString("yyyyMMddHHmmssfff")}') ";
                //    var dr = cmd.ExecuteNonQuery();
                //    cn.Close();
                //}
                //tx.Complete();
            }
        }
        private string querySqlServer(string cnStr)
        {
            cnStr = cnStr.TrimEnd(';') + ";Application Name=" + Guid.NewGuid().ToString();
            using (SqlConnection cn = new SqlConnection(cnStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT getdate() as D", cn);
                cn.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                var res = "GetDate = " + dr["D"];
                cn.Close();
                return res;
            }
        }
    }
}