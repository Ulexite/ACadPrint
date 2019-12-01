using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace CSPDS
{
    public class BorderEnumerator
    {
        private ObservableCollection<FileDescriptor> files = new ObservableCollection<FileDescriptor>();
        private Dictionary<string, FileDescriptor> fullFileNames = new Dictionary<string, FileDescriptor>();

        public ObservableCollection<FileDescriptor> refreshBorderList(DocumentCollection documents)
        {
            fullFileNames.Clear();
            files.Clear();
            foreach (Document document in Application.DocumentManager)
            {
                string name = document.Name;
                if (!document.IsNamedDrawing)
                    name = "*" + name;
                FileDescriptor descriptor = GetDescriptorFor(name);
                foreach (BorderDescriptor border in BordersInDocument(document))
                {
                    descriptor.Borders.Add(border);
                }
            }

            return files;
        }

        private IEnumerable<BorderDescriptor> BordersInDocument(Document document)
        {
            var db = document.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId  recordId in blockTable)
                {
                    BlockTableRecord record = (BlockTableRecord) tr.GetObject(recordId, OpenMode.ForRead);

                    foreach (ObjectId objectId in record)
                    {
                        var obj = tr.get
                    }
                }
            }
        }


        private FileDescriptor GetDescriptorFor(string fullFileName)
        {
            if (fullFileNames.ContainsKey(fullFileName))
                return fullFileNames[fullFileName];

            FileDescriptor fd = new FileDescriptor(fullFileName);
            files.Add(fd);
            fullFileNames.Add(fullFileName, fd);
            return fd;
        }
    }

    public class FileDescriptor
    {
        private string name;
        private ObservableCollection<BorderDescriptor> borders = new ObservableCollection<BorderDescriptor>();

        public string Name => name;

        public ObservableCollection<BorderDescriptor> Borders => borders;

        public FileDescriptor(string name)
        {
            this.name = name;
        }
    }

    public class BorderDescriptor
    {
        //Название
        private string name;

        //Номер листа
        private int sheetNumber = 1;

        public int SheetNumber => sheetNumber;

        //Формат 
        private string format;

        //Название набора настроек плоттера

        //Последний раз печаталось.


        public string Name => name;

        public string Format => format;

        public BorderDescriptor(string name, string format)
        {
            this.name = name;
            this.format = format;
        }
    }
}