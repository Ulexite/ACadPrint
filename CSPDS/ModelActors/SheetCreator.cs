using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Model;

namespace CSPDS.Actors
{
    /**
     * По-сути умеет только одно - из объекта Ac сделать CSPDS.Model.Sheet
     * Определяется как замкнутая кривая (Autodesk.AutoCAD.DatabaseServices.ImpCurve),
     * имее свойство "Формат"
     * Нативный идентификатор при этом - совокупность свойств:
     * "Листов"
     * "Лист"
     * "Наименование чертежа <0-2>"
     * "Наименование<1-3>"
     */
    public class SheetCreator
    {
        //TODO: Конфигурация
        private readonly string formatFieldName = "Формат";

        private readonly List<string> nativeIdFields = new List<string>()
        {
            "Формат",
            "Листов",
            "Лист",
            "Наименование чертежа",
            "Наименование чертежа 1",
            "Наименование чертежа 2",
            "Наименование1",
            "Наименование2",
            "Наименование3"
        };

        private readonly List<string> nameFields = new List<string>()
        {
            "Наименование чертежа",
            "Наименование чертежа 1",
            "Наименование чертежа 2",
        };

        private readonly BoundsCalculator bCalc;

        public SheetCreator(BoundsCalculator bCalc)
        {
            this.bCalc = bCalc;
        }

        public Sheet Create(Document document, ObjectId objId, Dictionary<string, object> properties)
        {
            string format = properties[formatFieldName].ToString();
            string nativeId = "";
            string name = "";
            string dbId = String.Format("{0:X}", objId) + document.Name;

            properties.Add("fileName", document.Name);
            
            //TODO:smart shortener;
            
            properties.Add("shortFileName", Path.GetFileName(document.Name));
            properties.Add("dbId", dbId);

            foreach (string field in nativeIdFields)
            {
                if (properties.ContainsKey(field))
                    nativeId = nativeId + properties[field];
            }

            properties.Add("nativeId", nativeId);

            foreach (string field in nameFields)
            {
                if (properties.ContainsKey(field))
                    name = name + " " + properties[field];
            }
            
            properties.Add("name", name);

            return new Sheet(properties, format, document.Name, nativeId, dbId,
                bCalc.BoundsFor(objId, document.Database), objId, document);
        }
    }
}