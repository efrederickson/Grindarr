using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Grindarr.Core.Logging
{
    public class Log
    {
        private static Log _instance = null;
        public static Log Instance => _instance ??= new Log();

        public bool PrependTimestamp { get => logger.PrependTimestamp; set => logger.PrependTimestamp = value; }

        private ILogger logger = null;

        public Log()
        {
            // By default configure to console
            ConfigureLogger(new ConsoleLogger());
        }

        public void ConfigureLogger(ILogger logger) => this.logger = logger;

        public static void WriteLine(string message) => Instance.logger.WriteLine(message);

        public static void WriteLine(object messageObject) => Instance.logger.WriteLine(messageObject);

        public static void WriteEnumerable<T>(IEnumerable<T> messageObject) => Instance.logger.WriteEnumerable(messageObject);

        public static void WriteAsyncEnumerable<T>(IAsyncEnumerable<T> messageObject) => Instance.logger.WriteAsyncEnumerable(messageObject);
    }
}
