using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/Product")]
    public class ProductController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();
        [HttpPost,Route("addNewProduct")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage addNewProduct([FromBody]Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim= TokenManager.ValidateToken(token);
                if(tokenClaim == null || tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                response.Message = "Product added successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,ex);
            }
        }
        [HttpGet, Route("getAllProducts")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage getAllProducts()
        {
            try
            {
                var result = from Products in db.Products
                             join Category in db.Categories
                             on Products.categoryId equals Category.id
                             select new
                             {
                                 Products.id,
                                 Products.name,
                                 Products.description,
                                 Products.price,
                                 Products.status,
                                 categoryId = Category.id,
                                 categoryName = Category.name

                             };
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }   
        }

        [HttpGet, Route("getProductByCategory/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage getProductByCategory(int id)
        {
            try
            {
                var result = db.Products.Where(p=>p.categoryId==id && p.status == "true")
                    .Select(i => new {i.name, i.id }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        
        [HttpGet, Route("getProductBy/{id}")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage getProductById (int id)
        {
            try
            {
                Product product = db.Products.Find(id);
                return Request.CreateResponse(HttpStatusCode.OK, product);

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, Route("updateProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage updateProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim == null || tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product existingProduct = db.Products.Find(product.id);
                if(existingProduct == null)
                {
                    response.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.NotFound, response);
                }
                existingProduct.name = product.name;
                existingProduct.description = product.description;
                existingProduct.price = product.price;
                existingProduct.categoryId = product.categoryId;
                db.Entry(existingProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Product updated successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("deleteProduct/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage delete(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim == null || tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product productObj = db.Products.Find(id);
                if (productObj == null)
                {
                    response.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.NotFound, response);
                }
                db.Products.Remove(productObj);
                db.SaveChanges();
                response.Message = "Product deleted successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, Route("updateProductStatus")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage updateProductStatus([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim == null || tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                Product existingProduct = db.Products.Find(product.id);
                if (existingProduct == null)
                {
                    response.Message = "Product not found";
                    return Request.CreateResponse(HttpStatusCode.NotFound, response);
                }
                existingProduct.status = product.status;
                db.Entry(existingProduct).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Product status updated successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


    }
}
