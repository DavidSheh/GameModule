using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ResUpdater
{
    public abstract class AbstractCheckState
    {
        protected readonly ResUpdater updater;
        private readonly string _name;
        private readonly string _latestName;

        protected AbstractCheckState(ResUpdater updater, string name, string latestName)
        {
            this.updater = updater;
            _name = name;
            _latestName = latestName;
        }
        
        protected abstract void OnDownloadError(Exception err);
        protected abstract void OnWWW(Loc loc, WWW www);

        internal IEnumerator StartRead(Loc loc)
        {
            string url;
            switch (loc)
            {
                case Loc.Stream:
                    url = Application.streamingAssetsPath + "/" + _name;
                    break;
                case Loc.Persistent:
                    url = "file://" + Application.persistentDataPath + "/" + _name;
                    break;
                default:
                    url = "file://" + Application.persistentDataPath + "/" + _latestName;
                    break;
            }

            WWW www = new WWW(url);
            yield return www;
            OnWWW(loc, www);
        }

        internal void OnDownloadCompleted(Exception err)
        {
            if (err == null)
                updater.StartCoroutine(StartRead(Loc.Latest));
            else
                OnDownloadError(err);
        }
    }
}