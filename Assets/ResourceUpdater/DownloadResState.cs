using System;
using System.Collections.Generic;

namespace ResUpdater
{
    public class DownloadResState
    {
        private readonly ResUpdater updater;
        public Dictionary<string, CheckMd5State.Info> DownloadList { get; private set; }
        public int OkCount { get; private set; }
        public int ErrCount { get; private set; }

        public DownloadResState(ResUpdater resUpdater)
        {
            updater = resUpdater;
        }

        public void Start(Dictionary<string, CheckMd5State.Info> needDownloads)
        {
            DownloadList = needDownloads;
            foreach (var kv in DownloadList)
            {
                updater.StartDownload(kv.Key + "?version=" + kv.Value.Md5, kv.Key, false);
            }
        }

        internal void OnDownloadCompleted(Exception err, string fn)
        {
            updater.Reporter.DownloadOneResComplete(err, fn, DownloadList[fn]);
            if (err != null)
                OkCount++;
            else
                ErrCount++;

            if ((OkCount + ErrCount) == DownloadList.Count)
            {
                if (ErrCount == 0)
                    updater.Reporter.DownloadResDone(State.Succeed, 0);
                else
                    updater.Reporter.DownloadResDone(State.Failed, ErrCount);
            }
        }
    }
}