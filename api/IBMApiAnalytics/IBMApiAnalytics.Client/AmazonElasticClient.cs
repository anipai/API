using System;
using System.Collections.Generic;
using IBMApiAnalytics.Models;
using Nest;


namespace IBMApiAnalytics.Client
{
    public class AmazonElasticClient
    {
        public void Index(string elastic)
        {
            var file = new System.IO.StreamReader("amazon.csv");
            var data = ServiceStack.Text.CsvSerializer.DeserializeFromReader<List<AmazonData>>(file);
            //var amazonData = CsvSerializer.DeserializeFromString<AmazonData>(file.ReadToEnd());
            data.RemoveAll(d => d.Record_type.Trim().Equals("RESPONSE"));
            data.ForEach(d => { if (d.Record_type.Trim().Equals("FAILURE")) { d.Record_type = "Amazon OLP"; d.Requests = d.Requests * 5; } });
            file.Close();
            file.Dispose();
            var node = new Uri(elastic);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("amazon");
            var client = new Nest.ElasticClient(settings);
            var response = client.IndexMany<AmazonData>(data);

        }
    }
}
