using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace temetryapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private string clusterUri = "https://kvc-byj9c2euw9pc3kp9wx.australiaeast.kusto.windows.net";
        private string databaseName = "MyDatabase"; // Replace with your actual database name
        private string tableName = "telemetry";

        [HttpGet]
        [Route("telemetry")]
        public async Task<ActionResult<List<Dictionary<string, object>>>> GetTelemetryData(string timeInterval)
        {
            if (!int.TryParse(timeInterval, out int timeInMinutes))
            {
                return BadRequest("Invalid time interval. Please provide a valid integer value in minutes.");
            }

            try
            {
                KustoConnectionStringBuilder kcsb = new KustoConnectionStringBuilder(clusterUri)
                {
                    FederatedSecurity = true,
                    InitialCatalog = databaseName
                };

                using (var queryService = KustoClientFactory.CreateCslQueryProvider(kcsb))
                {
                    string query = GenerateQuery(timeInMinutes);

                    // Create ClientRequestProperties
                    var clientRequestProperties = new ClientRequestProperties();

                    using (var reader = await queryService.ExecuteQueryAsync(databaseName, query, clientRequestProperties))
                    {
                        // Process the query result and return data to the client
                        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
                        var columnNames = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();

                        while (reader.Read())
                        {
                            var result = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var columnName = columnNames[i];
                                if (columnName == "data")
                                {
                                    // Convert dynamic data to a string
                                    var dynamicData = reader[columnName] as JToken;
                                    if (dynamicData != null)
                                    {
                                        result[columnName] = dynamicData.ToString();
                                    }
                                }
                                else
                                {
                                    result[columnName] = reader[i];
                                }
                            }
                            results.Add(result);
                        }

                        return Ok(results);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private string GenerateQuery(int timeInMinutes)
        {
            DateTime endTime = DateTime.UtcNow;
            DateTime startTime = endTime.AddMinutes(-timeInMinutes); // Calculate the start time

            string query = $@"
                {tableName}
                | where sats >= datetime({startTime:yyyy-MM-ddTHH:mm:ss.fffffffZ}) and sats <= datetime({endTime:yyyy-MM-ddTHH:mm:ss.fffffffZ})
                | extend dataAsString = tostring(data) // Cast dynamic data to a string
                | summarize any(aId, appId, tenantId, modelId, dataAsString, ts, ihts, sats) by bin(sats, 1m);
            ";

            //string query = $@"
            //    {tableName}
            //    | where sats >= datetime(2023-09-05T09:55:51.7454447Z) and sats <= datetime({endTime:yyyy-MM-ddTHH:mm:ss.fffffffZ})
            //    | extend dataAsString = tostring(data) // Cast dynamic data to a string
            //    | summarize any(aId, appId, tenantId, modelId, dataAsString, ts, ihts, sats) by bin(sats, 1m);
            //";

            //string query = $@"
            //{tableName}
            //| project machineId = modelId, data
            //| where data.Temperature == 16.761517043580078 
            //| project machineId, data
            //";

            return query;
        }
    }
}
