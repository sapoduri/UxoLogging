using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Kusto.Data;
using Kusto.Data.Net.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using UxoLoggingv2.Controllers;
using UxoLoggingv2.Utils;

namespace UxoLogging.Controllers
{
    [Route("logs/speech/{traceId}")]
    public class SpeechLoggerController : ApiController
    {
        private string speechTraceId;
        private UxoLoggerController uxoLoggerController;

        public SpeechLoggerController()
        {
            uxoLoggerController = new UxoLoggerController();
        }

        [HttpGet]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> GetSpeechLogs(string traceId)
        {
            try
            {
                if (string.IsNullOrEmpty(traceId))
                {
                    throw new Exception($"Invalid traceId {traceId}");
                }

                string authority = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47";
                string clientId = "db662dc1-0cfe-4e1c-a843-19a68e65be58";
                string redirectUrl = "https://microsoft/kustoclient";
                AuthenticationContext authContext = new AuthenticationContext(authority);

                AuthenticationResult authenticationResult = authContext.AcquireTokenAsync(DatabaseConstants.Uxo_Connection, clientId, new Uri(redirectUrl), new PlatformParameters(PromptBehavior.Always)).GetAwaiter().GetResult();
                Console.WriteLine(authenticationResult.AccessToken);

                var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(DatabaseConstants.Uxo_Connection)
                {
                    FederatedSecurity = true,
                    InitialCatalog = "NetDefaultDB",
                    Authority = authority,
                    UserToken = authenticationResult.AccessToken
                };

                var queryProvider = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder);

                speechTraceId = await uxoLoggerController.FetchSpeechTraceId(traceId, queryProvider);

                if (string.IsNullOrEmpty(speechTraceId))
                {
                    throw new Exception("No Speech logs found");
                }

                var phraseTextResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("phrasetext", "ImpressionId", speechTraceId), queryProvider);
                var websocketDebugInfoResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("websocketdebuginfo", "impressionId", speechTraceId), queryProvider);
                var traceDebugInfoResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("tracedebuginfo", "impressionId", speechTraceId), queryProvider);
                var decoderEventsDebugInfoResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("decodereventsdebuginfo", "impressionId", speechTraceId), queryProvider);
                var turnStopResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("turnstop", "impressionId", speechTraceId), queryProvider);
                var outboundgrpcDebugInfoResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Speech_Connection, DatabaseConstants.Speech_Database, GetQuery("outboundgrpcdebuginfo", "impressionId", speechTraceId), queryProvider);

                var result = new List<DatabaseResult>()
                    {
                         phraseTextResult, websocketDebugInfoResult, traceDebugInfoResult, decoderEventsDebugInfoResult, turnStopResult, outboundgrpcDebugInfoResult
                    };

                return new HttpResponseMessage()
                {
                    Content = new StringContent(JsonConvert.SerializeObject(result))
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

        private string GetQuery(string tableName, string columnName, string speechTraceId)
        {
            return $"{tableName} | where {columnName} == tostring(toguid(\"{speechTraceId}\"))";
        }

    }
}