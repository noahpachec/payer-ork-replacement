namespace Ork.Helpers
{
    // Copied from Cryptide/Cryptide/Tools/Utils.cs - only GetEpochSeconds is kept.
    public static class Utils
    {
        public static long GetEpochSeconds()
        {
            return (long)(DateTimeOffset.UtcNow - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds;

        }
    }
}
