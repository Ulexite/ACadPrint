using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using log4net;

namespace CSPDS.Model
{
    public class DWGFileDescriptor
    {
        private string fullPath;
        private string name;
        private bool isOpened;
        private Database db;

        public string FullPath => fullPath;

        //[JsonIgnore]
        public string Name => name;

        public bool IsOpened => isOpened;

        public Database Db => db;

        //[JsonConstructor]
        public DWGFileDescriptor(string fullPath)
        {
            this.fullPath = fullPath;
            this.name = Path.GetFileName(fullPath);
            this.isOpened = false;
            this.db = null;
        }

        public DWGFileDescriptor(Document document)
        {
            this.fullPath = document.Name;
            this.name = Path.GetFileName(fullPath);
            this.isOpened = true;
            this.db = document.Database;
        }

        public void Open(Document document)
        {
            if (isOpened)
                return;
            this.isOpened = true;
            this.db = document.Database;
        }
    }
    public class DWGFileManager
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, DWGFileDescriptor> knownFiles = new Dictionary<string, DWGFileDescriptor>();
        private Dictionary<string, DWGFileDescriptor> openedFiles = new Dictionary<string, DWGFileDescriptor>();

        public void FromOpened(DocumentCollection documentCollection)
        {
            try
            {
                foreach (Document document in documentCollection)
                {
                    DWGFileDescriptor file = new DWGFileDescriptor(document);
                    knownFiles.Add(file.FullPath,file);
                    openedFiles.Add(file.FullPath,file);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }

            
        }
    }
}