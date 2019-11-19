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
    [Route("logs/cortex/{traceId}")]
    public class CortexLoggerController : ApiController
    {


        private string Database;
        private string zone;
        private UxoLoggerController uxoLoggerController;

        public CortexLoggerController()
        {
            uxoLoggerController = new UxoLoggerController();
        }

        [HttpGet]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<HttpResponseMessage> GetCortexLogs(string traceId)
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

                zone = await uxoLoggerController.FetchZoneForTraceId(traceId, queryProvider);

                if (string.IsNullOrEmpty(zone))
                {
                    throw new Exception($"No Zone found for {traceId}");
                }

                if (string.Equals(zone, "WestUS2", StringComparison.InvariantCultureIgnoreCase))
                {
                    Database = DatabaseConstants.Cortex_PPE_Database;
                }
                else
                {
                    Database = DatabaseConstants.Cortex_Prod_Database;
                }

                var CortexTraceResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Cortex_Connection, Database, GetQuery("CortexTraceMDS", traceId), queryProvider);
                var CortexMonitoredScopeResult = await DatabaseQueryRunner.RunQuery(DatabaseConstants.Cortex_Connection, Database, GetQuery("CortexMonitoredScopeMDS", traceId), queryProvider);

                var result = new List<DatabaseResult>()
                    {
                        CortexTraceResult, CortexMonitoredScopeResult
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

        private string GetQuery(string tableName, string traceId)
        {
            return $"{tableName} | where trace_id == \"{traceId}\"";
        }
    }
}
