using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/Dashboard")]
    public class DashboardController : ApiController
    {
        CafeEntities db = new CafeEntities();

        [HttpGet, Route("details")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetDetails()
        {
            try
            {
                var data = new
                {
                    totalUsers = db.Users.Count(),
                    totalProducts = db.Products.Count(),
                    totalBills = db.Bills.Count(),
                    totalCategories = db.Categories.Count()
                };
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

    }
}
