using log4net.Config;
using System;

namespace RESTAPIClient
{
    public class Logger
    {
        private static readonly log4net.ILog log =
            log4net.LogManager.GetLogger("RollingLogFileAppender");
        static Logger()
        {
            //DOMConfigurator.Configure();
            XmlConfigurator.Configure();
            //BasicConfigurator.Configure();
        }
        public static void LogMessage(string Message)
        {
            log.Info(Message);
        }
        public static void LogException(string Message, Exception oException)
        {
            log.Error(Message, oException);
        }
    }
}
