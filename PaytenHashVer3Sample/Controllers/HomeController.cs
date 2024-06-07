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

    #region Generate Checkout Form and Handle Payment Result

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

        var serializedResponseParams = System.Text.Json.JsonSerializer.Serialize(responseParams); // modelde gönderilemez
        TempData["ResponseValues"] = serializedResponseParams;

        bool checkHash = new Payten().CheckHash(responseParams);
        if (checkHash)
        {
            if (procReturnCode == "00" && response == "Approved")
            {
                // bu aşamada iş akışınıza göre aksiyon alabilirsiniz.
                return RedirectToAction("OrderCompleted", "Home");
            }
            else
            {
                return RedirectToAction("OrderFailed", "Home", new { fromSuccess = true});
            }
        }
        else
        {
            return RedirectToAction("OrderFailed", "Home", new { fromSuccess = true });
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

        var serializedResponseParams = System.Text.Json.JsonSerializer.Serialize(responseParams); // modelde gönderilemez
        TempData["ResponseValues"] = serializedResponseParams;
        
        bool checkHash = new Payten().CheckHash(responseParams);
        if (checkHash)
        {
            // mdStatus Değerleri
            // • 1 = Doğrulanmış İşlem (Full 3D)
            // • 2, 3, 4 = Kart kayıtlı değil (Half 3D)
            // • 5, 6, 7, 8 = Geçerli doğrulama yok veya sistem hatası
            // • 0 = Doğrulama Başarısız
            return RedirectToAction("OrderFailed", "Home");
        }
        else
        {
            // Hash değeri uyuşmazlığı
            return RedirectToAction("OrderFailed", "Home");
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
                // bu aşamada iş akışınıza göre aksiyon alabilirsiniz.
                // save to database
            }
            else
            {
                // işlem başarısız
            }
        }
        else
        {
            // Hash değeri uyuşmazlığı
        }

        await Response.WriteAsync("Approved");
    }

    #endregion
    

    #region Show Order Result Pages

    public IActionResult OrderCompleted()
    {
        var responseParams = System.Text.Json.JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(TempData["ResponseValues"].ToString());
        return View(responseParams);
    }
    public IActionResult OrderFailed(bool fromSuccess = false)
    {
        var responseParams = System.Text.Json.JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(TempData["ResponseValues"].ToString());
        var orderFailedValues = responseParams;
        if (fromSuccess)
        {
            // Success sayfasından gelen hatalı işlem sonucu
            // 1. procReturnCode == "00" && response == "Approved" koşulu sağlanmamışsa
            // 2. Hash değeri uyuşmamışsa
        }
        var model = new PaymentErrorModel()
        {
            ErrorCode = orderFailedValues.FirstOrDefault(w=>w.Key == "mdStatus").Value,
            ErrorMessage = $"{orderFailedValues.FirstOrDefault(w=>w.Key == "ErrMsg").Value} - Response: {orderFailedValues.FirstOrDefault(w=>w.Key == "Response").Value}",
            AllParameters = string.Join(" | ", orderFailedValues.Select(x => x.Key + ": " + x.Value))
        };
        return View(model);
    }

    #endregion
   
}