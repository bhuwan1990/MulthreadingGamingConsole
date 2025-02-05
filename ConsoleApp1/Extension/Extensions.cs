namespace ConsoleApp1.Extension
{
    internal static class Extensions
    {
        public static string ToShortGuid(this Guid guid)
        {
            var shortGuid = guid.ToString();
            return shortGuid.Substring((int)(shortGuid?.LastIndexOf('-') + 1));
        }
    }
}
