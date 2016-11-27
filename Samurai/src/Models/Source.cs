namespace Samurai.Models
{
    public class Source
    {
        public const string GitTypeName = "git";
        public const string ArchiveTypeName = "archive";
        public const string FileTypeName = "file";

        public string Type { get; set; }
        public string Url { get; set; }
    }
}
