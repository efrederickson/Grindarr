using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Whether the logger should prepend a timestamp like "[YYYYMMDDTHH:MMZ]" to the message
        /// </summary>
        public bool PrependTimestamp { get; set; }

        /// <summary>
        /// Writes a line to the log
        /// </summary>
        /// <param name="message"></param>
        public void WriteLine(string message);

        /// <summary>
        /// Writes the string representation of the object to the log
        /// </summary>
        /// <param name="messageObject"></param>
        public void WriteLine(object messageObject);

        /// <summary>
        /// Enumerates all the objects in the enumerable, and writes them individually to the log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject"></param>
        public void WriteEnumerable<T>(IEnumerable<T> messageObject);

        /// <summary>
        /// Enumerates all the objects in the enumerable, and writes them individually to the log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject"></param>
        public void WriteAsyncEnumerable<T>(IAsyncEnumerable<T> messageObject);
    }
}
