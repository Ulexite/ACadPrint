/*
 * User: aleksey.nakoryakov
 * Date: 06.03.12
 * Time: 18:05
 */
//Microsoft
using System;
using Autodesk.AutoCAD.ApplicationServices.Core;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using CSPDS.TestWpfWindows;
using acad = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly:CommandClass(typeof(CSPDS.CommandClass))]

namespace CSPDS
{
	/// <summary>
	/// Данный класс содержит методы для непосредственной работы с AutoCAD
	/// </summary>
	public class CommandClass
	{
		[CommandMethodAttribute("bargLFM", CommandFlags.Modal|CommandFlags.NoPaperSpace)]
		[CommandMethodAttribute("LFM", CommandFlags.Modal|CommandFlags.NoPaperSpace)]
		public void LayoutFromUserInput()
		{
			CreateLayouts(new UserInputBordersBuilder());
		}
		
		[CommandMethod("bargLFBL", CommandFlags.Modal|CommandFlags.NoPaperSpace|CommandFlags.UsePickSet)]
		public void LayoutFromBlocks()
		{
			CreateLayouts(new BlocksBordersBuilder());
		}

		[CommandMethod("showTest", CommandFlags.Modal|CommandFlags.NoPaperSpace|CommandFlags.UsePickSet)]
		public void ShowTestWindow()
		{
//			var dialog = new TestWindow();
//			var result = Application.ShowModalWindow(dialog);
//			if (result.Value)
//				Application.ShowAlertDialog("Hello " + dialog.UserName);
//
			String list = "";
			int counter = 0;
			
			foreach (ObjectDescriptor objectDescriptor in AllDocumentsAllObjects.ListAllObjects())
			{
				//list += " \n" + objectDescriptor.FullName;
				counter++;
//				if (counter >= 5)
//				{
//					Application.ShowAlertDialog(list);
//					counter = 0;
//					list = "";
//				}
			}

			list = list + counter;
			Application.ShowAlertDialog(list);
		}
			
		
		private void CreateLayouts(IBordersCollectionBuilder bordersBuilder)
		{
			InitialUserInteraction initial = new InitialUserInteraction();
			initial.GetInitialData();
			if (initial.InitialDataStatus == PromptResultStatus.Cancelled)
				return;
			initial.FillPlotInfoManager();
			bordersBuilder.InitialBorderIndex = initial.Index;
			DrawingBorders[] borders = bordersBuilder.GetDrawingBorders();
			if (borders.Length == 0)
			{
				acad.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nНе выбран ни один чертёж");
				return;
			}
			LayoutCreator layoutCreator = new LayoutCreator();
			foreach (DrawingBorders border in borders)
			{
				layoutCreator.CreateLayout(border);
			}
			
			Configuration.AppConfig cfg = Configuration.AppConfig.Instance;
			// Если в конфигурации отмечено "возвращаться в модель" - то переходим в модель
			if (cfg.TilemodeOn)
				acad.SetSystemVariable("TILEMODE", 1);
			
			// Если в конфигурации отмечено "удалять неинициализированные листы" - удаляем их
			if (cfg.DeleteNonInitializedLayouts)
			{
				layoutCreator.DeleteNoninitializedLayouts();
				acad.DocumentManager.MdiActiveDocument.Editor.Regen();
			}
		}
	}
}