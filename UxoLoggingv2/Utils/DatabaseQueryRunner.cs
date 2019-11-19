using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace UxoLoggingv2.Utils
{
    public class DatabaseQueryRunner
    {
        public static async Task<DatabaseResult> RunQuery(string connection, string database, string query, ICslQueryProvider queryProvider)
        {
            try
            {
                var clientRequestProperties = new ClientRequestProperties() { ClientRequestId = Guid.NewGuid().ToString() };

                var reader = await queryProvider.ExecuteQueryAsync(database, query, clientRequestProperties);

                DatabaseResult dr = new DatabaseResult();
                dr.ConnectionString = connection;
                dr.Database = database;
                dr.Query = query;

                dr.Headers = new string[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dr.Headers[i] = reader.GetName(i);
                }

                dr.Results = new System.Collections.ArrayList();
                while (reader.Read() && reader.FieldCount > 0)
                {
                    object[] result = new object[reader.FieldCount];

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result[i] = reader.GetValue(i);
                    }
                    dr.Results.Add(result);
                }
                return dr;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
