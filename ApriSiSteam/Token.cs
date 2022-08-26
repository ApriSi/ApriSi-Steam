using System.IO;

namespace ApriSiSteam
{
    public static class Token
    {
        // Wish there was a better way, there probably is but not sure how
        public static string GetKey() => File.ReadAllText("token.txt");
    }
}
