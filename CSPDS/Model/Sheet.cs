using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Annotations;
using CSPDS.Utils;

namespace CSPDS.Model
{
    /*
     * Рамка.
     * */
    public class Sheet : IPropertiesHolder
    {
        //Все свойства объекта
        private readonly Dictionary<string, object> properties;

        //Формат
        private readonly string format;

        //Полный путь к файлу
        private readonly string fileName;

        //идентификатор:
        private readonly string native_id;

        //идентификатор объекта в БД
        private readonly string id;

        [CanBeNull] private string destination_id;
        //TODO:presettedOrientationHere!

        //Ac-objects:
        private Extents2d bounds; //Может быть пересчитано
        private readonly ObjectId acObjId;
        private readonly Document document;

        public Sheet(Dictionary<string, object> properties, string format, string fileName, string nativeId, string id,
            Extents2d bounds, ObjectId acObjId, Document document)
        {
            this.properties = properties;
            this.format = format;
            this.fileName = fileName;
            this.native_id = nativeId;
            this.id = id;
            this.bounds = bounds;
            this.acObjId = acObjId;
            this.document = document;
        }

        public string NativeId => native_id;

        public string Id => id;
        public string FileName => properties["fileName"].ToString();

        public string this[string key]
        {
            get
            {
                if (properties.ContainsKey(key))
                    return properties[key].ToString();
                return ""; //??
            }
        }

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        [CanBeNull]
        public string DestinationId
        {
            get => destination_id;
            set => destination_id = value;
        }

        public string Format => format;

        public Document Document => document;

        public Extents2d Bounds
        {
            get => bounds;
            set => bounds = value;
        }
    }
}