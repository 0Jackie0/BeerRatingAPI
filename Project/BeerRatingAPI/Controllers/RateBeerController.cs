using BeerRatingAPI.ActionFilter;
using BeerRatingAPI.Domain;
using Microsoft.AspNetCore.Mvc;
using Nancy.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;


namespace BeerRatingAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RateBeerController : ControllerBase
    {
        //Rate data file location
        private const string DATA_FILE_PATH = "..\\BeerRatingAPI\\Database\\database.json";
        //punkapi call for id check
        private const string ID_CHECK_URL = "https://api.punkapi.com/v2/beers/";
        //punkapi call for get beers by name
        private const string GET_BEER_BY_NAME_URL = "https://api.punkapi.com/v2/beers?beer_name=";

        [HttpPost("{id}")]//post maping with a url pram of beer id
        [UsernameFilter]//This attribute will check the email (valid email)
        /**
         * This method is the endpoint for adding a new rate
         **/
        public HttpResponseMessage addRate(int id, [FromBody] JsonElement newRate)
        {
            //Log endpoint action
            Console.WriteLine($"Add new Rate id: {id}");

            //Set up request for validating beer id
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ID_CHECK_URL + $"{id}");
            request.Method = "GET";
            var content = string.Empty;

            try
            {
                //get beer information
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
                //when there is a exception that means the beer id is not valid
                var respondMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
                respondMessage.Content = new StringContent("Beer does not exist");

                return respondMessage;
            }

            //check for rate datafile existence
            if (!System.IO.File.Exists(DATA_FILE_PATH))
            {
                // Create a file to write.
                using (StreamWriter writer = System.IO.File.CreateText(DATA_FILE_PATH))
                {
                    writer.Write("");
                }
            }

            //check rate value
            if (Convert.ToInt32(newRate.GetProperty("rating").ToString()) < 1 || Convert.ToInt32(newRate.GetProperty("rating").ToString()) > 5)
            {
                var respondMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                respondMessage.Content = new StringContent("Rating must be with in 1 -- 5");

                return respondMessage;
            }

            //assemble domain object
            var newRateObj = new Rate();
            newRateObj.beerId = id;
            newRateObj.userName = newRate.GetProperty("username").ToString();
            newRateObj.comment = newRate.GetProperty("comment").ToString();
            newRateObj.rating = Convert.ToInt32(newRate.GetProperty("rating").ToString());

            //saving json format data to datafile
            using (StreamWriter writer = System.IO.File.AppendText(DATA_FILE_PATH))
            {
                //convet domain object in to json format string and add ","at the end for easy reading
                writer.WriteLine(new JavaScriptSerializer().Serialize(newRateObj) + ",");
            }


            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    
        [HttpGet("{name}")]//get maping with a url pram of beer name
        /**
         * This methid is a endpoint of get beer rate by name
         **/
        public String getBeers(string name)
        {
            //Log endpoint action
            Console.WriteLine($"Get Reating by name: {name}");

            //setup request to get the beers based on beer name 
            //(Base on punkapi document: space in the beer name should be underscore)
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GET_BEER_BY_NAME_URL + $"{name.Replace(" ","_")}");
            request.Method = "GET";
            var content = string.Empty;

            try
            {
                //get search result of beer name
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //when exception happend that means there are errors within punkapi 
                //(if beer name does not have search result this exception will not be triggered)
                //therefore when this happened this endpoint return empty list as result
                Console.WriteLine("Error");
                Console.WriteLine(e);
                return "[]";
            }

            //if the beer name search result is empty then endpoint return EMPTY JSON ARRAY
            if(content == "[]")
            {
                return "[]";
            }

            //parse the search result in to json array
            JArray beerDataList = JArray.Parse(content);

            //check for rate json datafile existence
            if (!System.IO.File.Exists(DATA_FILE_PATH))
            {
                // Create a file to write.
                using (StreamWriter writer = System.IO.File.CreateText(DATA_FILE_PATH))
                {
                    writer.Write("");
                }
            }

            //Read all rate data in to memory
            String rateData = System.IO.File.ReadAllText(DATA_FILE_PATH);
            //create empty Json array in case there is no rating data
            JArray ratingDataList = new JArray();

            //check if rating data is empty
            if (rateData != "")
            {
                //parse rating json in to json array (the data in json file is not in correct format, so we need to fix it before parseing)
                ratingDataList = JArray.Parse($"[{rateData}]");
            }
            
            //create a empty arrray to contain the final result
            JArray respondDataList = new JArray();

            //Go though all the beer name search result
            foreach (JObject beer in beerDataList.Children())
            {
                //for each search result will be a json object in the final json array
                JObject newRateResult = new JObject();
                int targetId = Convert.ToInt32(beer.GetValue("id").ToString());
                String targetName = beer.GetValue("name").ToString();
                String targetDescription = beer.GetValue("description").ToString();

                //add beer info to the json object
                newRateResult.Add(new JProperty("id", targetId));
                newRateResult.Add(new JProperty("name", targetName));
                newRateResult.Add(new JProperty("description", targetDescription));


                //create empty json array to contain the rating info for this beer
                JArray userRatingList = new JArray();
                //Linq query to select all rate info that has the beer id
                var filterRatingList = ratingDataList.Where(eachRate => eachRate.Value<int>("beerId") == targetId);

                //go though the select result
                foreach (JObject eachRate in filterRatingList)
                {
                    //for each rate info is a json object in the "userRatings" json array
                    JObject temp = new JObject();

                    //add rate info in to json object
                    temp.Add(new JProperty("username", eachRate.GetValue("userName").ToString()));
                    temp.Add(new JProperty("rating", eachRate.GetValue("rating").ToString()));
                    temp.Add(new JProperty("comments", eachRate.GetValue("comment").ToString()));

                    //add json object in to "userRatings" json array
                    userRatingList.Add(temp);
                }
                //add "userRatings" json array in to beer json object with value name "userRatings"
                newRateResult.Add(new JProperty("userRatings", userRatingList));

                //add beer json object in to final json array
                respondDataList.Add(newRateResult);
            }

            //endpont respond the json array as string
            return respondDataList.ToString();
        }
    }
}
