using System.Collections.Generic;

namespace ResUpdater
{
    public static class Res //给lua用
    {
        public static bool useStreamVersion;
        public static readonly HashSet<string> resourcesInStreamWhenNotUseStreamVersion = new HashSet<string>();
    }
}
