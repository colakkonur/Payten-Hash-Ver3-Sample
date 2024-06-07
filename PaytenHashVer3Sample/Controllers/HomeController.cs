using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PaytenHashVer3Sample.Models;
using PaytenHashVer3Sample.Models.Payments.Payten;

namespace PaytenHashVer3Sample.Controllers;

public class HomeController : Controller
{

    public HomeController()
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult GenerateCheckoutForm()
    {
        var paymentModel = new Payten.InitializeModel()
        {
            ReturnOId = Guid.NewGuid().ToString(),
            Amount = "100",
            Currency = "TRY",
            Lang = "tr",
            Scheme = this.Request.Scheme,
            Host = this.Request.Host.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CustomerNameSurname = "Test Customer",
            CustomerPhoneNumber = "+905555555555",
            CustomerEmail = "testcustomer@example.com"
        };
        var checkoutFormParameters = new Payten().GenerateCheckoutFormParameters(paymentModel);
        return View(checkoutFormParameters);
    }

    public IActionResult Success()
    {
        // all params
        List<KeyValuePair<string, string>> responseParams = new List<KeyValuePair<string, string>>();
        foreach (var key in Request.Form.Keys)
        {
            responseParams.Add(new KeyValuePair<string, string>(key, Request.Form[key]));
        }

        var orderId = responseParams.FirstOrDefault(x => x.Key == "returnOid").Value;
        var procReturnCode = responseParams.FirstOrDefault(x => x.Key == "ProcReturnCode").Value;
        var response = responseParams.FirstOrDefault(x => x.Key == "Response").Value;

        string formattedResponseParams = string.Join(" | ", responseParams.Select(x => x.Key + ": " + x.Value));
        TempData["responseParams"] = formattedResponseParams;

        bool checkHash = new Payten().CheckHash(responseParams);
        if (checkHash)
        {
            if (procReturnCode == "00" && response == "Approved")
            {
                return View();
            }
            else
            {
                return RedirectToAction("Fail");
            }
        }
        else
        {
            return RedirectToAction("Fail");
        }
    }

    public IActionResult Fail()
    {
        // all params
        List<KeyValuePair<string, string>> responseParams = new List<KeyValuePair<string, string>>();
        foreach (var key in Request.Form.Keys)
        {
            responseParams.Add(new KeyValuePair<string, string>(key, Request.Form[key]));
        }

        var orderId = responseParams.FirstOrDefault(x => x.Key == "returnOid").Value;
        var mdStatus = responseParams.FirstOrDefault(x => x.Key == "mdStatus").Value;

        string formattedResponseParams = string.Join(" | ", responseParams.Select(x => x.Key + ": " + x.Value));
        TempData["responseParams"] = formattedResponseParams;

        bool checkHash = new Payten().CheckHash(responseParams);
        if (checkHash)
        {
            // mdStatus Değerleri
            // • 1 = Doğrulanmış İşlem (Full 3D)
            // • 2, 3, 4 = Kart kayıtlı değil (Half 3D)
            // • 5, 6, 7, 8 = Geçerli doğrulama yok veya sistem hatası
            // • 0 = Doğrulama Başarısız

            return View();
        }
        else
        {
            // Hash değeri uyuşmazlığı
            return View();
        }
    }

    public async Task Callback()
    {
        // all params
        List<KeyValuePair<string, string>> responseParams = new List<KeyValuePair<string, string>>();
        foreach (var key in Request.Form.Keys)
        {
            responseParams.Add(new KeyValuePair<string, string>(key, Request.Form[key]));
        }

        var orderId = responseParams.FirstOrDefault(x => x.Key == "returnOid").Value;
        var procReturnCode = responseParams.FirstOrDefault(x => x.Key == "ProcReturnCode").Value;
        var response = responseParams.FirstOrDefault(x => x.Key == "Response").Value;
        var authCode = responseParams.FirstOrDefault(x => x.Key == "AuthCode").Value;
        var hostRefNum = responseParams.FirstOrDefault(x => x.Key == "HostRefNum").Value;
        var transId = responseParams.FirstOrDefault(x => x.Key == "TransId").Value;

        // işlemin hata vermesine karşı
        var mdStatus = responseParams.FirstOrDefault(x => x.Key == "mdStatus").Value;
        var mdErrorMsg = responseParams.FirstOrDefault(x => x.Key == "mdErrorMsg").Value;

        bool checkHash = new Payten().CheckHash(responseParams);
        if (checkHash)
        {
            if (procReturnCode == "00" && response == "Approved")
            {
                // save to database
            }
            else
            {
                // save to database
            }
        }
        else
        {
            // Hash değeri uyuşmazlığı
        }

        await Response.WriteAsync("Approved");
    }
}