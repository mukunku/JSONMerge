
namespace JSONMerge
{
    public struct FileDetailsArg
    {
        public string FilePath { get; set; }
        public string Delimiter { get; set; }

        public FileDetailsArg(string filePath, string delimiter) : this()
        {
            this.FilePath = filePath;
            this.Delimiter = delimiter;
        }
    }
}
