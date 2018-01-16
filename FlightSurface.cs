public class FlightSurface{

    [HttpPost]
    public ActionResult bookingform()
    {
        String firstname = Request.Form["firstname"];
        String lastname = Request.Form["lastname"];
        String phone = Request.Form["phone"];
        String emailaddress = Request.Form["emailaddress"];
        String nationality = Request.Form["nationality"];
        String paymentmethod = Request.Form["paymentmethod"];
        String cardholder = Request.Form["cardholder"];
        String cardnumber = Request.Form["cardnumber"];
        String cardexpiremonth = Request.Form["cardexpiremonth"];
        String cardexpireyear = Request.Form["cardexpireyear"];
        String cvccode = Request.Form["cvccode"];
        String yourrequest = Request.Form["yourrequest"];

        DateTime now = DateTime.Now;
        string oid = "000000" + now.ToString("ddMMyyyyhhmmss");  // order id is datetime to unique

        // Email send to user
        String mail = "";
        String mail1 = "<p><h2>Receiving mail From Client " + firstname + "</h2>";
        String mail2 = "<p><h2>We received your order list. Please make sure to complete the payment process.</h2>";
        mail = mail + "<h3>Order ID : " + oid + "</h3>";
        mail = mail + "<h3>First Name : " + firstname + "</h3>";
        mail = mail + "<h3>Last Name : " + lastname + "</h3>";
        mail = mail + "<h3>Phone : " + phone + "</h3>";
        mail = mail + "<h3>Email : " + emailaddress + "</h3>";
        mail = mail + "<h3>Nationality : " + nationality + "</h3>";
        mail = mail + "<h3>Your Request : " + yourrequest + "</h3>";
        mail = mail + "<h2>Thank you</h2>";

        mail = mail + "<br><table width='80%' border=1 border-collapse='collapse'><thead><tr><th colspan='4'>Order Summary</th></tr><tr><th>DESCRIPTION</th><th>Qty</th><th>PRICE</th><th >TOTAL</th></tr></thead><tbody>";
        
        // Calculate total from session cart datas
        List<Tour> tours = Session["cart"] != null ? (List<Tour>)Session["cart"] : null;
        int finalprice = 0;
        int totalprice = 0;
        int index = 0;
        foreach (var t in tours)
        {
            totalprice = t.TourPrice * t.qty;
            finalprice = finalprice + totalprice;
            mail = mail + "<tr><td>" + t.TourTitle + "</td><td>" + t.qty + "</td><td>" + t.TourPrice + "</td><td>" + totalprice + "</td></tr>";
        }
    	
    	// new add for onlinebookfee({
    	int onlinebookfee = finalprice*5/100;
    	finalprice = finalprice+onlinebookfee;
    	// });

        mail = mail + "</tbody>";
        mail = mail + "<tfoot><tr><th colspan='3'>Online Booking Fees 5%</th><td>" + onlinebookfee + "</td></tr><tr><th colspan='3'>Total</th><td>" + finalprice + "</td></tr></tfoot></table>";

        mu.SendEmail("info@www.com", WebConfigurationManager.AppSettings["adminemail"].ToString(), "Order From Customer, Order ID : " + oid, mail1 + mail);
        mu.SendEmail("info@www.com", emailaddress, "Order Receive Mail From , Order ID : " + oid, mail2 + mail);


        // calculate amount that acceptable format accoding to 2c2p document    
        string amt = finalprice.ToString()+"00";
        while (amt.Length < 12)
        {
            amt = "0" + amt;
        }


        // prepare 2c2p information
        String version = "6.4";  // 2c2p version
        String merchant_id = WebConfigurationManager.AppSettings["merchant_id"].ToString();  // merchant id from owner
        String payment_description = "Payment From " + firstname + " " + lastname; // just description
        String order_id = oid;  // order id can set anything. but need to unique
        String invoice_no = oid; // order id can set anything. but need to unique
        String currency = "840";  // this is USD. there are many codes for other currency. check online
        String amount = amt; // amount according to 2c2p format. 12 character
        String customer_email = emailaddress; 
        String pay_category_id = "";
        String promotion = "";
        String user_defined_1 = emailaddress;
        String user_defined_2 = finalprice.ToString();
        String user_defined_3 = "";
        String user_defined_4 = "";
        String user_defined_5 = "";
        String result_url_1 = WebConfigurationManager.AppSettings["RedirectFrom2c2p"].ToString();  // success redirect url this is no need if register in 2c2p
        String result_url_2 = "";
        String secret_key = WebConfigurationManager.AppSettings["secret_key"].ToString(); // secret key from owner

        // call 2c2p hash algorithm for security
        string strSignatureString = version + merchant_id + payment_description + order_id + invoice_no + currency + amount + customer_email + pay_category_id + promotion + user_defined_1 + user_defined_2 + user_defined_3 + user_defined_4 + user_defined_5 + result_url_1 + result_url_2;
        string HashValue = getHMAC(strSignatureString, secret_key);

        String hash_value = HashValue;

        // prepare data to send view again
        var values = new Dictionary<string, string>
        {
            { "formaction", WebConfigurationManager.AppSettings["2c2pLink"].ToString() },
            { "version", version },
            { "merchant_id", merchant_id },
            { "payment_description", payment_description },
            { "order_id", order_id },
            { "invoice_no", invoice_no },
            { "currency", currency },
            { "amount", amount },
            { "customer_email", customer_email },
            { "pay_category_id", pay_category_id },
            { "promotion", promotion },
            { "user_defined_1", user_defined_1 },
            { "user_defined_2", user_defined_2 },
            { "user_defined_3", user_defined_3 },
            { "user_defined_4", user_defined_4 },
            { "user_defined_5", user_defined_5 },
            { "result_url_1", result_url_1 },
            { "result_url_2", result_url_2 },
            { "hash_value", hash_value }
        };

        return Json(values); // return to view ---->
    }

    private string getHMAC(string signatureString, string secretKey)
    {
        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
        byte[] keyByte = encoding.GetBytes(secretKey);
        HMACSHA1 hmac = new HMACSHA1(keyByte);
        byte[] messageBytes = encoding.GetBytes(signatureString);
        byte[] hashmessage = hmac.ComputeHash(messageBytes);
        return ByteArrayToHexString(hashmessage);
    }
    private string ByteArrayToHexString(byte[] Bytes)
    {
        StringBuilder Result = new StringBuilder();
        string HexAlphabet = "0123456789ABCDEF";
        foreach (byte B in Bytes)
        {
            Result.Append(HexAlphabet[(int)(B >> 4)]);
            Result.Append(HexAlphabet[(int)(B & 0xF)]);
        }
        return Result.ToString();
    }
}