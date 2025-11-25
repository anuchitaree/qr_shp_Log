using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace qr_shp_Log
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var context = new rawscaEntities())
            {
                var alldata = context.qr_shipping_log_org.ToList();

                var qr_shippping_log_mod = new List<qr_shipping_log_mod>();
                foreach (var item in alldata)
                {

                    var trdate = DateTime.ParseExact(Convert.ToString(item.TR_DATE_YMD), "yyyyMMdd", null);
                    var scandate = DateTime.ParseExact(Convert.ToString(item.SCAN_DATE_YMD), "yyyyMMdd", null);
                    var strtime = item.SCAN_TIME.ToString().PadLeft(6, '0');
                    var timepart = TimeSpan.ParseExact(strtime, "hhmmss", null);
                    DateTime scandatetime = scandate.Date.Add(timepart);

                    var record = new qr_shipping_log_mod
                    {
                        ID = item.ID,
                        SEQ = item.SEQ,
                        LOGIN_ID = item.LOGIN_ID,
                        TR_DATE_YMD = trdate,
                        TRUCK_NO = item.TRUCK_NO,
                        CUST_NO = item.CUST_NO,
                        TAG_TYPE = item.TAG_TYPE,
                        P_N = item.P_N,
                        CUST_P_N = item.CUST_P_N,
                        QTY = item.QTY,
                        TAG_SEQ = item.TAG_SEQ,
                        SCAN_DATE_YMD= scandatetime,
                       Status=false,
                       uuid="",
                    };

                    qr_shippping_log_mod.Add(record);
                }
                context.qr_shipping_log_mod.AddRange(qr_shippping_log_mod.OrderBy(x=>x.SCAN_DATE_YMD).ToList());
                context.SaveChanges();
                MessageBox.Show("Data migration completed.");

                //var qr_shippping_log_orderby = qr_shippping_log1.OrderBy(x => x.SCAN_DATE_YMD_Time).ToList();





            }




        }
    }
}
