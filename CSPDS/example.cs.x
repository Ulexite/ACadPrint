[CommandMethod("ADDXDATA")]

static public void AddXdata()

{

    Document doc =

        Application.DocumentManager.MdiActiveDocument;

    Database db =

        doc.Database;

    Transaction tr =

        db.TransactionManager.StartTransaction();

    using(tr)

    {

        Editor ed =

            Application.DocumentManager.MdiActiveDocument.Editor;

        // Prompt the user to select an entity

        PromptEntityResult ers =

           ed.GetEntity("Pick entity ");

        // Open the entity

        Entity ent =

          (Entity)tr.GetObject(ers.ObjectId,

                                        OpenMode.ForWrite);

        // Get the registered application names table

        RegAppTable regTable =

           (RegAppTable)tr.GetObject(db.RegAppTableId,

                                        OpenMode.ForRead);

 

        if(!regTable.Has("ADS"))

        {

            regTable.UpgradeOpen();

 

   // Add the application names that would be used to add Xdata

            RegAppTableRecord app =

                    new RegAppTableRecord();

            app.Name = "ADS";

            regTable.Add(app);

            tr.AddNewlyCreatedDBObject(app, true);

        }

        // Append the Xdata to the entity - two different

        // applications added.

        ent.XData = new ResultBuffer(new TypedValue(1001, "ADS"),

                                new TypedValue(1070, 100));

        tr.Commit();

    }

}

 

[CommandMethod("REMXDATA")]

static public void RemoveXdata() // This method can have any name

{

    Document doc =

        Application.DocumentManager.MdiActiveDocument;

    Database db =

        doc.Database;

    Transaction tr =

        db.TransactionManager.StartTransaction();

    using (tr)

    {

        Editor ed =

            Application.DocumentManager.MdiActiveDocument.Editor;

        try

        {

            // Prompt the user to select an entity

            PromptEntityResult ers =

                ed.GetEntity("Pick entity ");

            // Open the selected entity

            Entity ent =

                (Entity)tr.GetObject(ers.ObjectId,

                            OpenMode.ForRead);

 

            ResultBuffer buffer =

                ent.GetXDataForApplication("ADS");

            // This call would ensure that the

            //Xdata of the entity associated with ADSK application

            //name only would be removed

            if (buffer != null)

            {

                ent.UpgradeOpen();

                ent.XData =

                    new ResultBuffer(new TypedValue(1001, "ADS"));

                buffer.Dispose();

            }

            tr.Commit();

        }

        catch

        {

            tr.Abort();

        }

    }

}