using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;



namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();
        [HttpPost, Route("signup")]

        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Registration Successful" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Email already exists" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email && u.password == user.password).FirstOrDefault();
                if (userObj != null)
                {
                    if (userObj.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { token = TokenManager.GenerateToken(userObj.email, userObj.role) });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Wait for admin approval" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Invalid email or password" });
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage CheckToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "Token is valid/true" });
        }

        [HttpGet, Route("getAllUsers")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                var token = Request.Headers.GetValues("Authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim == null || tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Unauthorized Access" });
                }
                var users = db.Users.Select(u => new
                {
                    u.id,
                    u.name,
                    u.contactNumber,
                    u.email,
                    u.status,
                    u.role
                }).Where(x => (x.role == "user")).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, users);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut, Route("updateUserStatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage UpdateUserStatus(User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User userObj = db.Users.Find(user.id);
                if (userObj == null)
                {
                    response.Message = "User id not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "User status updated successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, Route("updateUserStatus")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage updateUserStatus(User user)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User userObj = db.Users.Find(user.id);
                if (userObj == null)
                {
                    response.Message = "User id not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "User status updated successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, Route("changePassword")]
        [CustomAuthenticationFilter]
        public HttpResponseMessage ChangePassword([FromBody] ChangePassword changePassword)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                User userObj = db.Users.Where(u => u.email == tokenClaim.Email && u.password == changePassword.OldPassword).FirstOrDefault();
                if (userObj == null)
                {
                    response.Message = "Incorrect old password";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.password = changePassword.NewPassword;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.Message = "Password changed successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string createEmailBody (string email, string password)
        {
            try
            {
                string body =string.Empty;
                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("~/Template/forgot-password.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{{email}}", email);
                body = body.Replace("{{password}}", password);
                body= body.Replace("{frontend_url}", "http://localhost:4200");
                return body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        [HttpPost, Route("forgotPassword")]
        public async Task<HttpResponseMessage> ForgotPassword ([FromBody] User user)
        {
            User userObj = db.Users.Where(u => u.email == user.email).FirstOrDefault();
            response.Message = "Password sent to your email successfully";
            if(userObj == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            var message = new MailMessage();
            message.To.Add(new MailAddress(userObj.email));
            message.Subject = userObj.email;
            message.Body = createEmailBody(userObj.email, userObj.password);
            message.IsBodyHtml = true;
            using(var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
                await Task.FromResult(0);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

    }
}
