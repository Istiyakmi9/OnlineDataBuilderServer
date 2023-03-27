using DocumentFormat.OpenXml.Office2010.ExcelAc;
using ModalLayer.Modal;
using System.Collections.Generic;

namespace ServiceLayer.Interface
{
    public interface IObjectiveService
    {
        List<ObjectiveDetail> ObjectiveInsertUpdateService(ObjectiveDetail objectiveDetail);
        List<ObjectiveDetail> GetPerformanceObjectiveService(FilterModel filterModel);
    }
}
