using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using UxoLoggingv2.Utils;


using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using System.Web;
using System.IdentityModel.Tokens.Jwt;

namespace UxoLoggingv2.Controllers
{
    [Route("logs/uxo/{traceId}")]
    public class UxoLoggerController : ApiController
    {
        private const string Connection = "https://cortanadiagnosticslogs.kusto.windows.net/";
        private const string Database = "CompliantCortana";

        [System.Web.Mvc.HttpGet]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> GetUxoLogs(string traceId)
        {
            try
            {
                if (string.IsNullOrEmpty(traceId))
                {
                    throw new Exception($"Invalid traceId {traceId}");
                }


                string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IkJCOENlRlZxeWFHckdOdWVoSklpTDRkZmp6dyIsImtpZCI6IkJCOENlRlZxeWFHckdOdWVoSklpTDRkZmp6dyJ9.eyJhdWQiOiJkYjY2MmRjMS0wY2ZlLTRlMWMtYTg0My0xOWE2OGU2NWJlNTgiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNTc0MTg4ODYyLCJuYmYiOjE1NzQxODg4NjIsImV4cCI6MTU3NDE5Mjc2MiwiYWlvIjoiQVZRQXEvOE5BQUFBbmpyTlpDVVFWNEFaeVg0dlk1aHVwKzBHVTRKcUJxSm5qMGNqQnJsVHllejNXWXRRalNQc2hLRHgvU3NWK0hFaGdGV2s0RUdyUCtZQnRqMXpTaEd0VG8zcFpGS0ZOMUVtTWFVNG8veVI0OGs9IiwiYW1yIjpbInB3ZCIsIm1mYSJdLCJmYW1pbHlfbmFtZSI6IlBvZHVyaSIsImdpdmVuX25hbWUiOiJTYWljaGFyYW4iLCJpcGFkZHIiOiIxNjcuMjIwLjI0LjExMSIsIm5hbWUiOiJTYWljaGFyYW4gUG9kdXJpIiwibm9uY2UiOiJiZmUxMWM3Mi0yODZkLTQ5ZjgtOWEwMi02NzMxOTEzNDY3YTIiLCJvaWQiOiI5Mzk5Yzk1YS03YjhlLTQwNzktODM5MS1lYTEyMjM3ZTg2ZDkiLCJvbnByZW1fc2lkIjoiUy0xLTUtMjEtMjEyNzUyMTE4NC0xNjA0MDEyOTIwLTE4ODc5Mjc1MjctMzI4MzczOTgiLCJyaCI6IkkiLCJzdWIiOiJZWVh4d29FWUg4cG9yRVZxTW5rY3ZyUDJBczlCd2R4UEhxb0E5LTR0UTF3IiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJzYXBvZHVyaUBtaWNyb3NvZnQuY29tIiwidXBuIjoic2Fwb2R1cmlAbWljcm9zb2Z0LmNvbSIsInV0aSI6ImxHbmF2Yjh3ZkVpWEQ4Qk02QkVCQUEiLCJ2ZXIiOiIxLjAifQ.GmgoMwnBYFFe1VnJ3Usqeh9p6GVjHZ1JgyhJWe7fuzMNXsxZE9A7l4tWH_4HQUTRwUrgpBd54e_glDN1QIVw8xRfjLUA8ATHsBG_ctpyTK5RmYIK3E58JKPPJgr7PxS3tSlZhRGWFiNBYo-K9qrQtdTUq2Uky-JEEMZapFBOIBzrW6Ut5xivUmGD8lEligk5Z6tsZm3Vc-pAZiGnNturp0lSgonMJ-ijpY7oMqB2SyMYlkuguBedpFbHY4Og5BIZG-kUKM0nUZomQl3RAqGn09W6DWC8-DISEbicZ761WtWebQ-cCNDyoBFFjFOFX6l-PPTLaY3w-cK_MBdxG8F7qw&state=412d483e-5f55-4674-84f0-d247ddfe663f&session_state=e55787ff-0b9e-4e2d-9969-b943ffa0ee14";
                var jwtHandler = new JwtSecurityTokenHandler();
                var readableToken = jwtHandler.ReadJwtToken(token);
                var claims = readableToken.Claims;


                string oid = "9399c95a-7b8e-4079-8391-ea12237e86d9";
                string clientId = "db662dc1-0cfe-4e1c-a843-19a68e65be58";
                string redirectUrl = "https://microsoft/kustoclient";
                var userIdentifier = new UserIdentifier(oid, UserIdentifierType.UniqueId);
                string authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47";
                AuthenticationContext authContext = new AuthenticationContext(authority);
                AuthenticationResult authResult = await authContext.AcquireTokenAsync(DatabaseConstants.Uxo_Connection, clientId, new Uri(redirectUrl), new PlatformParameters(PromptBehavior.Never), userIdentifier);
                Console.WriteLine(authResult.AccessToken);


                var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(DatabaseConstants.Uxo_Connection)
                {
                    FederatedSecurity = true,
                    InitialCatalog = "NetDefaultDB",
                    Authority = authority,
                    UserToken = authResult.AccessToken
                };

                var queryProvider = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder);

                var UxoRequestResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, GetQuery("UxoRequest", traceId), queryProvider);
                var UxoConnectionResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, GetQuery("UxoConnection", traceId), queryProvider);
                var UxoTraceResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, GetQuery("UxoTrace", traceId), queryProvider);
                var UxoWebSocketMessageResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, GetQuery("UxoWebSocketMessage", traceId), queryProvider);
                var UxoEventResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, GetQuery("UxoEvent", traceId), queryProvider);

                var result1 = new List<DatabaseResult>()
                        {
                            UxoRequestResult, UxoConnectionResult, UxoTraceResult, UxoWebSocketMessageResult, UxoEventResult
                        };
                return new HttpResponseMessage()
                {
                    Content = new StringContent(JsonConvert.SerializeObject(result1))
                };
            }
            catch (Exception e)
            {
                Console.Write(e);
                return new HttpResponseMessage()
                {
                    Content = new StringContent(e.Message),
                    StatusCode = System.Net.HttpStatusCode.InternalServerError
                };
            }
        }

        private string GetQuery(string tableName, string traceId)
        {
            return $"{tableName} | where TraceId == \"{traceId}\"";
        }

        public async Task<string> FetchSpeechTraceId(string uxoTraceId, ICslQueryProvider queryProvider)
        {
            var query = $"UxoRequest | where TraceId == \"{uxoTraceId}\" | project SpeechTraceId | order by SpeechTraceId | take 1";
            var queryResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, query, queryProvider);
            object[] result = (object[])queryResult.Results[0];
            return result[0].ToString();
        }

        public async Task<string> FetchZoneForTraceId(string uxoTraceId, ICslQueryProvider queryProvider)
        {
            var query = $"UxoTrace | where TraceId == \"{uxoTraceId}\" | project Zone | take 1";
            var queryResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Uxo_Connection, DatabaseConstants.Uxo_Database, query, queryProvider);
            object[] result = (object[])queryResult.Results[0];
            return result[0].ToString();
        }
    }
}
