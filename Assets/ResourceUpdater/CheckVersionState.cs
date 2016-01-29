using System;
using System.IO;
using UnityEngine;

namespace ResUpdater
{
    public class CheckVersionState : AbstractCheckState
    {
        internal const string res_version = "res.version";
        internal const string res_version_latest = "res.version.latest";

        //0: err, >0 ok
        public int StreamVersion { get; private set; }
        public int PersistentVersion { get; private set; }
        public int LatestVersion { get; private set; }

        public int LocalVersion { get; private set; }
        

        public CheckVersionState(ResUpdater updater) : base(updater, res_version, res_version_latest)
        {
        }

        internal void Start()
        {
            StreamVersion = -1;
            PersistentVersion = -1;
            LatestVersion = -1;
            LocalVersion = -1;
            Res.useStreamVersion = false;
            Res.resourcesInStreamWhenNotUseStreamVersion.Clear();

            updater.StartCoroutine(StartRead(Loc.Stream));
            string path = Application.persistentDataPath + "/" + res_version;
            if (File.Exists(path))
            {
                updater.StartCoroutine(StartRead(Loc.Persistent));
            }
            else
            {
                PersistentVersion = 0;
            }

            updater.StartDownload(res_version + "?version=" + DateTime.Now.Ticks, res_version_latest, true);
        }

        protected override void OnDownloadError(Exception err)
        {
            updater.Reporter.DownloadLatestVersionErr(err);
            LatestVersion = 0;
            check();
        }
        
        protected override void OnWWW(Loc loc, WWW www)
        {
            int version = 0;
            if (www.error != null)
            {
                updater.Reporter.ReadVersionErr(loc, www.error, null);
            }
            else
            {
                try
                {
                    version = int.Parse(www.text);
                }
                catch (Exception e)
                {
                    updater.Reporter.ReadVersionErr(loc, null, e);
                }
            }

            switch (loc)
            {
                case Loc.Stream:
                    StreamVersion = version;
                    break;
                case Loc.Persistent:
                    PersistentVersion = version;
                    break;
                default:
                    LatestVersion = version;
                    break;
            }
            check();
        }

        private void check()
        {
            if (StreamVersion != -1 && PersistentVersion != -1 && LatestVersion != -1)
            {
                if (LatestVersion != 0)
                {
                    if (LatestVersion == StreamVersion)
                    {
                        Res.useStreamVersion = true;
                        updater.Reporter.CheckVersionDone(State.Succeed, LocalVersion, LatestVersion);
                    }
                    else
                    {
                        LocalVersion = Math.Max(StreamVersion, PersistentVersion);
                        updater.Reporter.CheckVersionDone(State.CheckMd5, LocalVersion, LatestVersion);
                        updater.CheckMd5.Start();
                    }
                }
                else
                {
                    LocalVersion = Math.Max(StreamVersion, PersistentVersion);
                    updater.Reporter.CheckVersionDone(State.Failed, LocalVersion, LatestVersion);
                }
            }
        }
    }
}