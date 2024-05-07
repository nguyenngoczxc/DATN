using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TTTN3.Models;
using TTTN3.Models.ViewModels;
using TTTN3.Responsitory;

namespace TTTN3.Controllers
{
    //[Authorize]
    public class CheckOutController : Controller
    {
        private readonly DataContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signinManager;

        public CheckOutController(DataContext _db, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signinManager, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            db = _db;
            _userManager = userManager;
            _signinManager = signinManager;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CheckOut()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(InvoiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                string userId = _userManager.GetUserId(User);
                if (cartItems != null)
                {
                    decimal total_Payment = 0;
                    Random rd = new Random();

                    invoice order = new invoice();
                    order.invoice_Code = "I" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    order.CustomerName = model.CustomerName;
                    order.Phone = model.Phone;
                    order.Address = model.Address;
                    order.Email = model.Email;
                    order.status = 1;//chưa hoàn thành / 2/đã hoàn thành
                    db.invoices.Add(order);
                    db.SaveChanges();
                    foreach (var x in cartItems)
                    {
                        var invoice_Detail = new invoice_Detail();
                        invoice_Detail.invoice_Code = order.invoice_Code;
                        invoice_Detail.invoice_Detail_Code = "ID" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                        invoice_Detail.product_Code = x.Product_Code;
                        invoice_Detail.quantity_Sold = x.Sizes.First().Quantity;
                        invoice_Detail.price = x.Sizes.First().Price;
                        invoice_Detail.color_Code = x.Colors.First().Color_Code;
                        invoice_Detail.size_Code = x.Sizes.First().Size_Code;

                        db.invoice_Details.Add(invoice_Detail);
                        db.SaveChanges();

                        var product = db.products.FirstOrDefault(p => p.product_Code == x.Product_Code);

                        if (product != null)
                        {
                            product.quantity -= x.Sizes.First().Quantity;
                        }
                        total_Payment += x.Sizes.Sum(size => size.Total);
                    }

                    order.type_Payment = model.TypePayment;
                    order.total_Payment = total_Payment;
                    order.invoice_Date = DateTime.Now;
                    order.CreateBy = model.Phone;
                    order.total_Quantity = cartItems.Count();
                    order.Note = model.Note;

                    if (User.Identity.IsAuthenticated)
                    {
                        string userName = User.Identity.Name;
                        var user = await _userManager.FindByNameAsync(userName);
                        order.customerId = user.Id;
                    }

                    //    db.SaveChanges();
                    //    HttpContext.Session.Remove("Cart");
                    //}
                    //return RedirectToAction("List_Invoice", "Invoice", new { Id = userId });
                    db.SaveChanges();
                    

                    // Kiểm tra loại thanh toán
                    if (model.TypePayment == 1)
                    {
                        // Thanh toán thông thường
                        HttpContext.Session.Remove("Cart");
                        return RedirectToAction("List_Invoice", "Invoice", new { Id = userId });
                    }
                    else if (model.TypePayment == 2)
                    {
                        // Thanh toán bằng PayPal
                        // Truyền thông tin cần thiết để thực hiện thanh toán bằng PayPal
                        return RedirectToAction("PaymentWithPaypal");
                    }
                }
            }
            return View("FailedCheckOut");
        }
        public ActionResult FailedCheckOut()
        {
            return View();
        }
        public ActionResult PaymentWithPaypal(string Cancel = null, string blogId = "", string PayerID = "", string guid = "")
        {
            string userId = _userManager.GetUserId(User);
            //getting the apiContext
            var ClientID = _configuration.GetValue<string>("PayPal:Key");
            var ClientSecret = _configuration.GetValue<string>("PayPal:Secret");
            var mode = _configuration.GetValue<string>("PayPal:mode");
            APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);
            try
            {
                string payerId = PayerID;
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = this.Request.Scheme + "://" + this.Request.Host + "/CheckOut/PaymentWithPayPal?";
                    var guidd = Convert.ToString((new Random()).Next(100000));
                    guid = guidd;
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, blogId);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid
                    _httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment

                    var paymentId = _httpContextAccessor.HttpContext.Session.GetString("payment");
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId as string);
                    //If executed payment failed then we will show payment failure message to user
                    if (executedPayment.state.ToLower() != "approved")
                    {

                        return View("FailedCheckOut");
                    }
                    var blogIds = executedPayment.transactions[0].item_list.items[0].sku;
                    return RedirectToAction("List_Invoice", "Invoice", new { Id = userId });
                }
            }
            catch (Exception ex)
            {
                return View("FailedCheckOut");
            }
            return RedirectToAction("List_Invoice", "Invoice", new { Id = userId });
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId

            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId)
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            //create itemlist and add item objects to it
            decimal total_Payment=0;
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc
            foreach(var item in cartItems)
            {
                itemList.items.Add(new Item()
                {
                    name = item.Product_Name,
                    currency = "USD",
                    price = Math.Round(item.Sizes.First().Price/23500,2).ToString(),
                    quantity = item.Sizes.First().Quantity.ToString(),
                    sku = item.Product_Code
                });
                total_Payment += Math.Round(item.Sizes.Sum(size => size.Total)/23500,2);
            }
            
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details
            //var details = new Details()
            //{
            // tax = "1",
            // shipping = "1",
            // subtotal = "1"
            //};
            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = total_Payment.ToString(),
                // Total must be equal to sum of tax, shipping and subtotal.
                                                                 //details = details
        };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(), //Generate an Invoice No
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext
            HttpContext.Session.Remove("Cart");
            return this.payment.Create(apiContext);
        }

    }
}
