using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal.PropertyInspector;

namespace CSPDS.Actors
{
    /**
     * Умееть извлекать свойства из com-объектов
     */
    public class PropertiesExractor
    {
        public Dictionary<string, object> PropertiesFrom(ObjectId objectId)
        {
            IntPtr pUnknown = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(objectId);
            Dictionary<string, object> ret = new Dictionary<string, object>();
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
                                        object value = PropertyValue(prop, prop.CollectableName, pUnknown);
                                        string name = prop.CollectableName.Trim();
                                        if (ret.ContainsKey(name))
                                            name = "+" + name;

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

        private object PropertyValue(PropertyCollectable propertyCollectable, string propName, IntPtr pUnknown)
        {
            try
            {
                object value = null;
                if (propertyCollectable != null && propertyCollectable.GetValue(pUnknown, ref value) && value != null)
                {
                    return value;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return "";
        }
    }
}