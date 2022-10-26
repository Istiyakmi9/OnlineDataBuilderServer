using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Code
{
    public class ProjectService : IProjectService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;

        public ProjectService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }
        public string AddWikiService(WikiDetail project)
        {
            if (project.ProjectId <= 0)
                throw new HiringBellException("Invalid project id");

            Project projectDetail = _db.Get<Project>("sp_project_detail_getby_id", new { project.ProjectId });
            if (projectDetail == null)
                throw new HiringBellException("Invalid project selected");
            else
            {
                projectDetail.PageIndexDetail = "[]";
                projectDetail.KeywordDetail = "[]";
                projectDetail.DocumentationDetail = JsonConvert.SerializeObject(project);
                projectDetail.AdminId = _currentSession.CurrentUserDetail.UserId;
            }
            var result = _db.Execute<Project>("sp_wiki_detail_upd", projectDetail, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("fail to insert or update");
            return result;
        }

        public string AddUpdateProjectDetailService(Project projectDetail)
        {
            this.ProjectDetailValidtion(projectDetail);
            Project project = _db.Get<Project>("sp_project_detail_getby_id", new { projectDetail.ProjectId });
            if (project == null)
            {
                project = projectDetail;
                project.TeamMemberIds = project.TeamMemberIds == null ? "[]" 
                                        : JsonConvert.SerializeObject(project.TeamMemberIds);
                project.PageIndexDetail = "[]";
                project.KeywordDetail = "[]";
                project.DocumentationDetail = "[]";
            }
            else
            {
                project.ProjectName = projectDetail.ProjectName;
                project.ProjectDescription = projectDetail.ProjectDescription;
                project.ProjectManagerId = projectDetail.ProjectManagerId;
                project.TeamMemberIds = project.TeamMemberIds == null ? "[]"
                                        : project.TeamMemberIds;
                project.ArchitectId = projectDetail.ArchitectId;
                project.IsClientProject = projectDetail.IsClientProject;
                project.ClientId = projectDetail.ClientId;
                project.HomePageUrl = projectDetail.HomePageUrl;
                project.ProjectStartedOn = projectDetail.ProjectStartedOn;
                project.ProjectEndedOn = projectDetail.ProjectEndedOn;
                project.CompanyId = projectDetail.CompanyId;
            }
            projectDetail.AdminId = _currentSession.CurrentUserDetail.UserId;

            var result = _db.Execute<Project>("sp_project_detail_insupd", project, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to Insert or Update");
            return result;
        }

        public Project GetAllWikiService(long ProjectId)
        {
            var result = _db.Get<Project>("sp_project_detail_getby_id", new { ProjectId });
            return result;
        }

        public List<Project> GetAllProjectDeatilService(FilterModel filterModel)
        {
            var result = _db.GetList<Project>("sp_project_detail_getall", new
            {
                filterModel.SearchString,
                filterModel.SortBy,
                filterModel.PageIndex,
                filterModel.PageSize
            });
            if (result == null)
                throw new HiringBellException("Unable to load projext list data.");

            return result;
        }

        private void ProjectDetailValidtion(Project project)
        {
            if (string.IsNullOrEmpty(project.ProjectName))
                throw new HiringBellException("Project name is null or empty");

            if (project.CompanyId <= 0)
                throw new HiringBellException("Compnay is not selected. Please selete your company.");

        }
    }
}
