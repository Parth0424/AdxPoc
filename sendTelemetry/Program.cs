using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Dynamic;

namespace sendTelemetry
{
    class Program
    {
        static void Main(string[] args)
        {
            // Specify the output file path.
            string outputPath = @"C:\Users\158227\Downloads\sendTelemetry\telemetry_data.json";

            // Load existing data from the JSON file if it exists.
            List<object> existingTelemetryData = LoadExistingData(outputPath);

            // Create a StreamWriter to write to the specified output file.
            using (StreamWriter writer = File.CreateText(outputPath))
            {
                // Add existing data to the new data.
                foreach (var existingData in existingTelemetryData)
                {
                    string jsonData = JsonConvert.SerializeObject(existingData);
                    writer.WriteLine(jsonData);
                }

                while (true)
                {
                    // Generate random data for each field.
                    string aId = "aId_3";
                    string appId = "appId_3";
                    Guid tenantId = Guid.NewGuid();
                    string modelId = "m_3";
                    dynamic data = new ExpandoObject();
                    data.Temperature = GetRandomTemperature();
                    data.Humidity = GetRandomHumidity();
                    int ts = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    DateTime ihts = DateTime.UtcNow;
                    DateTime sats = DateTime.UtcNow;

                    // Create an anonymous object to represent the telemetry data.
                    var telemetryData = new
                    {
                        aId,
                        appId,
                        tenantId,
                        modelId,
                        data,
                        ts,
                        ihts,
                        sats
                    };

                    // Serialize the object to JSON.
                    string jsonData = JsonConvert.SerializeObject(telemetryData);

                    // Write the JSON data to the output file.
                    writer.WriteLine(jsonData);
                    writer.Flush();

                    Console.WriteLine("Telemetry data written to file: " + jsonData);

                    // Wait for 1 second before sending the next telemetry message.
                    Thread.Sleep(1000);
                }
            }
        }

        static double GetRandomTemperature()
        {
            Random random = new Random();
            return 15 + random.NextDouble() * 15; // Generate temperature between 15°C and 30°C
        }

        static double GetRandomHumidity()
        {
            Random random = new Random();
            return 40 + random.NextDouble() * 30; // Generate humidity between 40% and 70%
        }

        static List<object> LoadExistingData(string outputPath)
        {
            List<object> existingData = new List<object>();

            if (File.Exists(outputPath))
            {
                using (StreamReader reader = File.OpenText(outputPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            var telemetryData = JsonConvert.DeserializeObject(line);
                            existingData.Add(telemetryData);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error loading existing data: " + ex.Message);
                        }
                    }
                }
            }

            return existingData;
        }
    }
}
