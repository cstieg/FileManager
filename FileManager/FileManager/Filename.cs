namespace FileManager
{
    /// <summary>
    /// Represents a filename, allowing it to be broken down into its constituent components
    /// </summary>
    public class Filename
    {
        /// <summary>
        /// Constructor for Filename
        /// </summary>
        /// <param name="filename">Filename to be represented</param>
        public Filename(string filename)
        {
            FileName = filename;
        }

        public string FileName { get; set; }

        public int Length
        {
            get
            {
                return FileName.Length;
            }
        }

        public int ExtensionLength
        {
            get
            {
                // example: abc.gif  
                //   Length = 7
                //   LastIndexOf('.') = 3
                //   ExtensionLength = 4 ('.gif')
                return Length - FileName.LastIndexOf('.');
            }
        }

        public string Extension
        {
            get
            {
                return FileName.Substring(Length - ExtensionLength);
            }
            set
            {
                FileName = BaseName + value;
            }
        }

        public string BaseName
        {
            get
            {
                return FileName.Substring(0, Length - ExtensionLength);
            }
            set
            {
                FileName = value + Extension;
            }
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}