using System;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;

namespace CSPDS.Actors
{
    public class BoundsCalculator
    {
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
        static extern int acedTrans(
            double[] point,
            IntPtr fromRb,
            IntPtr toRb,
            int disp,
            double[] result
        );

        //TODO: configurable
        private readonly int fromCode = 1;
        private readonly int toCode = 2;
        
        public Extents2d BoundsFor(ObjectId acObjId, Database database)
        {
            using (Transaction tr = database.TransactionManager.StartTransaction())
            {
                Drawable curve = tr.GetObject(acObjId, OpenMode.ForRead);
                Extents3d bounds = curve.Bounds.Value;
                Point3d first = bounds.MinPoint;
                Point3d second = bounds.MaxPoint;
                ResultBuffer rbFrom = new ResultBuffer(new TypedValue(5003, fromCode));
                ResultBuffer rbTo = new ResultBuffer(new TypedValue(5003, toCode));
                double[] firres = {0, 0, 0};
                double[] secres = {0, 0, 0};
                acedTrans(first.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, firres);
                acedTrans(second.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, secres);
                var ret = new Extents2d(
                    firres[0],
                    firres[1],
                    secres[0],
                    secres[1]
                );
                return ret;
            }
        }
    }
}