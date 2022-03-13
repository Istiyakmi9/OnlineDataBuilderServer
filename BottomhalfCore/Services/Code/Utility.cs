namespace BottomhalfCore.Services.Code
{
    public static class Utility
    {
        public static string GetExtension(string SavedExtension, string FindExtension)
        {
            if (SavedExtension.IndexOf(FindExtension) == -1)
                return null;
            else
                return FindExtension;
        }
    }
}
