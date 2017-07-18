using System;

namespace IBMApiAnalytics.App
{
    public class CommandLineParser
    {
        public static ApplicationArguments Parse(string[] args)
        {
            var p = new Fclp.FluentCommandLineParser<ApplicationArguments>();

            p.Setup(arg => arg.Credentials)
                .As('c', "credentials") // define the short and long option name
                .Required(); 
            p.Setup(arg => arg.StartDateTime)
                .As('s', "startDate")
                .Required();

            p.Setup(arg => arg.EndDateTime)
                .As('e', "endDateTime")
                .Required(); 

            p.Setup(arg => arg.NoOfDaysToProcess)
                .As('d', "noOfDays")
                .SetDefault(1); // use the standard fluent Api to define a default value if non is specified in the arguments

            var result = p.Parse(args);

            if (result.HasErrors)
            {
                throw new ArgumentException("Please pass the correct arguments on the commandine {0}", result.ErrorText);
            }

            return p.Object;
        }
    }
}
