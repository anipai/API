using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBMApiAnalytics.App
{
    public class ApplicationArguments
    {
        public string Credentials { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int NoOfDaysToProcess { get; set; }
    }
}
