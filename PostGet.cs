using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenetousSDKCore
{
    public static class UrlTypes
    {
        public static readonly string addCollection = "dataservice/add/collection";
        public static readonly string addUniqueCollection = "dataservice/add/unique/collection";
        public static readonly string addMultiCollection = "dataservice/add/multicollection";
        public static readonly string addRelation = "dataservice/add/relation";
        public static readonly string deleteCollection = "dataservice/delete/collection";
        public static readonly string updateCollection = "dataservice/update/collection";
        public static readonly string getCollections = "dataservice/get/collections";
        public static readonly string getRelations = "dataservice/get/relations";
        public static readonly string isUnique = "dataservice/check/unique";
        public static readonly string createSecureLink = "dataservice/create/slink";
        public static readonly string updateSecure = "dataservice/update/slink";
        public static readonly string execute = "dataservice/execute/";
        public static readonly string cmd = "dataservice/cmd/";
        public static readonly string login = "dataservice/login";

        public static readonly string verifyToken = "authservice/verify";
        public static readonly string killToken = "authservice/logout";
        public static readonly string createClient = "authservice/client";
        public static readonly string auth = "authservice/auth";

    }
    public static class applicationVariables
    {
        public static string applicationId = "";
        public static string organizationId = "";
        public static string clientSecret = "";
        public static string host = "";
    }
    public static class Methods
    {
        public static readonly string POST = "Post";
        public static readonly string GET = "Get";
    }
    public class PostGet
    {

        private static readonly HttpClient client = new HttpClient();
        public string urlType { get; set; }
        public string method { get; set; }
        public string token { get; set; }
        public Dictionary<string, object> data { get; set; }
        public async Task<object> process()
        {
            StringContent content = null;
            HttpResponseMessage response = null;
            if (data != null)
            {
                var jsonDictionary = JsonConvert.SerializeObject(this.data);
                content = new StringContent(jsonDictionary, Encoding.UTF8, "application/json");
            }
            if (this.token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            }
            if (method == Methods.POST)
            {
                response = await client.PostAsync(applicationVariables.host + this.urlType, content);
            }
            else if (method == Methods.GET)
            {
                response = await client.GetAsync(applicationVariables.host + this.urlType);
            }
            object result = await returnResults(response);
            return result;
        }
        async Task<object> returnResults(HttpResponseMessage response)
        {
            try
            {
                var responseString = await response.Content.ReadAsStringAsync();
                object? result = null;
                if (response != null && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                    return result;
                }
                else if (response.Content.Headers.ContentType.MediaType == "application/json")
                {
                    result = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);
                    return result;
                }
                else
                {
                    Dictionary<string, object> r = new Dictionary<string, object>();
                    r.Add("StatusCode", response.StatusCode);
                    r.Add("Message", await response.Content.ReadAsStringAsync());
                    return r;
                }
            }
            catch (HttpRequestException e)
            {
                Dictionary<string, object> r = new Dictionary<string, object>();
                r.Add("message", "Exception Caught!(" + e.Message + ")");
                return r;
            }
        }
        public async Task<object> login()
        {

            Dictionary<string, object> tdata = new Dictionary<string, object>();
            tdata.Add("organizationId", applicationVariables.organizationId);
            tdata.Add("applicationId", applicationVariables.applicationId);
            var res = await GetTokenAsync(tdata);
            HttpResponseMessage response = null;
            if (res is Dictionary<string, object>)
            {
                Dictionary<string, object> r = (Dictionary<string, object>)res;
                if (!r.ContainsKey("success") || ((bool)r["success"]) == false)
                {
                    return res;
                }
                var jsonDictionary = JsonConvert.SerializeObject(this.data);
                var content = new StringContent(jsonDictionary, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r["token"].ToString());
                response = await client.PostAsync(applicationVariables.host + UrlTypes.login, content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await returnResults(response);
                    r = (Dictionary<string, object>)result;
                    if (r.ContainsKey("success") && (bool)r["success"] == true)
                    {
                        Dictionary<string, object> u = JObject.FromObject(r["data"]).ToObject<Dictionary<string, object>>();
                        Console.WriteLine(u["id"]);
                        if (tdata.ContainsKey("clientId"))
                        {
                            tdata.Remove("clientId");
                        }
                        if (u.ContainsKey("_id"))
                            tdata.Add("clientId", u["_id"]);
                        else if (u.ContainsKey("id"))
                            tdata.Add("clientId", u["id"]);
                        else
                            return await returnResults(response);
                        if (tdata.ContainsKey("clientSecret"))
                        {
                            tdata.Remove("clientSecret");
                        }
                        res = await GetTokenAsync(tdata);
                        Console.WriteLine("Token:" + token);
                        return res;

                    }
                    else
                    {
                        return result;
                    }

                }
                else
                {
                    return await returnResults(response);
                }
            }
            else
            {
                return res;

            }

        }
        async Task<object> GetTokenAsync(Dictionary<string, object> data)
        {
            var res = await createClient(data);
            if (res is Dictionary<string, object>)
            {
                res = await auth((Dictionary<string, object>)res);
                if (res is Dictionary<string, object>)
                {
                    return (Dictionary<string, object>)res;
                }
            }
            return res;

        }
        public async Task<object> guestToken()
        {
            Dictionary<string, object> tdata = new Dictionary<string, object>();
            tdata.Add("organizationId", applicationVariables.organizationId);
            tdata.Add("applicationId", applicationVariables.applicationId);
            object r = await GetTokenAsync(tdata);
            return r;
        }
        async Task<object> createClient(Dictionary<string, object> data)
        {
            HttpResponseMessage response = null;
            var jsonDictionary = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonDictionary, Encoding.UTF8, "application/json");
            response = await client.PostAsync(applicationVariables.host + UrlTypes.createClient, content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await returnResults(response);
                Dictionary<string, object> r = (Dictionary<string, object>)result;
                if (data.ContainsKey("clientId"))
                {
                    data.Remove("clientId");
                }
                data.Add("clientId", r["clientId"]);
                data.Add("clientSecret", applicationVariables.clientSecret);
                return data;

            }
            else
            {
                return response;
            }
        }
        async Task<object> auth(Dictionary<string, object> data)
        {
            HttpResponseMessage response = null;
            var jsonDictionary = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonDictionary, Encoding.UTF8, "application/json");
            response = await client.PostAsync(applicationVariables.host + UrlTypes.auth, content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await returnResults(response);
                Dictionary<string, object> r = (Dictionary<string, object>)result;
                return r;
            }
            return response;
        }
        async Task<object> VerifyTokenAsync()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            var response = await client.GetAsync(applicationVariables.host + UrlTypes.verifyToken);
            return await returnResults(response);
        }
        async Task<object> KillTokenAsync()
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);
            var response = await client.PostAsync(applicationVariables.host + UrlTypes.killToken, null);
            return await returnResults(response);
        }
        public async Task<object> verifyToken()
        {
            return await VerifyTokenAsync();
        }
        public async Task<object> killToken()
        {
            return await KillTokenAsync();
        }
    }
}
