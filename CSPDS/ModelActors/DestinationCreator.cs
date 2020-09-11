using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using CSPDS.Model;

namespace CSPDS.Actors
{
    public class DestinationCreator
    {
         public Destination Create(Document document, string key, Dictionary<string, object> properties)
         {
             string name = key;
             string fileName = document.Name;
             string id = key+fileName;
             HashSet<string> formats = new HashSet<string>();
             
             
             properties.Add("fileName", fileName);
             properties.Add("name", name);
             //TODO:smart shortener;
             properties.Add("shortFileName", Path.GetFileName(fileName));
             
             return new Destination(properties, name, fileName, formats, id, key, document.Database);
         }
    }
}