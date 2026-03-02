using Cafe_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using System.Web;
using System.Net.Http.Headers;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Cafe_Management_System.Controllers
{
    [RoutePrefix("api/Bill")]
    public class BillController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();
        private string pdfPath = "D:\\";

        [HttpPost, Route("generateReport")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GenerateReport([FromBody] Bill bill)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                var ticks = DateTime.Now.Ticks;
                var guid = Guid.NewGuid().ToString();
                var unique = ticks.ToString() + "_" + guid;
                bill.createdBy = tokenClaim.Email;
                bill.uuid = "BILL" + unique;
                db.Bills.Add(bill);
                db.SaveChanges();
                Get(bill);
                return Request.CreateResponse(HttpStatusCode.OK, new { uuid = bill.uuid });

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void Get(Bill bill)
        {
            try
            {
                dynamic productDetails = JsonConvert.DeserializeObject(bill.productDetails.ToString());
                var todayDate = "Date: " + Convert.ToDateTime(DateTime.Today).ToString("dd-MM-yyyy");
                PdfWriter writer = new PdfWriter(pdfPath + bill.uuid + ".pdf");
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                Paragraph header = new Paragraph("Cafe Management System").SetTextAlignment(TextAlignment.CENTER).SetFontSize(25);
                document.Add(header);

                Paragraph newline = new Paragraph(new Text("\n"));

                LineSeparator lineSeparator = new LineSeparator(new SolidLine());
                document.Add(lineSeparator);

                //Customer Details
                Paragraph customerDetails = new Paragraph("Name: " + bill.name + "\nEmail: " + bill.email + "\nContact Number: " + bill.contactNumber + "\nPayment Method:" + bill.paymentMethod).SetFontSize(12);
                document.Add(customerDetails);

                //Table
                Table table = new Table(5, false);
                table.SetWidth(new UnitValue(UnitValue.PERCENT, 100));

                //Header
                Cell headerName = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Name"));

                Cell headerCategory = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Category"));

                Cell headerQuantity = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Quantity"));

                Cell headerPrice = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Price"));

                Cell headerSubTotal = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("Sub Total"));

                table.AddCell(headerName);
                table.AddCell(headerCategory);
                table.AddCell(headerQuantity);
                table.AddCell(headerPrice);
                table.AddCell(headerSubTotal);

                foreach (JObject product in productDetails)
                {
                    Cell nameCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph((string)product["name"]));

                    Cell categoryCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph((string)product["category"]));

                    Cell quantityCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph((string)product["quantity"]));

                    Cell priceCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph((string)product["pricee"]));

                    Cell totalCell = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph((string)product["total"].ToString()));

                    table.AddCell(nameCell);
                    table.AddCell(categoryCell);
                    table.AddCell(quantityCell);
                    table.AddCell(priceCell);
                    table.AddCell(totalCell);
                }
                document.Add(table);
                Paragraph last = new Paragraph("Total Amount: " + bill.totalAmount + "\n" + todayDate + "\nThank you for visiting. Please visit again !!!").SetTextAlignment(TextAlignment.RIGHT);
                document.Add(last);
                document.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        [HttpPost, Route("getPdf")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetPdf([FromBody] Bill bill)
        {
            try
            {
                if (bill.name != null)
                {
                    Get(bill);
                }
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                string filePath = pdfPath + bill.uuid + ".pdf";
                byte[] bytes = File.ReadAllBytes(filePath);

                response.Content = new ByteArrayContent(bytes);
                response.Content.Headers.ContentLength = bytes.LongLength;

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = bill.uuid.ToString() + ".pdf";

                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(bill.uuid.ToString() + ".pdf"));

                return response;

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("getBills")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetBills()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    var userResult = db.Bills.Where(x => (x.createdBy == tokenClaim.Email))
                        .AsEnumerable().Reverse();
                    return Request.CreateResponse(HttpStatusCode.OK, userResult);
                }
                var adminResul = db.Bills.AsEnumerable().Reverse();

                return Request.CreateResponse(HttpStatusCode.OK, adminResul);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost, Route("deleteBill/{id}")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage DeleteBill(int id)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = "Unauthorized" });
                }
                var bill = db.Bills.Find(id);
                if (bill == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { message = "Bill not found" });
                }
                db.Bills.Remove(bill);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, new { message = "Bill deleted successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

    }
}
