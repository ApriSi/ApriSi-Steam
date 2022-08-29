using System.IO;

namespace ApriSiSteam
{
    public static class Token
    {
        public static string GetKey() => File.ReadAllText("token.txt");
    }
}
