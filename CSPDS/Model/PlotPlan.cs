using System.Collections.Generic;

namespace CSPDS.Model
{
    public class PlotPlan
    {
        private readonly ModuleModel model;
        private readonly Dictionary<string, PlotTask> taskList = new Dictionary<string, PlotTask>();

        public PlotPlan(ModuleModel model)
        {
            this.model = model;
        }

        public void Add(Sheet sheet)
        {
            taskList.Add(sheet.Id, new PlotTask(sheet, model.GetDestinationForSheet(sheet)));
        }

        public IEnumerable<PlotTask> TaskList => taskList.Values;
    }
}