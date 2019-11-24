using System.Collections.Generic;

namespace CSPDS
{
    public class BorderEnumerator
    {
        public List<FileDescriptor> fromAllFiles()
        {
            List<FileDescriptor> files = new List<FileDescriptor>();
            var fd = new FileDescriptor("aaa");
            fd.Borders.Add(new BorderDescriptor("AAAAA", "A3x3"));
            fd.Borders.Add(new BorderDescriptor("sdfsdd dhfnfdkl gkljs dfkls dfljsd fklsd flkjsd flskdjf sdkljf skdlfj ", "A4"));
            fd.Borders.Add(new BorderDescriptor("sdfsdd fklsd flkjsd flskdjf sdkljf skdlfj ", "A0"));
            files.Add(fd);
            return files;
        }
    }

    public class FileDescriptor
    {
        private string name;
        private List<BorderDescriptor> borders = new List<BorderDescriptor>();

        public string Name => name;

        public List<BorderDescriptor> Borders => borders;

        public FileDescriptor(string name)
        {
            this.name = name;
        }
    }

    public class BorderDescriptor
    {
        private string name;
        private string format;
        
        public string Name => name;

        public string Format => format;

        public BorderDescriptor(string name, string format)
        {
            this.name = name;
            this.format = format;
        }
    }
}