namespace PaytenHashVer3Sample.Models.Payments.Payten;

public class Payten
{
    private string _clientId = "500300000"; // Mağaza Numarası (Client Id ) test > XXXXXXX
    private string _storeKey = "123456"; // store key test > XXXXXXX

    public List<KeyValuePair<string, string>> GenerateCheckoutFormParameters(InitializeModel request)
    {
        List<KeyValuePair<string, string>> postParams = new List<KeyValuePair<string, string>>();
        postParams.Add(new KeyValuePair<string, string>("ClientId", _clientId));
        postParams.Add(new KeyValuePair<string, string>("oid", $"{request.ReturnOId}")); // sipariş numarası
        postParams.Add(new KeyValuePair<string, string>("amount", $"{request.Amount}")); // tutar
        postParams.Add(new KeyValuePair<string, string>("okUrl", $"{request.Scheme}://{request.Host}/order/ordersuccess"));
        postParams.Add(new KeyValuePair<string, string>("failUrl", $"{request.Scheme}://{request.Host}/order/orderfail"));
        postParams.Add(new KeyValuePair<string, string>("callbackUrl",$"{request.Scheme}://{request.Host}/order/ordercallback"));
        postParams.Add(new KeyValuePair<string, string>("TranType", "Auth"));
        postParams.Add(new KeyValuePair<string, string>("Instalment", ""));
        postParams.Add(new KeyValuePair<string, string>("currency",
            request.Currency == "TRY" ? "949" :
            request.Currency == "USD" ? "840" :
            request.Currency == "EUR" ? "978" : throw new Exception("Geçersiz para birimi")));
        
        postParams.Add(new KeyValuePair<string, string>("rnd", $"{Guid.NewGuid().ToString("N")}"));
        postParams.Add(new KeyValuePair<string, string>("storetype", "3d_pay_hosting"));
        postParams.Add(new KeyValuePair<string, string>("lang", $"{request.Lang}"));
        postParams.Add(new KeyValuePair<string, string>("hashAlgorithm", "ver3"));
        postParams.Add(new KeyValuePair<string, string>("refreshtime", "5"));
        postParams.Add(new KeyValuePair<string, string>("email", $"{request.CustomerEmail}"));
        postParams.Add(new KeyValuePair<string, string>("tel", $"{request.CustomerPhoneNumber}"));
        postParams.Add(new KeyValuePair<string, string>("BillToName",$"{request.CustomerNameSurname} {request.CustomerId}"));
        postParams.Add(new KeyValuePair<string, string>("BillToCompany", $"TEST COMPANY"));

        System.Security.Cryptography.SHA512 sha = new System.Security.Cryptography.SHA512CryptoServiceProvider();

        #region hash

        postParams.Sort(delegate(KeyValuePair<string, string> firstPair, KeyValuePair<string, string> nextPair)
        {
            return String.Compare(firstPair.Key.ToLowerInvariant(), nextPair.Key.ToLowerInvariant(),StringComparison.OrdinalIgnoreCase);
        });

        string hashVal = "";
        _storeKey = _storeKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (KeyValuePair<string, string> pair in postParams)
        {
            string escapedValue = pair.Value.Replace("\\", "\\\\").Replace("|", "\\|");
            string lowerValue = pair.Key.ToLowerInvariant();
            if (!"encoding".Equals(lowerValue, StringComparison.OrdinalIgnoreCase) &&
                !"hash".Equals(lowerValue, StringComparison.OrdinalIgnoreCase) &&
                !"countdown".Equals(lowerValue, StringComparison.OrdinalIgnoreCase))
            {
                hashVal += escapedValue + "|";
            }
        }

        hashVal += _storeKey;

        byte[] hashbytes = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(hashVal);
        byte[] inputbytes = sha.ComputeHash(hashbytes);
        string hash = System.Convert.ToBase64String(inputbytes);
        postParams.Add(new KeyValuePair<string, string>("hash", hash));

        #endregion

        return postParams;
    }

    public bool CheckHash(List<KeyValuePair<string, string>> responseParams)
    {
        responseParams.Sort((firstPair, nextPair) =>
            String.Compare(firstPair.Key.ToLowerInvariant(), nextPair.Key.ToLowerInvariant(),StringComparison.InvariantCulture)
        );

        string hashVal = "";
        foreach (KeyValuePair<string, string> pair in responseParams)
        {
            string escapedValue = pair.Value.Replace("\\", "\\\\").Replace("|", "\\|");
            string lowerValue = pair.Key.ToLowerInvariant();
            if (!"encoding".Equals(lowerValue, StringComparison.OrdinalIgnoreCase) &&
                !"hash".Equals(lowerValue, StringComparison.OrdinalIgnoreCase) &&
                !"countdown".Equals(lowerValue, StringComparison.OrdinalIgnoreCase))
            {
                hashVal += escapedValue + "|";
            }
        }

        hashVal += _storeKey;

        System.Security.Cryptography.SHA512 sha = new System.Security.Cryptography.SHA512CryptoServiceProvider();
        // byte[] hashbytes = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(hashVal);
        byte[] hashbytes = System.Text.Encoding.UTF8.GetBytes(hashVal);
        byte[] inputbytes = sha.ComputeHash(hashbytes);
        string actualHash = System.Convert.ToBase64String(inputbytes);

        string retrievedHash = responseParams.FirstOrDefault(x => x.Key.Equals("hash", StringComparison.OrdinalIgnoreCase)).Value;
        if (!actualHash.Equals(retrievedHash))
        {
            // Security Alert. The digital signature is not valid. HASH mismatch.
            return false;
        }
        else
        {
            // Hash is SUCCESSFULL.
            return true;
        }
    }

    public class InitializeModel
    {
        public string ReturnOId { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Lang { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string CustomerId { get; set; }
        public string CustomerNameSurname { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
    }
}