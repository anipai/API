using System;
using System.Collections.Generic;

namespace IBMApiAnalytics.Models
{
    public class AmazonData
    {
        public DateTime Day_of_Year { get; set; }
        public string Operation_name { get; set; }
        public string Record_type { get; set; }
        public int Requests { get; set; }
    }

    public class CallVolumePerDay
    {
        public string Id { get; set; }
        public DateTime datetime { get; set; }
        public string day { get; set; }
        public int count { get; set; }
    }

    public class AmazonSummary
    {
        public DateTime Day_of_Year { get; set; }
        public string Operation_name { get; set; }
        public string Record_type { get; set; }
        public int Requests { get; set; }
        public int Sum { get; set; }
    }

    public class Call
    {
        public string Id { get; set; }
        public string datetime { get; set; }
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public string appName { get; set; }
        public string envName { get; set; }
        public string planName { get; set; }
        public string planVersion { get; set; }
        public string devOrgName { get; set; }
        public string resourceName { get; set; }
        public int timeToServeRequest { get; set; }
        public int bytesSent { get; set; }
        public string requestProtocol { get; set; }
        public string requestMethod { get; set; }
        public string uriPath { get; set; }
        public string queryString { get; set; }
        public string statusCode { get; set; }
        public string requestHeaders { get; set; }
        public string userAgent { get; set; }
        public string requestBody { get; set; }
        public string responseHeaders { get; set; }
        public string responseBody { get; set; }
        public string latency { get; set; }
        public int? bytesReceived { get; set; }
        public string productName { get; set; }
    }

    public class CallLit
    {
        public string Id { get; set; }
        public string datetime { get; set; }
        public string apiName { get; set; }
        public string productName { get; set; }
        public string devOrgName { get; set; }
        public int timeToServeRequest { get; set; }
        public string requestBody { get; set; }
        public string statusCode { get; set; }
        public string responseBody { get; set; }
        public string planName { get; set; }
    }

    public class CallSummaryForOrg
    {
        public string Id { get; set; }
        public string Org { get; set; }
        public string Api { get; set; }
        public int Count { get; set; }
        public int Ok200 { get; set; }
        public int Error5X { get; set; }
        public int Error4X { get; set; }
        public int Error3x { get; set; }
        public decimal SuccessPercent { get; set; }
        public DateTime datetime { get; set; }
    }

    public class CallSummary
    {
        public string Id { get; set; }
        public string Api { get; set; }
        public int Count { get; set; }
        public int Ok200 { get; set; }
        public int Error5X { get; set; }
        public int Error4X { get; set; }
        public int Error3x { get; set; }
        public decimal SuccessPercent { get; set; }
        public DateTime datetime { get; set; }
    }
    public class Log
    {
        public int totalCalls { get; set; }
        public string next { get; set; }
        public string nextHref { get; set; }
        public List<Call> calls { get; set; }
    }

    public enum TimeRangeType
    {
        Specific,
        XDays
    }
}
