using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeerRatingAPI.ActionFilter
{
    /**
     * This method is a filter attribute which will check the email in the request body 
     * to make sure it follows the correct email format 
     **/
    public class UsernameFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var syncIOFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIOFeature != null)
            {
                String jsonString = "";
                syncIOFeature.AllowSynchronousIO = true;

                var request = context.HttpContext.Request;

                request.EnableBuffering();

                // check if there is request body content
                if (request.Body.CanSeek)
                {
                    request.Body.Seek(0, SeekOrigin.Begin);

                    //read the request body
                    using (var reader = new StreamReader(request.Body, Encoding.UTF8, false, 8192, true))
                    {
                        jsonString = reader.ReadToEnd();
                    }

                    //reset pointer for other server function
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
                else
                {
                    //when no request body give server error as 406 NotAcceptable
                    context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(StatusCodes.Status406NotAcceptable);
                }

                //parse request body in to josn object
                JObject bodyObject = JObject.Parse(jsonString);

                //check username with regular expression
                if (Regex.IsMatch(bodyObject["username"].ToString(), @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase) == false)
                {
                    //when not match give server error as 406 NotAcceptable
                    context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(StatusCodes.Status406NotAcceptable);
                }
            }
            else
            {
                //when no access to syncIOFeature give server error as 409 Conflict
                context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(StatusCodes.Status409Conflict);
            }
        }
    }
}
