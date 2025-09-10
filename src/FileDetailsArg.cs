
namespace JSONMerge
{
    public struct FileDetailsArg
    {
        public string FilePath { get; set; }

        public FileDetailsArg(string filePath) : this()
        {
            this.FilePath = filePath;
        }
    }
}
