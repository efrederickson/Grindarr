using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;

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
                // Initialize download
                using HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

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
    }
}
