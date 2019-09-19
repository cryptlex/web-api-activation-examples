using System;

using Cryptlex.Services;

namespace Cryptlex
{
    public class LexSystemInfo
    {
        public static string GetFingerPrint()
        {
            // TODO
            return LexEncryptionService.Sha256("DummyFingerprint");
        }

        public static string GetOsName()
        {
            return "android";
        }

        public static string GetOsVersion()
        {
            // TODO
            return "8.0";
        }

        public static string GetVmName()
        {
            return String.Empty;
        }

        public static string GetHostname()
        {
            return System.Environment.MachineName;
        }

        public static string GetUser()
        {
            return System.Environment.UserName;
        }
    }
}