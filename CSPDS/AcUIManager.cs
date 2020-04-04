using Autodesk.AutoCAD.ApplicationServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CSPDS
{
    public class AcUIManager
    {
        public static void FocusOnFile(FileDescriptor fd)
        {
            if (!fd.Document.IsDisposed)
            {
                if (!fd.Document.IsActive)
                {
                    DocumentCollection docMgr = Application.DocumentManager;
                    if (!docMgr.DocumentActivationEnabled)
                        docMgr.DocumentActivationEnabled = true;

                    docMgr.MdiActiveDocument = fd.Document;
                }
            }
        }
    }
}