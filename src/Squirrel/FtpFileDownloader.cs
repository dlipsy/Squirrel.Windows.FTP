using Splat;
using Squirrel;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;


namespace Squirrel
{
    public class FtpFileDownloader : IFileDownloader, IEnableLogger
    {
        private string username;
        private string password;

        public FtpFileDownloader(string user = null, string pwd = null)
        {
            username = user ?? Utility.FtpUsername;
            Utility.FtpPassword = username;
            password = pwd ?? Utility.FtpPassword;
            Utility.FtpPassword = password;
        }

        public Task DownloadFile(string url, string targetFile, Action<int> progress)
        {
            url = HttpUtility.UrlDecode(url);
            if (url.Contains("?"))
            {
                url = url.Substring(0, url.IndexOf("?"));
            }

            this.Log().Info("Downloading  ftp file: " + (url));

            progress = progress ?? (s => { });

            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(username, password);

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var file = new FileStream(targetFile, FileMode.Create))
                {
                    using (var reader = response.GetResponseStream())
                    {
                        while (true)
                        {
                            byte[] buff = new byte[2048];
                            var read = reader.Read(buff, 0, buff.Length);
                            if (read == 0)
                            {
                                break;
                            }
                            file.Write(buff, 0, read);
                        }
                    }
                    file.Close();
                }
            }

            this.Log().Info("Downloading file ftp finished: " + (url));

            return Task.Delay(0);
        }

        public Task<byte[]> DownloadUrl(string url)
        {
            url = HttpUtility.UrlDecode(url);
            if (url.Contains("?"))
            {
                url = url.Substring(0, url.IndexOf("?"));
            }
            this.Log().Info("Downloading ftp url: " + (url));

            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(username, password);
            var mem = new MemoryStream();

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var reader = response.GetResponseStream())
                {
                    while (true)
                    {
                        var buff = new byte[2048];
                        var read = reader.Read(buff, 0, buff.Length);
                        if (read == 0)
                        {
                            break;
                        }
                        mem.Write(buff, 0, read);
                    }
                }
            }
            this.Log().Info("Downloading url ftp finished: " + url);

            return Task.FromResult(mem.ToArray());
        }
    }
}
