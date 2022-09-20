using System.Runtime.CompilerServices;

namespace StudentInfo.WebApi.Helpers
{
    public static class StringHelper
    {
        public static string EmptyIfNull(this string? s)
        {
            return string.IsNullOrWhiteSpace(s) ? string.Empty : s;
        }
    }
}
