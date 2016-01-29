using System;
using System.Collections.Generic;

namespace ResUpdater
{
    public enum Loc
    {
        Stream,
        Persistent,
        Latest
    }

    public enum State
    {
        CheckVersion,
        CheckMd5,
        DownloadRes,
        Succeed,
        Failed
    }

    public interface Reporter
    {
        void DownloadLatestVersionErr(Exception err);
        
        void ReadVersionErr(Loc loc, string wwwErr, Exception parseErr);

        void CheckVersionDone(State nextState, int localVersion, int latestVersion); //CheckMd5, Succeed, Failed


        void DownloadLatestMd5Err(Exception err);

        void ReadMd5Err(Loc loc, string wwwwErr, Exception parseErr);

        void CheckMd5Done(State nextState, Dictionary<string, CheckMd5State.Info> downloadList); //DownloadRes, Succeed, Failed


        void DownloadOneResComplete(Exception err, string fn, CheckMd5State.Info info);

        void DownloadResDone(State nextState, int errCount); //Succeed, Failed
    }

}