using System;
using System.IO;
using System.Windows.Input;

namespace ApriSiSteam
{
    public static class Token
    {
        private static string _key { get; set; }

        public static string GetKey() => _key;

        internal static void SetKey(string password)
        {
            _key = password;
        }
    }
}
