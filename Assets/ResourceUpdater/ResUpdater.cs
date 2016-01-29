using System;
using System.Collections;
using UnityEngine;

namespace ResUpdater
{
    public delegate Coroutine StartCoroutineFunc(IEnumerator routine);

    public class ResUpdater : IDisposable
    {
        private readonly Downloader downloader;

        internal readonly Reporter Reporter;
        internal readonly StartCoroutineFunc StartCoroutine;

        public readonly CheckVersionState CheckVersion;
        public readonly CheckMd5State CheckMd5;
        public readonly DownloadResState DownloadRes;

        public ResUpdater(string[] hosts, int thread, Reporter reporter, StartCoroutineFunc startCoroutine)
        {
            downloader = new Downloader(hosts, thread, Application.persistentDataPath, DownloadDone);
            Reporter = reporter;
            StartCoroutine = startCoroutine;

            CheckVersion = new CheckVersionState(this);
            CheckMd5 = new CheckMd5State(this);
            DownloadRes = new DownloadResState(this);
        }

        public void Start()
        {
            CheckVersion.Start();
        }

        public void Dispose()
        {
            downloader.Dispose();
        }

        internal void StartDownload(string url, string fn, bool isHighPriority)
        {
            downloader.StartDownload(url, fn, isHighPriority);
        }

        private void DownloadDone(Exception err, string fn)
        {
            switch (fn)
            {
                case CheckVersionState.res_version_latest:
                    CheckVersion.OnDownloadCompleted(err);
                    break;
                case CheckMd5State.res_md5_latest:
                    CheckMd5.OnDownloadCompleted(err);
                    break;
                default:
                    DownloadRes.OnDownloadCompleted(err, fn);
                    break;
            }
        }
    }
}