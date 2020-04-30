using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace Grindarr.Core.Net
{
    public class PausableEventedDownloader
    {
        private const int CHUNK_SIZE = 2048;

        private bool doDownload = true;
        private bool failed = false;

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

        public bool IsPaused()
        {
            return !doDownload;
        }

        public bool IsDone()
        {
            return Progress == Size;
        }

        public bool HasFailed()
        {
            return failed;
        }

        public async void StartDownloadAsync()
        {
            Progress = 0;
            Size = 0;
            doDownload = true;
            await Task.Run(DownloadWorkerAsync);
            StatusChanged?.Invoke(this, new EventArgs());
        }

        public void Stop()
        {
            Pause();
        }

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
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(Uri);

            FileMode filemode;
            // For download resume
            // TODO: this does not work
            if (Progress == 0)
            {
                filemode = FileMode.CreateNew;
            }
            else
            {
                filemode = FileMode.Append;
                httpRequest.AddRange(Progress);
            }

            if (File.Exists(Filename) && Progress == 0)
            {
                //filemode = FileMode.Append;
                //var size = new FileInfo(Filename).Length;
                //httpRequest.AddRange(size);
                //Progress = size;
                File.Delete(Filename);
            }

            try
            {
                // Initialize fs writer
                using FileStream fs = new FileStream(Filename, filemode);
                using HttpWebResponse httpResponse = GetUnredirectedResponse(httpRequest);

                var contentDispositionHeader = httpResponse.Headers.Get("Content-Disposition");
                if (!string.IsNullOrEmpty(contentDispositionHeader))
                {
                    ContentDisposition contentDisposition = new ContentDisposition(contentDispositionHeader);
                    ResponseFilename = contentDisposition.FileName;
                    ReceivedResponseFilename?.Invoke(this, new ResponseFilenameEventArgs(ResponseFilename));
                }

                // Gets the stream associated with the response.
                Stream receiveStream = httpResponse.GetResponseStream();
                Size = httpResponse.ContentLength;

                byte[] read = new byte[CHUNK_SIZE];

                while (doDownload)
                {
                    var count = await receiveStream.ReadAsync(read, 0, CHUNK_SIZE);
                    if (count == 0) // End of stream, etc
                        break;
                    await fs.WriteAsync(read, 0, count);
                    Progress += count;
                    ProgressChanged?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex) // WebException, IOException
            {
                failed = true;
                throw ex;
            }
            finally
            {
                StatusChanged?.Invoke(this, new EventArgs());
            }
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
                        Console.WriteLine("Following redirect to: " + redirect);
                        var newUrl = new Uri(redirect);
                        var newFn = newUrl.Segments.Last();
                        ReceivedResponseFilename?.Invoke(this, new ResponseFilenameEventArgs(HttpUtility.UrlDecode(newFn)));
                        return GetUnredirectedResponse(WebRequest.Create(newUrl));
                    }
                }
                // Rethrow error if it's not a 300-type redirect
                throw ex;
            }
        }
    }
}
