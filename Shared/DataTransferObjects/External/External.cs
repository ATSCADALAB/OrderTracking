namespace Shared.DataTransferObjects.External
{
    public class OrderApiResponse
    {
        public OrderInfo order_info { get; set; }
        public List<Product> products { get; set; }
        public List<Payment> payments { get; set; }
        public List<string> terms { get; set; }
    }

    public class OrderInfo
    {
        public int order_id { get; set; }
        public string order_code { get; set; }
        public string order_date { get; set; }
        public int order_type { get; set; }
        public int order_status { get; set; }
        public decimal amount { get; set; }
        public decimal discount { get; set; }
        public decimal discount_amount { get; set; }
        public string created_at { get; set; }
        public int installation { get; set; }
        public decimal installation_amount { get; set; }
        public int transport { get; set; }
        public decimal transport_amount { get; set; }
        public string lading_status { get; set; }
        public int store_id { get; set; }
        public int account_id { get; set; }
        public string account_name { get; set; }
        public string account_code { get; set; }
        public string account_phone { get; set; }
        public string account_email { get; set; }
        public string address { get; set; }
        public string country_name { get; set; }
        public string province_name { get; set; }
        public string district_name { get; set; }
        public string assigned_name { get; set; }
        public string assigned_user_email { get; set; }
        public string is_repay { get; set; }
        public string account_created_at { get; set; }
        public string account_address { get; set; }
        public string payment_status { get; set; }
        public string lading_code { get; set; }
        public List<object> lading_info { get; set; }
    }

    public class Product
    {
        public int product_id { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
        public decimal discount_amount { get; set; }
        public decimal discount { get; set; }
        public decimal vat { get; set; }
        public decimal vat_amount { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public string unit_name { get; set; }
    }

    public class Payment
    {
        public decimal amount { get; set; }
        public string pay_date { get; set; }
        public string description { get; set; }
        public string created_at { get; set; }
    }

    public class AccountApiResponse
    {
        public AccountInfo info { get; set; }
        public List<Contact> contacts { get; set; }
    }

    public class AccountInfo
    {
        public string account_id { get; set; }
        public string account_code { get; set; }
        public string account_name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string birthday { get; set; }
        public string description { get; set; }
        public string relation_id { get; set; }
        public string revenue { get; set; }
        public string account_type { get; set; }
        public string created_at { get; set; }
        public string account_source { get; set; }
        public string account_manager { get; set; }
        public string province_name { get; set; }
        public string district_name { get; set; }
        public string country_name { get; set; }
        public string country_id { get; set; }
        public string province_id { get; set; }
        public string district_id { get; set; }
        public string industry { get; set; }
        public string creator_name { get; set; }
        public string industry_name { get; set; }
    }

    public class Contact
    {
        public string contact_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone_mobile { get; set; }
        public string phone_home { get; set; }
        public string email { get; set; }
        public string description { get; set; }
        public string title { get; set; }
    }
}