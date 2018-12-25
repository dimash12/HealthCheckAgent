using HealthCheckAgent.Metrics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheckAgent
{
    public class DataDogClient
    {
        private static HttpClient dataDogMetricSubmitter = new HttpClient
        {
            BaseAddress = new Uri("https://app.datadoghq.com")
        };

        public static async Task SendHealthReport(HealthReport healthReport, 
            ILogger log)
        {
            var metricAppPrefix = Environment.GetEnvironmentVariable("METRIC_APP_PREFIX");
            var metricTags = Environment.GetEnvironmentVariable("METRIC_TAGS");
            var posixTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            await SendMetricForOverallAppHealth(
                log, healthReport,
                metricAppPrefix,
                metricTags, posixTimeStamp);

            foreach (var item in healthReport.Entries)
            {
                await SendMetricForComponentHealth(
                    log, metricAppPrefix,
                    metricTags, posixTimeStamp, item);
            }
        }

        private static async Task SendMetricForComponentHealth(ILogger log,
            string metricAppPrefix,
            string metricTags,
            long posixTimeStamp,
            KeyValuePair<string, HealthReportEntry> item)
        {
            int metricCount = item.Value.Status == HealthStatus.Healthy ? 1 : 2;
            log.LogInformation($"Health test :{item.Key}, " +
                $"health: {(metricCount == 1 ? "healthy" : "unhealthy")}");

            CountMetric countMetric = new CountMetric(
                metricName: $"{metricAppPrefix}.app." +
                    $"{item.Key.ToLower().Replace(" ", "_")}.ishealthy",
                posixTimeStamp: posixTimeStamp,
                count: metricCount,
                tags: metricTags);

            await SendMetric(log, countMetric);
        }

        private static async Task SendMetricForOverallAppHealth(ILogger log,
            HealthReport healthReport,
            string metricAppPrefix,
            string metricTags,
            long posixTimeStamp)
        {
            var appHealthMetricCount =
                            healthReport.Status == HealthStatus.Healthy ? 1 : 2;
            CountMetric countMetric1 = new CountMetric(
                    metricName: $"{metricAppPrefix}.app.ishealthy",
                    posixTimeStamp: posixTimeStamp,
                    count: appHealthMetricCount,
                    tags: metricTags);

            await SendMetric(log, countMetric1);
        }

        private static async Task SendMetric(ILogger log, CountMetric countMetric)
        {
            var ddApiKey = Environment.GetEnvironmentVariable("DD_API_KEY");
            var ddAppKey = Environment.GetEnvironmentVariable("DD_APP_KEY");

            var json = JsonConvert.SerializeObject(countMetric, Formatting.Indented);
            var jsonContent = new StringContent(json,
                    Encoding.UTF8, "application/json");
            log.LogInformation($"JSON was {json}");

            var ddResponse = await dataDogMetricSubmitter.PostAsync(
                $"/api/v1/series?api_key={ddApiKey}&application_key={ddAppKey}",
                jsonContent);
            ddResponse.EnsureSuccessStatusCode();
            var ddResponseContent = await ddResponse.Content.ReadAsStringAsync();
            log.LogInformation($"DD returned {ddResponseContent}");
        }
    }
}
