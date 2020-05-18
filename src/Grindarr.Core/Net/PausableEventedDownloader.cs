using Grindarr.Core.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace Grindarr.Core.Net
{
    /// <summary>
    /// Provides a generic utility to download files. 
    /// It follows redirects, raises events on download start/stop/change, 
    /// and is pausable/resumable from where it left off (on disk)
    /// </summary>
    public class PausableEventedDownloader
    {
        /// <summary>
        /// The amount of bytes read from the network at once
        /// </summary>
        private const int CHUNK_SIZE = 4096;

        private bool doDownload = true;
        private bool failed = false;
        private HttpWebResponse lastResponse = null;

        public long Size { get; private set; }
        public long Progress { get; private set; }

        public Uri Uri { get; private set; }
        public string Filename { get; private set; }
        public string ResponseFilename { get; private set; }

        public event EventHandler ProgressChanged;
        public event EventHandler StatusChanged;
        public event EventHandler<ResponseFilenameEventArgs> ReceivedResponseFilename;

        public PausableEventedDownloader(Uri url, string file)
        {
            this.Uri = url;
            this.Filename = file;
        }

        /// <summary>
        /// Whether the download is currently paused or stopped
        /// </summary>
        /// <returns></returns>
        public bool IsPaused() => !doDownload;

        /// <summary>
        /// Whether the download has completed (assuming size > 0)
        /// </summary>
        /// <returns></returns>
        public bool IsDone() => Progress == Size && Size > 0;

        /// <summary>
        /// Whether the download has failed
        /// </summary>
        /// <returns></returns>
        public bool HasFailed() => failed;

        public async void StartDownloadAsync()
        {
            Progress = 0;
            Size = 0;
            doDownload = true;
            await Task.Run(DownloadWorkerAsync);
            StatusChanged?.Invoke(this, new EventArgs());
        }

        public void Stop() => Pause();

        public void Pause()
        {
            doDownload = false;
            StatusChanged?.Invoke(this, new EventArgs());
        }

        public async void Resume()
        {
            doDownload = true;
            await Task.Run(DownloadWorkerAsync);
            StatusChanged?.Invoke(this, new EventArgs());
        }

        private async void DownloadWorkerAsync()
        {
            try
            {
                // Initialize reader/writer
                using var fs = GetOrUpdateFileStream();
                var httpResponse = GetOrCreateWebStream();

                // Gets the stream associated with the response.
                Stream receiveStream = httpResponse.GetResponseStream();

                // Better handle resulting file size than blindly using content-length when
                // a range could result in content-length being a portion of the size.
                // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Range
                var contentRangeHeader = httpResponse.GetResponseHeader("Content-Range");
                if (string.IsNullOrEmpty(contentRangeHeader))
                {
                    Size = httpResponse.ContentLength;
                }
                else
                {
                    var sizeHeaderStr = contentRangeHeader.Split("/").LastOrDefault();
                    if (long.TryParse(sizeHeaderStr, out long parsedSize))
                        Size = parsedSize;
                }

                byte[] read = new byte[CHUNK_SIZE];

                while (doDownload)
                {
                    var count = await receiveStream.ReadAsync(read, 0, CHUNK_SIZE);
                    if (count == 0) // End of stream, etc
                    {
                        lastResponse = null;
                        break;
                    }
                    await fs.WriteAsync(read, 0, count);
                    Progress += count;
                    ProgressChanged?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex) // WebException, IOException
            {
                failed = true;
                //throw ex;
            }
            finally
            {
                StatusChanged?.Invoke(this, new EventArgs());
            }
        }

        private HttpWebResponse GetOrCreateWebStream()
        {
            if (lastResponse != null)
                return lastResponse;
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(Uri);
            if (Progress > 0)
                httpRequest.AddRange(Progress);

            lastResponse = GetUnredirectedResponse(httpRequest);

            var contentDispositionHeader = lastResponse.Headers.Get("Content-Disposition");
            if (!string.IsNullOrEmpty(contentDispositionHeader))
            {
                ContentDisposition contentDisposition = new ContentDisposition(contentDispositionHeader);
                ResponseFilename = contentDisposition.FileName;
                ReceivedResponseFilename?.Invoke(this, new ResponseFilenameEventArgs(ResponseFilename));
            }

            return lastResponse;
        }

        private HttpWebResponse GetUnredirectedResponse(WebRequest req)
        {
            try
            {
                var res = req.GetResponse();
                return (HttpWebResponse)res;
            }
            catch (WebException ex)
            {
                // We have to manually deal with redirects here because .NET Core does not 
                // auto redirect https to http in the name of safety. While this is a noble 
                // effort, and probably the correct to handle it, unfortunately some sites
                // do not do this and redirect down to https. So this will hopefully handle 
                // that. 
                var httpResp = ((HttpWebResponse)ex.Response);
                if ((int)httpResp.StatusCode >= 300 && (int)httpResp.StatusCode < 400)
                {
                    var redirect = httpResp.GetResponseHeader("Location");
                    if (!string.IsNullOrEmpty(redirect))
                    {
                        Log.WriteLine("Following redirect to: " + redirect);
                        var newUrl = new Uri(redirect);
                        var newFn = newUrl.Segments.Last();
                        ReceivedResponseFilename?.Invoke(this, new ResponseFilenameEventArgs(HttpUtility.UrlDecode(newFn)));

                        HttpWebRequest newRequest = (HttpWebRequest)WebRequest.Create(newUrl);
                        if (Progress > 0)
                            newRequest.AddRange(Progress);
                        return GetUnredirectedResponse(newRequest);
                    }
                }
                // Rethrow error if it's not a 300-type redirect
                throw ex;
            }
        }

        private Stream GetOrUpdateFileStream()
        {
            FileMode filemode;
            // For download resume
            // TODO: this does not work
            if (Progress == 0)
                filemode = FileMode.CreateNew;
            else
                filemode = FileMode.Append;

            if (File.Exists(Filename))
            {
                filemode = FileMode.Append;
                var size = new FileInfo(Filename).Length;
                Progress = size;
                //File.Delete(Filename);
            }
            return new FileStream(Filename, filemode);
        }
    }
}
