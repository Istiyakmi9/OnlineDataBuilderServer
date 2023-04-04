using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;

namespace ServiceLayer.Code
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;

        public ObjectiveService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public dynamic ObjectiveInsertUpdateService(ObjectiveDetail objectiveDetail)
        {
            validateObjectiveDetail(objectiveDetail);
            var objective = _db.Get<ObjectiveDetail>("sp_performance_objective_get_by_id", new { ObjectiveId = objectiveDetail.ObjectiveId });
            if (objective == null)
                objective = objectiveDetail;
            else
            {
                objective.Objective = objectiveDetail.Objective;
                objective.StartValue = objectiveDetail.StartValue;
                objective.TargetValue = objectiveDetail.TargetValue;
                objective.ObjSeeType = objectiveDetail.ObjSeeType;
                objective.IsIncludeReview = objectiveDetail.IsIncludeReview;
                objective.ProgressMeassureType = objectiveDetail.ProgressMeassureType;
                objective.TimeFrameStart = objectiveDetail.TimeFrameStart;
                objective.TimeFrmaeEnd = objectiveDetail.TimeFrmaeEnd;
                objective.ObjectiveType = objectiveDetail.ObjectiveType;
                objective.Description = objectiveDetail.Description;
                objective.TagRole = objectiveDetail.TagRole;
            }

            if (objective.TagRole.Count > 0)
                objective.Tag = JsonConvert.SerializeObject(objective.TagRole);
            else
                objective.Tag = "[]";

            objective.AdminId = _currentSession.CurrentUserDetail.UserId;

            var result = _db.Execute<ObjectiveDetail>("sp_performance_objective_insupd", objective, true);
            if (string.IsNullOrEmpty(result))
                throw HiringBellException.ThrowBadRequest("Fail to insert/update objective deatils");

            var filterModel = new FilterModel();
            filterModel.CompanyId = objective.CompanyId;
            return this.GetPerformanceObjectiveService(filterModel);
        }

        private void validateObjectiveDetail(ObjectiveDetail objectiveDetail)
        {
            if (objectiveDetail.CompanyId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid company selected. Please login again");

            if (string.IsNullOrEmpty(objectiveDetail.Objective))
                throw HiringBellException.ThrowBadRequest("Objective is null or empty");

            if (objectiveDetail.TimeFrameStart == null)
                throw HiringBellException.ThrowBadRequest("Invalid time frame start date selected");

            if (objectiveDetail.TimeFrmaeEnd == null)
                throw HiringBellException.ThrowBadRequest("Invalid time frame end date selected");

            if (string.IsNullOrEmpty(objectiveDetail.ObjectiveType))
                throw HiringBellException.ThrowBadRequest("Objective type is invalid");

            if (objectiveDetail.ProgressMeassureType <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid progress measured type selected");

            if (objectiveDetail.ProgressMeassureType == 1)
            {
                if (objectiveDetail.StartValue < 0)
                    throw HiringBellException.ThrowBadRequest("Invalid start value entered");

                if (objectiveDetail.TargetValue < 0)
                    throw HiringBellException.ThrowBadRequest("Invalid target value entered");
            }
            else
            {
                objectiveDetail.StartValue = 0;
                objectiveDetail.TargetValue = 0;
            }
        }

        public dynamic GetPerformanceObjectiveService(FilterModel filterModel)
        {
            if (filterModel.CompanyId > 0)
                filterModel.SearchString += $" and CompanyId = {filterModel.CompanyId} ";

            (List<ObjectiveDetail> objectiveDetails, List<EmployeeRole> empRoles) = _db.GetList<ObjectiveDetail, EmployeeRole>("sp_performance_objective_getby_filter", filterModel);
            objectiveDetails.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.Tag) && x.Tag != "[]")
                    x.TagRole = JsonConvert.DeserializeObject<List<int>>(x.Tag);
            });
            return new {ObjectiveDetails = objectiveDetails, EmployeeRoles = empRoles};
        }

        public List<ObjectiveDetail> GetEmployeeObjectiveService(int designationId, int companyId)
        {
            var empObjective = new List<ObjectiveDetail>();
            if (designationId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid designation selected. Please login again");

            if (companyId <= 0)
                throw HiringBellException.ThrowBadRequest("Invalid company selected. Please login again");

            var objectives = _db.GetList<ObjectiveDetail>("sp_emp_performance_getby_compid", new { CompanyId = companyId });
            if (objectives != null && objectives.Count > 0)
            {
                objectives.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x.Tag) && x.Tag != "[]")
                    {
                        x.TagRole = JsonConvert.DeserializeObject<List<int>>(x.Tag);
                        var value = x.TagRole.Find(i => i == designationId);
                        if (value > 0)
                            empObjective.Add(x);
                    }
                });
            }
            return empObjective;
        }
    }
}
