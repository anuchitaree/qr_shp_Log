using Newtonsoft.Json;
using qr_shp_Log.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
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
            try
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
                            W_H = item.W_H,
                            DEV_ID = item.DEV_ID,
                            LOGIN_ID = item.LOGIN_ID,
                            TR_DATE_YMD = trdate,
                            TR_TYPE = item.TR_TYPE,
                            OPE_SEQ = item.OPE_SEQ,
                            RESULT = item.RESULT,
                            RSN_CODE = item.RSN_CODE,
                            TRUCK_NO = item.TRUCK_NO,
                            CUST_NO = item.CUST_NO,
                            CHECK_TYPE = item.CHECK_TYPE,
                            SHIP_NO = item.SHIP_NO,
                            SHIP_P_N = item.SHIP_P_N,
                            SHIP_QTY = item.SHIP_QTY,
                            TAG_TYPE = item.TAG_TYPE,
                            P_N = item.P_N,
                            CUST_P_N = item.CUST_P_N,
                            QTY = item.QTY,
                            TAG_SEQ = item.TAG_SEQ,
                            SCAN_DATE_YMD = scandatetime,
                            status = 0,
                            point = 0,
                            uuid = "",
                        };

                        qr_shippping_log_mod.Add(record);
                        //context.qr_shipping_log_mod.AddRange(qr_shippping_log_mod.OrderBy(x => x.SCAN_DATE_YMD).ToList());
                        //context.SaveChanges();
                    }
                    context.qr_shipping_log_mod.AddRange(qr_shippping_log_mod.OrderBy(x => x.SCAN_DATE_YMD).ToList());
                    context.SaveChanges();
                    MessageBox.Show("Data migration completed.");







                }



            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var truck_error = 0;
            try
            {
                var uuid = 0;
                using (var context = new rawscaEntities())
                {
                    var qr_shp_logs = context.qr_shipping_log_mod
                        .Where(x => x.RESULT == "0").Where(x => x.TAG_TYPE != null).ToList();

                    var tr_date_grp = qr_shp_logs.GroupBy(x => x.TR_DATE_YMD).Select(x => new grp_tr_date { tr_date = x.Key }).ToList();
                    foreach (var row in tr_date_grp.OrderBy(a => a.tr_date).ToList())
                    {
                        Console.WriteLine($"{row.tr_date}");

                        var truck_grp = qr_shp_logs.Where(t => t.TR_DATE_YMD == row.tr_date)
                            .GroupBy(t => t.TRUCK_NO).Select(t => new grp_truk { truck = (int)t.Key }).ToList();
                        //var json_string = JsonConvert.SerializeObject(truck_grp);

                        //var text = "";
                        var qr_shp_log_by_truck = new List<qr_shipping_log_mod>();
                        foreach (var item in truck_grp)
                        {
                            //text += item.truck + "\n";

                            qr_shp_log_by_truck = context.qr_shipping_log_mod.Where(x => x.RESULT == "0")
                                .Where(t => t.TR_DATE_YMD == row.tr_date).Where(t => t.TRUCK_NO == item.truck)
                                .Where(t => t.TAG_TYPE != null).OrderBy(s => s.SCAN_DATE_YMD).ToList();

                            var count = qr_shp_log_by_truck.Count;

                            truck_error = item.truck;

                            for (var i = 0; i < qr_shp_log_by_truck.Count; i++)
                            {

                                var check_type = qr_shp_log_by_truck[i].CHECK_TYPE;
                                switch (check_type)
                                {
                                    case null: // 2 point check
                                        qr_shp_log_by_truck[i].status = 1;
                                        qr_shp_log_by_truck[i].point = 2;
                                        qr_shp_log_by_truck[i].uuid = uuid++.ToString();
                                        break;

                                    case "1":  // 3 point check
                                        try
                                        {


                                            if (qr_shp_log_by_truck[i].TAG_TYPE == "21" && (qr_shp_log_by_truck[i + 1].TAG_TYPE == "UR" || qr_shp_log_by_truck[i + 1].TAG_TYPE == "CS"))
                                            {
                                                var uid = uuid++.ToString();
                                                qr_shp_log_by_truck[i].status = 1;
                                                qr_shp_log_by_truck[i].point = 3;
                                                qr_shp_log_by_truck[i].uuid = uid;
                                                qr_shp_log_by_truck[i + 1].status = 1;
                                                qr_shp_log_by_truck[i + 1].point = 3;
                                                qr_shp_log_by_truck[i + 1].uuid = uid;
                                            }
                                            else if (qr_shp_log_by_truck[i].TAG_TYPE == "21" && qr_shp_log_by_truck[i + 1].TAG_TYPE == "21")
                                            {
                                                qr_shp_log_by_truck[i].status = 2;
                                                qr_shp_log_by_truck[i].point = 0;
                                                qr_shp_log_by_truck[i].uuid = "Err : 21->21";

                                            }

                                        }
                                        catch
                                        {
                                            qr_shp_log_by_truck[i].status = 2;
                                            qr_shp_log_by_truck[i].point = 0;
                                            qr_shp_log_by_truck[i].uuid = "Err : 21->End";

                                        }


                                        break;

                                    case "3":  // 3 point check without complete data
                                    case "I":
                                    case "J":
                                    case "7":
                                    case "D":
                                        try
                                        {

                                            if (qr_shp_log_by_truck[i].TAG_TYPE == "21" && (qr_shp_log_by_truck[i + 1].TAG_TYPE == "UR" || qr_shp_log_by_truck[i + 1].TAG_TYPE == "CS"))
                                            {
                                                var uid = uuid++.ToString();
                                                qr_shp_log_by_truck[i].status = 1;
                                                qr_shp_log_by_truck[i].point = 3;
                                                qr_shp_log_by_truck[i].uuid = uid;
                                                qr_shp_log_by_truck[i + 1].status = 1;
                                                qr_shp_log_by_truck[i + 1].point = 3;
                                                qr_shp_log_by_truck[i + 1].uuid = uid;
                                            }
                                            else if (qr_shp_log_by_truck[i].TAG_TYPE == "21" && qr_shp_log_by_truck[i + 1].TAG_TYPE == "21")
                                            {
                                                qr_shp_log_by_truck[i].status = 2;
                                                qr_shp_log_by_truck[i].point = 0;
                                                qr_shp_log_by_truck[i].uuid = "Err : 21->21";

                                            }
                                        }
                                        catch
                                        {
                                            qr_shp_log_by_truck[i].status = 2;
                                            qr_shp_log_by_truck[i].point = 0;
                                            qr_shp_log_by_truck[i].uuid = "Err : 21->End";
                                        }
                                        break;
                                    default:
                                        MessageBox.Show("Check now");
                                        break;

                                }



                            }

                            //context.Update(qr_shp_log_by_truck);
                            context.SaveChanges();



                            //MessageBox.Show($"Truck no ; {item.truck}");



                            //var json_string = JsonConvert.SerializeObject(qr_shp_log_by_truck);
                            
                        }

                        //MessageBox.Show("Check now 2025-09-01");
                        

                    }

                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString() + truck_error);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using(var context = new rawscaEntities())
            {
                var dataContext = context.qr_shipping_log_mod.ToList();

                string filePath = "C:\\Users\\Administrator\\source\\repos\\qr_shp_Log\\qr_shp_logs.txt";

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"ID,SEQ,W_H,DEV_ID,LOGIN_ID,TR_DATE_YMD,TR_TYPE,OPE_SEQ,RESULT,RSN_CODE,TRUCK_NO,CUST_NO,SHIP_NO,CHECK_TYPE,SHIP_P_N,SHIP_QTY,TAG_TYPE,P_N,CUST_P_N,QTY,TAG_SEQ,SCAN_DATE_YMD,status,point,uuid"); // Uses the overridden ToString()

                    //writer.WriteLine($"ID \t SEQ \t W_H \t DEV_ID \t LOGIN_ID \t TR_DATE_YMD \t TR_TYPE \t OPE_SEQ \t RESULT \t RSN_CODE \t TRUCK_NO \t CUST_NO \t SHIP_NO \t CHECK_TYPE \t SHIP_P_N \t SHIP_QTY \t TAG_TYPE \t P_N \t CUST_P_N \t QTY \t TAG_SEQ \t SCAN_DATE_YMD \t status \t point \t uuid"); // Uses the overridden ToString()
                    foreach (var obj in dataContext)
                    {
                       writer.WriteLine($"{obj.ID},{obj.SEQ},{obj.W_H},{obj.DEV_ID},{obj.LOGIN_ID},{obj.TR_DATE_YMD.ToString("yyyy-MM-dd HH:mm:ss")},{obj.TR_TYPE},{obj.OPE_SEQ},{obj.RESULT},{obj.RSN_CODE},{obj.TRUCK_NO},{obj.CUST_NO},{obj.SHIP_NO},{obj.CHECK_TYPE},{obj.SHIP_P_N},{obj.SHIP_QTY},{obj.TAG_TYPE},{obj.P_N},{obj.CUST_P_N},{obj.QTY},{obj.TAG_SEQ},{obj.SCAN_DATE_YMD.ToString("yyyy-MM-dd HH:mm:ss")},{obj.status},{obj.point},{obj.uuid}"); // Uses the overridden ToString()
                        //writer.WriteLine($"{obj.ID}\t{obj.SEQ}\t{obj.W_H}\t{obj.DEV_ID}\t{obj.LOGIN_ID}\t{obj.TR_DATE_YMD.ToString("yyyy-MM-dd HH:mm:ss")}\t{obj.TR_TYPE}\t{obj.OPE_SEQ}\t{obj.RESULT}\t{obj.RSN_CODE}\t{obj.TRUCK_NO}\t{obj.CUST_NO}\t{obj.SHIP_NO}\t{obj.CHECK_TYPE}\t{obj.SHIP_P_N}\t{obj.SHIP_QTY}\t{obj.TAG_TYPE}\t{obj.P_N}\t{obj.CUST_P_N}\t{obj.QTY}\t{obj.TAG_SEQ}\t{obj.SCAN_DATE_YMD.ToString("yyyy-MM-dd HH:mm:ss")}\t{obj.status}\t{obj.point}\t{obj.uuid}"); // Uses the overridden ToString()
                    }
                }

                MessageBox.Show("Finished");
                //FileStream fs = new FileStream("C:\\Users\\Administrator\\source\\repos\\qr_shp_Log>\\qr_shp_logs.txt", FileMode.Append, FileAccess.Write, FileShare.Write);
                //fs.Close();
                //StreamWriter sw = new StreamWriter("C:\\Users\\Administrator\\source\\repos\\qr_shp_Log>\\qr_shp_logs.txt", true, Encoding.ASCII);
                
                //sw.Write("");
                //sw.Close();

            }



        }



    }
}
