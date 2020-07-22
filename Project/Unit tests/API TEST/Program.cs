using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace API_TEST
{
    class Program
    {
        private const string SERVER_URL = "http://localhost:50932/api/RateBeer/";

        static void Main(string[] args)
        {
            if (!addRateEx())//expected value
            {
                Console.WriteLine("Error 1");
            }
            else if(!addRateBo(1))//boundary value for rating min 1
            {
                Console.WriteLine("Error 2");
            }
            else if (!addRateBo(5))//boundary value for rating max 5
            {
                Console.WriteLine("Error 3");
            }
            else if (!addRateUN("test@test.com", 0, "3", "Bad Request"))//unexpected value for rating
            {
                Console.WriteLine("Error 4");
            }
            else if (!addRateUN("user.com", 4, "2", ""))//unexpected value for email
            {
                Console.WriteLine("Error 5");
            }
            else if (!addRateUN("test@test.com", 3, "114514", "Not Found"))//unexpected value for beer id
            {
                Console.WriteLine("Error 6");
            }
            else
            {
                Console.WriteLine("All tests passed");
            }
        }

        /**
         * This method will test the add rate controller for expected value
         */
        private static bool addRateEx()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SERVER_URL + "5");
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = "{\"username\": \"test@test.com\", \"rating\": 3, \"comment\": \"This is a TEST comment\"}";

                streamWriter.Write(json);
            }

            var content = string.Empty;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                
                JObject respondObject = JObject.Parse(content);

                if (Convert.ToInt32(respondObject["statusCode"].ToString()) == 200)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Faill: statusCode is not 200");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Faill: exception is thrown");
                return false;
            }
        }

        /**
         * This method will test the add rate controller for boundary value
         */
        private static bool addRateBo(int rate)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SERVER_URL + "4");
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = $"{{\"username\": \"Btest@test.com\", \"rating\": {rate}, \"comment\": \"This is a TEST comment\"}}";

                streamWriter.Write(json);
            }

            var content = string.Empty;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                JObject respondObject = JObject.Parse(content);

                if (Convert.ToInt32(respondObject["statusCode"].ToString()) == 200)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Faill: statusCode is not 200");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Faill: exception is thrown");
                return false;
            }
        }

        /**
         * This method will test the add rate controller for unexpected value
         */
        private static bool addRateUN(String email, int rate, String id, String result)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SERVER_URL + id);
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = $"{{\"username\": \"{email}\", \"rating\": {rate}, \"comment\": \"This is a TEST comment\"}}";

                streamWriter.Write(json);
            }

            var content = string.Empty;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                JObject respondObject = JObject.Parse(content);

                if (result == respondObject["reasonPhrase"].ToString())
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Faill: exception is not handle by server");
                    return false;
                }
            }
            catch
            {
                return true;
            }
        }
    }
}
