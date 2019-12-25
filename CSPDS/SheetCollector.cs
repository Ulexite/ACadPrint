﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using log4net;

namespace CSPDS
{
    public class SheetCollector
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static List<string> minimumProps = new List<string>()
        {
            "Листов",
            "Лист",
            "Формат",
            "Наименование чертежа",
            "Наименование чертежа 1",
            "Наименование чертежа 2",
            "Наименование1",
            "Наименование2",
            "Наименование3"
        };

        // Листы по файлам
        private ObservableCollection<FileDescriptor> files = new ObservableCollection<FileDescriptor>();

        private Dictionary<string, FileDescriptor> fullFileNames = new Dictionary<string, FileDescriptor>();
        public ObservableCollection<FileDescriptor> ByFiles => files;

        public void Refresh(DocumentCollection documentCollection)
        {
            _log.Debug("Refresh Started");

            var documents = Documents(documentCollection);

            RemoveClosed(documents);

            AddFrom(documents);
        }

        private void AddFrom(Dictionary<string, Document> documents)
        {
            _log.Debug("Process opened documents");
            foreach (string name in documents.Keys)
            {
                FileDescriptor fd = DescriptorFor(name, documents[name]);
                Dictionary<string, SheetDescriptor> newSheets = Sheets(fd);
                Dictionary<string, SheetDescriptor> oldSheets = OldSheets(fd);

                foreach (string uniqId in oldSheets.Keys)
                {
                    if (!newSheets.ContainsKey(uniqId))
                        fd.Remove(oldSheets[uniqId]);
                    else
                        oldSheets[uniqId].update(newSheets[uniqId].Properties);
                }

                foreach (string uniqId in newSheets.Keys)
                {
                    if (!oldSheets.ContainsKey(uniqId))
                        fd.Add(newSheets[uniqId]);
                }
            }
        }

        private Dictionary<string, SheetDescriptor> OldSheets(FileDescriptor fd)
        {
            Dictionary<string, SheetDescriptor> ret = new Dictionary<string, SheetDescriptor>();
            foreach (SheetDescriptor sheet in fd.Sheets)
            {
                ret.Add(sheet.UniqId, sheet);
            }

            return ret;
        }

        private void RemoveClosed(Dictionary<string, Document> documents)
        {
            _log.Debug("Remove closed");
            List<FileDescriptor> toRemove = new List<FileDescriptor>();
            foreach (FileDescriptor file in files)
            {
                if (!documents.ContainsKey(file.Name))
                    toRemove.Add(file);
            }

            foreach (FileDescriptor file in toRemove)
            {
                _log.DebugFormat("Remove {0}", file.Name);
                files.Remove(file);
                fullFileNames.Remove(file.Name);
            }
        }

        private Dictionary<string, Document> Documents(DocumentCollection documentCollection)
        {
            _log.Debug("Collect Documents");
            Dictionary<string, Document> documents = new Dictionary<string, Document>();
            foreach (Document document in documentCollection)
            {
                documents.Add(document.Name, document);
            }

            return documents;
        }

        private Dictionary<string, SheetDescriptor> Sheets(FileDescriptor fd)
        {
            Dictionary<string, SheetDescriptor> sheets = new Dictionary<string, SheetDescriptor>();
            foreach (SheetDescriptor sd in SheetsFromDocument(fd))
            {
                sheets.Add(sd.UniqId, sd);
            }

            return sheets;
        }

