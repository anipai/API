using System.Collections.Generic;
using System.Linq;
using IBMApiAnalytics.Models;
using ServiceStack.Text;

namespace IBMApiAnalytics.Client
{
    public class AnalyticsCsvClient
    {
        public void WriteSummaryWithOrg(IEnumerable<CallSummaryForOrg> data, string fileNameSuffix)
        {
            using (var file = new System.IO.StreamWriter($"apiSummaryForOrg_{fileNameSuffix}.csv"))
            {
                var csv = CsvSerializer.SerializeToCsv(data);
                file.Write(csv);
            }

        }

        public void WriteSummary(IEnumerable<CallSummary> data, string fileNameSuffix)
        {
            using (var file = new System.IO.StreamWriter($"apiSummary_{fileNameSuffix}.csv"))
            {
                var csv = CsvSerializer.SerializeToCsv(data);
                file.Write(csv);
            }

        }

        public void WriteDetails(IEnumerable<Call> data, string fileNameSuffix)
        {
            using (var file = new System.IO.StreamWriter($"apiDetail_{fileNameSuffix}.csv", true))
            {
                var csv = CsvSerializer.SerializeToCsv(data.Select(i => new { i.Id, i.apiName, i.apiVersion, i.appName, i.datetime, i.devOrgName, i.envName, i.planName, i.planVersion, i.statusCode, i.timeToServeRequest, i.latency, i.requestBody, i.queryString }));
                file.Write(csv);
            }
        }
    }
}
