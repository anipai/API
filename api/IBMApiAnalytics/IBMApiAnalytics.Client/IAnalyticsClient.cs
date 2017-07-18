using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBMApiAnalytics.Client
{
    public interface IAnalyticsClient
    {
        void LoadDetails();
        void LoadSummary();
        void LoadCounts();
    }
}
