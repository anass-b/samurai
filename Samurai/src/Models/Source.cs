namespace Samurai.Models
{
    /// <summary>
    /// Represents a Git repository or URL
    /// </summary>
    public class Source
    {
        public const string GitTypeName = "git";
        public const string ArchiveTypeName = "archive";
        public const string FileTypeName = "file";

        public string Type { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// For <see cref="Type"/>=archive only, it tells that the extracted
        /// archive will have a root folder after extraction and that we
        /// should avoid it
        /// </summary>
        /// <value><c>true</c> if archive has root dir; otherwise, <c>false</c>.</value>
        public bool ArchiveHasRootDir { get; set; }
    }
}
