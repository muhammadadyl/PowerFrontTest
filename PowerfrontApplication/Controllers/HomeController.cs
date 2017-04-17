using ClosedXML.Excel;
using PowerfrontApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PowerfrontApplication.Controllers
{
    public class HomeController : Controller
    {
        private SqlConnection _conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult OperatorReport()
        {
            OperatorReportItems ProductivityReport = new OperatorReportItems();

            ViewBag.Message = "Operator Productivity Report";
            SqlCommand sqlcomm;
            SqlDataReader dr;
            List<string> websites = new List<string>();
            List<string> devices = new List<string>();
            try
            {
                sqlcomm = new SqlCommand("dbo.GetWebsites", _conn);
                sqlcomm.CommandType = CommandType.StoredProcedure;
                _conn.Open();
                dr = sqlcomm.ExecuteReader();
                while (dr.Read())
                {
                    websites.Add(dr[0].ToString());
                }
            }
            finally
            {
                _conn.Close();
            }


            try
            {
                sqlcomm = new SqlCommand("GetDevices", _conn);
                sqlcomm.CommandType = CommandType.StoredProcedure;
                _conn.Open();
                dr = sqlcomm.ExecuteReader();
                while (dr.Read())
                {
                    devices.Add(dr[0].ToString());
                }
                _conn.Close();
            }
            finally
            {
                _conn.Close();
            }

            ViewBag.seletedListWebsites = new SelectList(websites);
            ViewBag.seletedListDevices = new SelectList(devices);

            return View(ProductivityReport.OperatorProductivity);
        }

        /// <summary>
        /// This is a Web service that is designed for only perticular View. No need to have a Web Api implimentation here
        /// </summary>
        /// <param name="dataFilters"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetOperationResult(DataFilters dataFilters)
        {
            OperatorReportItems ProductivityReport = new OperatorReportItems();
            dataFilters = GetDateOutOfPreDefine(dataFilters);
            if (dataFilters.ToDate >= dataFilters.FromDate)
            {

                try
                {
                    SqlCommand sqlcomm = new SqlCommand("dbo.OperatorProductivityWithFilters ", _conn);
                    sqlcomm.CommandType = CommandType.StoredProcedure;
                    sqlcomm.Parameters.Add(new SqlParameter("@fromDate", (dataFilters.FromDate == DateTime.MinValue) ? (object)DBNull.Value : dataFilters.FromDate.ToString("yyyy-MM-dd")));
                    sqlcomm.Parameters.Add(new SqlParameter("@toDate", (dataFilters.ToDate == DateTime.MinValue) ? (object)DBNull.Value : dataFilters.ToDate.ToString("yyyy-MM-dd")));
                    sqlcomm.Parameters.Add(new SqlParameter("@website", dataFilters.Website ?? (object)DBNull.Value));
                    sqlcomm.Parameters.Add(new SqlParameter("@device", dataFilters.Device ?? (object)DBNull.Value));
                    _conn.Open();
                    SqlDataReader dr = sqlcomm.ExecuteReader();
                    while (dr.Read())
                    {
                        OperatorReportViewModel opVM = new OperatorReportViewModel();
                        opVM.ID = Convert.ToInt32(dr[0]);
                        opVM.Name = Convert.ToString(dr[1]);
                        opVM.ProactiveSent = Convert.ToInt32(dr[2]);
                        opVM.ProactiveAnswered = Convert.ToInt32(dr[3]);
                        opVM.ProactiveResponseRate = Convert.ToInt32(dr[4]);
                        opVM.ReactiveAnswered = Convert.ToInt32(dr[5]);
                        opVM.ReactiveReceived = Convert.ToInt32(dr[6]);
                        opVM.ReactiveResponseRate = Convert.ToInt32(dr[7]);
                        opVM.TotalChatLength = TimeSpan.FromMinutes(Convert.ToInt32(dr[8])).ToString(@"d\d' 'hh\h' 'mm\m");
                        opVM.AverageChatLength = TimeSpan.FromMinutes(Convert.ToInt32(dr[9])).ToString(@"mm\m");
                        ProductivityReport.OperatorProductivity.Add(opVM);
                    }
                }
                catch (SqlException ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Database Error, Connot complete the process");
                }
                finally
                {
                    _conn.Close();
                }
                return Json(ProductivityReport.OperatorProductivity);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Supplied Filter is incorrect");
        }

        [HttpPost]
        public ActionResult GetDataOnExcel(DataFilters dataFilters)
        {
            dataFilters = GetDateOutOfPreDefine(dataFilters);
            if (dataFilters.ToDate >= dataFilters.FromDate)
            {
                using (SqlCommand sqlcomm = new SqlCommand("dbo.OperatorProductivityWithFilters ", _conn))
                {
                    sqlcomm.CommandType = CommandType.StoredProcedure;
                    sqlcomm.Parameters.Add(new SqlParameter("@fromDate", (dataFilters.FromDate == DateTime.MinValue) ? (object)DBNull.Value : dataFilters.FromDate.ToString("yyyy-MM-dd")));
                    sqlcomm.Parameters.Add(new SqlParameter("@toDate", (dataFilters.ToDate == DateTime.MinValue) ? (object)DBNull.Value : dataFilters.ToDate.ToString("yyyy-MM-dd")));
                    sqlcomm.Parameters.Add(new SqlParameter("@website", dataFilters.Website ?? (object)DBNull.Value));
                    sqlcomm.Parameters.Add(new SqlParameter("@device", dataFilters.Device ?? (object)DBNull.Value));

                    using (SqlDataAdapter sda = new SqlDataAdapter(sqlcomm))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            using (XLWorkbook wb = new XLWorkbook())
                            {
                                wb.Worksheets.Add(dt, "OperatorsReport");
                                string path = Server.MapPath("~/Files") + "/Report.xlsx";
                                if (System.IO.File.Exists(path))
                                    System.IO.File.Delete(path);
                                wb.SaveAs(path);
                                return Json(new{ path = "/files/Report.xlsx" });
                            }
                        }
                    }
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Supplied Filter is incorrect");
        }

        private DataFilters GetDateOutOfPreDefine(DataFilters dataFilters) {
            if (!string.IsNullOrEmpty(dataFilters.PreDefinedDate))
            {
                switch (dataFilters.PreDefinedDate)
                {
                    case "Today":
                        dataFilters.FromDate = DateTime.Now.AddDays(-1);
                        dataFilters.ToDate = DateTime.Now;
                        break;
                    case "Yesterday":
                        dataFilters.FromDate = DateTime.Now.AddDays(-2);
                        dataFilters.ToDate = DateTime.Now.AddDays(-1);
                        break;
                    case "CurrentWeek":
                        dataFilters.FromDate = DateTime.Now.AddDays(-7);
                        dataFilters.ToDate = DateTime.Now;
                        break;
                    case "LastWeek":
                        dataFilters.FromDate = DateTime.Now.AddDays(-14);
                        dataFilters.ToDate = DateTime.Now.AddDays(-7);
                        break;
                    case "CurrentMonth":
                        dataFilters.FromDate = DateTime.Now.AddMonths(-1);
                        dataFilters.ToDate = DateTime.Now;
                        break;
                    case "LastMonth":
                        dataFilters.FromDate = DateTime.Now.AddMonths(-2);
                        dataFilters.ToDate = DateTime.Now.AddMonths(-1);
                        break;
                    case "CurrentYear":
                        dataFilters.FromDate = DateTime.Now.AddYears(-1);
                        dataFilters.ToDate = DateTime.Now;
                        break;
                    case "LastYear":
                        dataFilters.FromDate = DateTime.Now.AddYears(-2);
                        dataFilters.ToDate = DateTime.Now.AddYears(-1);
                        break;
                    default:
                        dataFilters.FromDate = DateTime.Now.AddYears(-15);
                        dataFilters.ToDate = DateTime.Now;
                        break;
                }
            }
            return dataFilters;
        }
    }
}