using ISeeMonitor.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Xml.Linq;

namespace ISeeMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index(searchalljob searchalljob)
        {
            if (HttpContext.Session.GetString("token") is null)
                return RedirectToAction("Logout");
            RestClient client = new RestClient(_configuration["API:ISEECENTER"]);
            RestRequest request = new RestRequest($"api/Monitors/GET_DETAIL_ALLJOB", Method.Post);
            request.AddHeader("Authorization", "Bearer " + HttpContext.Session.GetString("token"));
            var isNulldata = searchalljob.fname is null 
                            && searchalljob.owner_id is null
                            && searchalljob.job_id is null
                            && searchalljob.job_status_code is null
                            ;
            if(isNulldata)
            {
                searchalljob = new searchalljob
                {
                    limit = "50",
                    fname = string.Empty,
                    owner_id = string.Empty,
                    job_id = string.Empty,
                    job_status_code = string.Empty
                };
            }
            else
            {
                searchalljob.fname =searchalljob.fname?? string.Empty;
                searchalljob.job_id =searchalljob.job_id ?? string.Empty;
                searchalljob.owner_id =searchalljob.owner_id ?? string.Empty;
                searchalljob.job_status_code =searchalljob.job_status_code ?? string.Empty;
                searchalljob.limit =searchalljob.limit ?? "100";
            }        
            request.AddBody(searchalljob);
            var response = client.Execute<List<crmmonitor>>(request);
            ViewBag.Fullname =  HttpContext.Session.GetString("fullname");
            ViewData["ownerid"] = GET_OWNERID();
            ViewData["substatus"]= GET_TBM_SUBSTATUS();
            ViewBag.Substatus = HttpContext.Session.GetString("substatus")==null?null: JsonSerializer.Deserialize<List<tbm_substatus>>(HttpContext.Session.GetString("substatus"));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return View(response?.Data);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return View();
            }
            else
            {
                ViewBag.Error = response.Content;
                return View("Error");
            }
            
        }     

        public IActionResult Link(string token)
        {
            GET_USERINFO(token);
           // GET_OWNERID();
            GET_TBM_SUBSTATUS();
            HttpContext.Session.SetString("token", token);
            return RedirectToAction("Index","Home",null);
        }

        protected void GET_USERINFO(string token)
        {
            RestClient client = new RestClient(_configuration["API:ISEESERVICE"]);
            RestRequest request = new RestRequest($"api/v1/ISEEServices/Userinfo", Method.Post);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = client.Execute<UserInfo>(request);
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                HttpContext.Session.SetString("userinfo", JsonSerializer.Serialize(response.Data));
                HttpContext.Session.SetString("fullname",response.Data.fullname);
            }
        }

        protected List<owner_id> GET_OWNERID()
        {
            RestClient client = new RestClient(_configuration["API:ISEECENTER"]);
            RestRequest request = new RestRequest($"api/Monitors/GET_ALL_OWNER", Method.Get);
            request.AddHeader("Authorization", "Bearer " + HttpContext.Session.GetString("token"));
            var response = client.Execute<List<owner_id>>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
               return response.Data;
            }
            return null;
        }
        protected List<tbm_substatus> GET_TBM_SUBSTATUS()
        {
                var client = new RestClient(_configuration["API:ISEESERVICE"]);
                var request = new RestRequest("api/v1/ISEEServices/TBM_SUBSTATUS", Method.Get);
                request.AddHeader("Authorization", "Bearer " + HttpContext.Session.GetString("token"));
                var response = client.Execute<List<tbm_substatus>>(request);
                if (response.StatusCode== HttpStatusCode.OK)
                {
                    HttpContext.Session.SetString("substatus", JsonSerializer.Serialize(response.Data));               
                }
            return response?.Data;
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userinfo");
            HttpContext.Session.Remove("fullname");
            HttpContext.Session.Remove("token");
            return Redirect(_configuration["Link:Login"]);
        }
        [HttpPost]
        public IActionResult UpdateStatus(update_status data)
        {
            if (HttpContext.Session.GetString("token") is null)
                return RedirectToAction("Logout");
            RestClient client = new RestClient(_configuration["API:ISEESERVICE"]);
            RestRequest request = new RestRequest($"api/v2/ISEEStatus/UpdateStatus", Method.Post);
            request.AddHeader("Authorization", "Bearer " + HttpContext.Session.GetString("token"));
            request.AddJsonBody(data);
            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Json(new { success = true }) ;
            }
            return Json(new { success = false,error=response.Content }) ;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}