
namespace IDB.Analyzer.Common.Helper
{
    public static class StringHelper
    {
        public static string GetReplacedFilename(this string origFilename)
        {
            var diffLocalFilestorePath = DataHandler.Instance.Settings["DifferentLoadLocalFilestorePath"];
            var filestorePath = DataHandler.Instance.Settings["FilestorePath"];
            return (string.IsNullOrEmpty(diffLocalFilestorePath) || diffLocalFilestorePath == filestorePath)
                ? origFilename
                : origFilename.Replace(diffLocalFilestorePath, filestorePath);
        }
    }
}
