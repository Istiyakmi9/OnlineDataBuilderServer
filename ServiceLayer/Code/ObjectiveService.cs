using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
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

        public List<ObjectiveDetail> ObjectiveInsertUpdateService(ObjectiveDetail objectiveDetail)
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
                objective.Tag = objectiveDetail.Tag;
                objective.ProgressMeassureType = objectiveDetail.ProgressMeassureType;
                objective.MetricUnits = objectiveDetail.MetricUnits;
                objective.ProgressCalculatedAs = objectiveDetail.ProgressCalculatedAs;
                objective.TimeFrameStart = objectiveDetail.TimeFrameStart;
                objective.TimeFrmaeEnd = objectiveDetail.TimeFrmaeEnd;
                objective.ObjectiveType = objectiveDetail.ObjectiveType;
                objective.Description = objectiveDetail.Description;
            }
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

            } else if (objectiveDetail.ProgressMeassureType == 2)
            {
                if (objectiveDetail.StartValue < 0)
                    throw HiringBellException.ThrowBadRequest("Invalid start value entered");

                if (objectiveDetail.TargetValue < 0)
                    throw HiringBellException.ThrowBadRequest("Invalid target value entered");

                if (objectiveDetail.MetricUnits <= 0)
                    throw HiringBellException.ThrowBadRequest("Invalid matric unit selected");

                if (objectiveDetail.ProgressCalculatedAs <= 0)
                    throw HiringBellException.ThrowBadRequest("Invalid progress calculation type selected");

            } else
            {
                objectiveDetail.StartValue = 0;
                objectiveDetail.TargetValue = 0;
                objectiveDetail.MetricUnits = 0;
                objectiveDetail.ProgressCalculatedAs = 0;
            }

        }

        public List<ObjectiveDetail> GetPerformanceObjectiveService(FilterModel filterModel)
        {
            if (filterModel.CompanyId > 0)
                filterModel.SearchString += $" and l.CompanyId = {filterModel.CompanyId} ";

            var result = _db.GetList<ObjectiveDetail>("sp_performance_objective_getby_filter", filterModel);
            return result;
        }
    }
}
