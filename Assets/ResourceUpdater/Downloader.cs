using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

namespace ResUpdater
{
    internal class Downloader : IDisposable
    {
        private class Info
        {
            public string fn;
            public bool isHighPriority;
            public int tryCnt;
            public string url;
        }

        public delegate void DownloadDoneFunc(Exception err, string fn);


        private readonly string[] hosts; //前面的网络更好，带宽更贵，所以meta从前往后试，data从后往前试；只试一轮
        private readonly int thread; //同时download的文件数

        private readonly string outputPath;
        private readonly DownloadDoneFunc DownloadDone;

        private readonly WebClient webclient;

        private readonly Dictionary<string, Info> downloadings = new Dictionary<string, Info>();
        private readonly Dictionary<string, Info> pendings = new Dictionary<string, Info>();
        
        public Downloader(string[] hosts, int thread, string outputPath, DownloadDoneFunc downloadDone)
        {
            this.hosts = hosts;
            this.thread = thread;
            this.outputPath = outputPath;
            DownloadDone = downloadDone;
            webclient = new WebClient();
            webclient.DownloadFileCompleted += OnDownloadFileCompleted;
        }


        public void StartDownload(string url, string fn, bool isHighPriority = false)
        {
            if (downloadings.ContainsKey(fn))
                return;

            var info = new Info
            {
                url = url,
                fn = fn,
                isHighPriority = isHighPriority,
                tryCnt = 0
            };

            if (downloadings.Count < thread)
            {
                downloadings.Add(fn, info);
                StartDownload(info);
            }
            else
            {
                if (pendings.ContainsKey(fn))
                    pendings[fn] = info;
                else
                    pendings.Add(fn, info);
            }
        }

        private void StartDownload(Info info)
        {
            var idx = info.isHighPriority
                ? info.tryCnt
                : hosts.Length - 1 - info.tryCnt;
            webclient.DownloadFileAsync(new Uri(hosts[idx] + info.url), outputPath + info.fn, info.fn);
            info.tryCnt++;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string fn = (string) e.UserState;
            if (e.Error == null)
            {
                oneDone(fn, null);
            }
            else
            {
                var info = downloadings[fn];
                if (info.tryCnt < hosts.Length)
                {
                    info.tryCnt++;
                    StartDownload(info);
                }
                else
                {
                    oneDone(fn, e.Error);
                }
            }
        }

        private void oneDone(string fn, Exception err)
        {
            downloadings.Remove(fn);
            DownloadDone(err, fn);

            if (pendings.Count > 0)
            {
                var e = pendings.GetEnumerator();
                e.MoveNext();
                var pfn = e.Current.Key;
                var pinfo = e.Current.Value;
                pendings.Remove(pfn);

                if (downloadings.ContainsKey(pfn))
                    downloadings[pfn] = pinfo;
                else
                    downloadings.Add(pfn, pinfo);
            }
        }

        public void Dispose()
        {
            webclient.Dispose();
        }
    }
}