        private IEnumerable<SheetDescriptor> SheetsFromDocument(FileDescriptor document)
        {
            _log.DebugFormat("Start sheet extrating from {0}", document.Name);
            var db = document.Document.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                _log.Debug("Transaction start");
                _log.Debug("Get BlockTable");
                BlockTable blockTable = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId recordId in blockTable)
                {
                    BlockTableRecord record = (BlockTableRecord) tr.GetObject(recordId, OpenMode.ForRead);
                    _log.DebugFormat("BTRecord: {0}", record.Name);
                    
                    foreach (ObjectId objectId in record)
                    {
                        var obj = tr.GetObject(objectId, OpenMode.ForRead);

                        if (obj.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.ImpCurve"))
                        {
                            var properties = PropsFromObjectId(obj.ObjectId);
                            var excepted = minimumProps.Except(properties.Keys);
                            if(!minimumProps.Except(properties.Keys).Any())
                                yield return new SheetDescriptor(objectId, db, document, properties);
                            else
                            {
                                _log.Debug("Отсутствуют обязательные свойства");
                                foreach (var pr in excepted)
                                {
                                    _log.DebugFormat("Пропущено: {0}",pr);                                                                        
                                }
                            }
                                
                        }
                    }
                }
            }
        }

        private Dictionary<string, string> PropsFromObjectId(ObjectId objectId)
        {
            _log.Debug(String.Format("Objectid is: {0:X}", objectId));
            IntPtr pUnknown = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(objectId);
            _log.Debug(String.Format("pUnknown :{0:X}", pUnknown));
            Dictionary<string, string> ret = new Dictionary<string, string>();

            if (pUnknown != IntPtr.Zero)
            {
                using (CollectionVector properties =
                    ObjectPropertyManagerProperties.GetProperties(objectId, false, false))
                {
                    if (properties.Count() > 0)
                    {
                        using (CategoryCollectable category = properties.Item(0) as CategoryCollectable)
                        {
                            CollectionVector props = category.Properties;

                            for (var i = 0; i < props.Count(); ++i)
                            {
                                using (PropertyCollectable prop = props.Item(i) as PropertyCollectable)
                                {
                                    if (prop != null)
                                    {
                                        string value = PropertyValue(prop, prop.CollectableName, pUnknown);
                                        string name = prop.CollectableName.Trim();
                                        if (ret.ContainsKey(name))
                                            name = "+" + name;

                                        _log.Debug(String.Format("property: {0}={1}", name, value));
                                        ret.Add(name, value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        private string PropertyValue(PropertyCollectable propertyCollectable, string propName, IntPtr pUnknown)
        {
            try
            {
                object value = null;
                if (propertyCollectable != null && propertyCollectable.GetValue(pUnknown, ref value) && value != null)
                {
                    return value.ToString();
                }
            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Не смогли прочитать "), exception);
            }

            return "";
        }

        private FileDescriptor DescriptorFor(string Name, Document db)
        {
            String fullFileName = Name;
            if (fullFileNames.ContainsKey(fullFileName))
                return fullFileNames[fullFileName];

            FileDescriptor fd = new FileDescriptor(fullFileName, db);
            files.Add(fd);
            fullFileNames.Add(fullFileName, fd);
            return fd;
        }
    }

    public class FileDescriptor : INotifyPropertyChanged
    {
        private string name;
        private Document document;
        private bool isChecked;
        
        private ObservableCollection<SheetDescriptor> sheets = new ObservableCollection<SheetDescriptor>();

        public string Name => name;

        public Document Document => document;

        public ObservableCollection<SheetDescriptor> Sheets => sheets;

        public bool IsChecked
        {
            get => isChecked;
            set => isChecked = value;
        }

        public FileDescriptor(string name, Document document)
        {
            this.document = document;
            this.name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Add(SheetDescriptor sheet)
        {
            sheets.Add(sheet);
            OnPropertyChanged("Sheets");
        }

        public void Remove(SheetDescriptor sheet)
        {
            sheets.Remove(sheet);
            OnPropertyChanged("Sheets");
        }
    }

    public class SheetDescriptor : INotifyPropertyChanged
    {
        private Dictionary<string, string> properties;
        private ObjectId borderEntity;
        private Database db;
        private FileDescriptor file;
        private bool isChecked;
        private string uniqId;
        private string format;

        public ObjectId BorderEntity => borderEntity;
        public Database Db => db;
        public Dictionary<string, string> Properties => properties;
        public string UniqId => uniqId;
        public string Format => format;

        public FileDescriptor File => file;

        public bool IsChecked
        {
            get => isChecked;
            set => isChecked = value;
        }

        public SheetDescriptor(ObjectId borderEntity, Database db, FileDescriptor file,Dictionary<string, string> properties)
        {
            this.borderEntity = borderEntity;
            this.db = db;
            this.properties = properties;
            this.file = file;
            uniqId = String.Format("{0:X}", borderEntity);
            format = properties["Формат"];
        }

        public void update(Dictionary<string, string> properties)
        {
            this.properties = properties;
            OnPropertyChanged("Properties");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}