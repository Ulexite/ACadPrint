
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;

namespace CSPDS
{
    public class BorderEnumerator
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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
                _log.Debug(String.Format("for document: {0}",name));

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
                        _log.Debug(String.Format("for objectId: {0:X}",objectId));

                        var obj = tr.GetObject(objectId, OpenMode.ForRead);
                        
                        if (obj.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.ImpCurve"))
                        {
                            _log.Debug("object is ImpCurve");

                            //if mcd
                            yield return DescriptorForBorder(objectId);
                        }
                    }
                }
            }
        }

        private BorderDescriptor DescriptorForBorder(ObjectId objectId)
        {
            _log.Debug("DescriptorForBorder");

            IntPtr pUnknown = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(objectId);
            if (pUnknown != IntPtr.Zero)
            {
                _log.Debug(String.Format("pUnknown :{0:X}",pUnknown));
                using (CollectionVector properties =
                    ObjectPropertyManagerProperties.GetProperties(objectId, false, false))
                {
                    if (properties.Count() > 0)
                    {
                        using (CategoryCollectable category = properties.Item(0) as CategoryCollectable)
                        {
                            CollectionVector props = category.Properties;
                            Dictionary<String, PropertyCollectable> propByName =
                                new Dictionary<string, PropertyCollectable>();
                            for (int i = 0; i < props.Count(); ++i)
                            {
                                using (PropertyCollectable prop = props.Item(i) as PropertyCollectable)
                                {
                                    if (prop != null)
                                    {
                                        _log.Debug(String.Format("property: {0} {1} {2}",prop.Name, prop.CollectableName, prop.DISP.ToString()));

                                        propByName.Add(prop.CollectableName.Trim(), prop);
                                    }
                                }
                            }

                            return DescriptorFromProperties(propByName, pUnknown);
                        }
                    }
                }
            }

            return ForError("Что-то пошло не так:формат не получилось вытащить");
        }

        private BorderDescriptor DescriptorFromProperties(Dictionary<string, PropertyCollectable> propByName,
            IntPtr pUnknown)
        {
            string sheetCountProp = "Листов";
            string sheetNumberProp = "Лист";
            string formatProp = "Формат";
            string lastName0 = "Наименование чертежа";
            string lastName1 = "Наименование чертежа 1";
            string lastName2 = "Наименование чертежа 2";
            string firstName0 = "Наименование1";
            string firstName1 = "Наименование2";
            string firstName2 = "Наименование3";

            string format = GetPropValue(propByName, formatProp, pUnknown);

            string lastName = ComposeName(GetPropValue(propByName, lastName0, pUnknown),
                GetPropValue(propByName, lastName1, pUnknown),
                GetPropValue(propByName, lastName2, pUnknown));

            string firstName = ComposeName(GetPropValue(propByName, firstName0, pUnknown),
                GetPropValue(propByName, firstName1, pUnknown),
                GetPropValue(propByName, firstName2, pUnknown));

            int sheetCount = 0;
            Int32.TryParse(GetPropValue(propByName, sheetCountProp, pUnknown), out sheetCount);
            int sheetNumber = 0;
            Int32.TryParse(GetPropValue(propByName, sheetNumberProp, pUnknown), out sheetNumber);

            return new BorderDescriptor(firstName + " : " + lastName, format, sheetNumber, sheetCount);
        }

        private string ComposeName(string sub0, string sub1, string sub2)
        {
            return sub0.Trim() + " " +
                   sub1.Trim() + " " +
                   sub2.Trim() + " ";
        }

        private string GetPropValue(Dictionary<string, PropertyCollectable> propByName, string propName, IntPtr pUnknown)
        {
            PropertyCollectable propertyCollectable = propByName[propName]; 

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
                return "Не нашел " + propertyCollectable is null ? "null":propertyCollectable.ToString();
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