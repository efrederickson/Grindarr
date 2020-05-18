using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Logging
{
    public class ConsoleLogger : ILogger
    {
        public bool PrependTimestamp { get; set; } = true;

        private void WriteLineInternal(string msg)
        {
            var msgFormatted = new StringBuilder();
            if (PrependTimestamp)
                msgFormatted.Append(DateTime.Now.ToUniversalTime().ToString("[yyyy-MM-ddTHH:mm:ssZ] "));
            msgFormatted.Append(msg);
            Console.WriteLine(msgFormatted.ToString());
        }

        public async void WriteAsyncEnumerable<T>(IAsyncEnumerable<T> messageObject)
        {
            await foreach (var item in messageObject)
                WriteLineInternal(item.ToString());
        }

        public void WriteEnumerable<T>(IEnumerable<T> messageObject)
        {
            foreach (var item in messageObject)
                WriteLineInternal(item.ToString());
        }

        public void WriteLine(string message)
        {
            WriteLineInternal(message);
        }

        public void WriteLine(object messageObject)
        {
            WriteLineInternal(messageObject.ToString());
        }
    }
}
