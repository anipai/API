using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBMApiAnalytics.Utils
{
    public static class ApplicationSettingsHelper
    {
        public static T SafeGet<T>(this NameValueCollection appSettings,  string settingKey, T defaultValue) where T : IConvertible
        {
            T returnValue; 
            try
            {
                var settingValue = appSettings[settingKey];
                if (settingValue == null)
                {
                    throw new MissingMemberException();
                }
                returnValue = (T) Convert.ChangeType(settingValue, typeof(T));
            }
            catch
            {
                returnValue = defaultValue;
            }
            return returnValue;

        }
    }
}
