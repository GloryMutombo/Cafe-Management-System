using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("addNewCategory")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage AddNewCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin" || tokenClaim == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Unauthorized Access" });
                }
                db.Categories.Add(category);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, new { message = "Category added successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet, Route("getAll")]
        public HttpResponseMessage GetAllCategories()
        {
            try
            {

                List<Category> categories = db.Categories.ToList();
                return Request.CreateResponse(HttpStatusCode.OK, categories);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut, Route("update")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage updateCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(!tokenClaim.Role.Equals("admin") || tokenClaim == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Unauthorized Access" });
                }
                Category categoryObj= db.Categories.Find(category.id);
                if (categoryObj == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Category id not found" });
                }
                categoryObj.name = category.name;
                db.Entry(categoryObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Category updated successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
