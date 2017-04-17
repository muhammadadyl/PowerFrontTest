using PowerfrontApplication.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
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
                sqlcomm = new SqlCommand("SELECT DISTINCT Website FROM Conversation", _conn);
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
                sqlcomm = new SqlCommand("SELECT DISTINCT Device FROM Visitor", _conn);
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

        [HttpPost]
        public ActionResult GetOperationResult(DataFilters dataFilters)
        {
            OperatorReportItems ProductivityReport = new OperatorReportItems();

            try
            {
                SqlCommand sqlcomm = new SqlCommand("exec dbo.OperatorProductivity ", _conn);
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
            finally
            {
                _conn.Close();
            }
            return Json(ProductivityReport.OperatorProductivity);
        }

    }
}