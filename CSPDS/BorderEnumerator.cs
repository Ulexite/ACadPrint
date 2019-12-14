using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using log4net;

namespace CSPDS
{
    public class BorderEnumerator
    {
        private const string sheetCountProp = "Листов";
        private const string sheetNumberProp = "Лист";
        private const string formatProp = "Формат";
        private const string lastName0 = "Наименование чертежа";
        private const string lastName1 = "Наименование чертежа 1";
        private const string lastName2 = "Наименование чертежа 2";
        private const string firstName0 = "Наименование1";
        private const string firstName1 = "Наименование2";
        private const string firstName2 = "Наименование3";

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<FileDescriptor> files = new ObservableCollection<FileDescriptor>();
        private Dictionary<string, FileDescriptor> fullFileNames = new Dictionary<string, FileDescriptor>();

        public ObservableCollection<FileDescriptor> refreshBorderList(DocumentCollection documents)
        {
            _log.Debug("started");
            fullFileNames.Clear();
            files.Clear();

            foreach (Document document in Application.DocumentManager)
            {
                string name = document.Name;
                _log.Debug(String.Format("for document: {0}", name));

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
                foreach (ObjectId recordId in blockTable)
                {
                    BlockTableRecord record = (BlockTableRecord) tr.GetObject(recordId, OpenMode.ForRead);

                    foreach (ObjectId objectId in record)
                    {
                        _log.Debug(String.Format("for objectId: {0:X}", objectId));

                        var obj = tr.GetObject(objectId, OpenMode.ForRead);

                        if (obj.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.ImpCurve"))
                        {
                            _log.Debug("object is ImpCurve");

                            //if mcd
                            yield return DescriptorForBorder(obj.ObjectId);
                        }
                    }
                }
            }
        }

        private BorderDescriptor DescriptorForBorder(ObjectId objectId)
        {
            _log.Debug("DescriptorForBorder");
            _log.Debug(String.Format("Objectid is: {0:X}", objectId));
            Dictionary<string, string> propByName =
                new Dictionary<string, string>()
                {
                    {formatProp, ""},
                    {sheetCountProp, ""},
                    {sheetNumberProp, ""},
                    {firstName0, ""},
                    {firstName1, ""},
                    {firstName2, ""},
                    {lastName0, ""},
                    {lastName1, ""},
                    {lastName2, ""}
                };

            IntPtr pUnknown = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(objectId);
            _log.Debug(String.Format("pUnknown :{0:X}", pUnknown));
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
                                        _log.Debug(String.Format("property: {0} {1}", prop.Name,
                                            prop.CollectableName));

                                        if (propByName.ContainsKey(prop.CollectableName.Trim()))
                                            propByName[prop.CollectableName.Trim()] =
                                                GetPropValue(prop, prop.CollectableName, pUnknown);
                                    }
                                }
                            }

                            return DescriptorFromProperties(propByName);
                        }
                    }
                }
            }

            return ForError("Что-то пошло не так:формат не получилось вытащить");
        }

        private BorderDescriptor DescriptorFromProperties(Dictionary<string, string> propByName)
        {
            string format = propByName[formatProp];

            string lastName = ComposeName(propByName[lastName0], propByName[lastName1], propByName[lastName2]);
            string firstName = ComposeName(propByName[firstName0], propByName[firstName1], propByName[firstName2]);


            int sheetCount = 0;
            Int32.TryParse(propByName[sheetCountProp], out sheetCount);
            int sheetNumber = 0;
            Int32.TryParse(propByName[sheetNumberProp], out sheetNumber);

            return new BorderDescriptor(firstName + " : " + lastName, format, sheetNumber, sheetCount);
        }

        private string ComposeName(string sub0, string sub1, string sub2)
        {
            return sub0.Trim() + " " +
                   sub1.Trim() + " " +
                   sub2.Trim() + " ";
        }

        private string GetPropValue(PropertyCollectable propertyCollectable, string propName,
            IntPtr pUnknown)
        {
            try
            {
                _log.Debug(string.Format("Читаем свойство {0} - {1}", propName, propertyCollectable));
                object value = null;
                if (propertyCollectable != null && propertyCollectable.GetValue(pUnknown, ref value) && value != null)
                {
                    _log.Debug(string.Format("Прочитали: {0}", value));
                    return value.ToString();
                }
            }
            catch (Exception exception)
            {
                _log.Error(string.Format("Не смогли прочитать "), exception);
            }

            return "";
        }

        private BorderDescriptor ForError(string errorMsg)
        {
            return new BorderDescriptor(errorMsg);
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
        //Формат 
        private string format;

        //Название
        private string name;

        //Номер листа
        private int sheetNumber;

        //Всего листов
        private int sheetCount;

        //Название набора настроек плоттера

        //Последний раз печаталось.

        public string Name => name;

        public string Format => format;
        public int SheetCount => sheetCount;
        public int SheetNumber => sheetNumber;

        public BorderDescriptor(string format, string name, int sheetNumber, int sheetCount)
        {
            this.format = format;
            this.name = name;
            this.sheetNumber = sheetNumber;
            this.sheetCount = sheetCount;
        }

        public BorderDescriptor(string name)
        {
            this.name = name;
            this.format = "";
            this.sheetCount = 0;
            this.sheetNumber = 0;
        }
    }
}