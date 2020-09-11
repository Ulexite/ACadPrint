using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Utils;

namespace CSPDS.Model
{
    /*
     * На самом деле это описание всех параметров для печати
     * (из Autodesk.AutoCAD.DatabaseServices.PlotSettings)     
     */
    public class Destination : IPropertiesHolder
    {
        //Все свойства объекта
        private readonly Dictionary<string, object> properties;

        private readonly string name;

        //Полный путь к файлу
        private readonly string fileName;

        //Форматы
        private readonly HashSet<string> formats;

        //идентификатор объекта в БД
        private readonly string id;

        //Ac-objects:
        private Database db;
        private readonly string dict_key;

        public Destination(Dictionary<string, object> properties, string name, string fileName, HashSet<string> formats,
            string id, string dictKey, Database db )
        {
            this.properties = properties;
            this.name = name;
            this.fileName = fileName;
            this.formats = formats;
            this.id = id;
            this.db = db;
            dict_key = dictKey;
        }

        public string FileName => fileName;

        public string Id => id;

        public Database Db => db;
 
        public string DictKey => dict_key;

        public string this[string key]
        {
            get
            {
                if (properties.ContainsKey(key))
                    return $"{properties[key]}";
                return ""; //??
            }
        }

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        public void RemoveFormat(string format)
        {
            formats.Remove(format);
        }

        public void AddFormat(string format)
        {
            formats.Add(format);
        }

        private IEnumerable<string> Formats => formats;

        public string Name => name;
    }
}