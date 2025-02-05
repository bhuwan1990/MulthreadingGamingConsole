namespace MaindashBoard
{
    internal static class Extensions
    {

        public static string GetShortGuid(this Guid guid)
        {
            var shortGuid = guid.ToString();
            return shortGuid.Substring((int)(shortGuid?.LastIndexOf('-') + 1));
        }
    }
}
