using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Code
{
    public class ProjectService: IProjectService
    {
        private readonly IDb _db;
        public ProjectService(IDb db)
        {
            _db = db;
        }
        public string AddProjectService(WikiDetail project)
        {
            Project projectDetail = new Project();
            projectDetail.ProjectId = project.ProjectId;
            projectDetail.ProjectName = project.ProjectName;
            projectDetail.ProjectDescription = "Accounts related project";
            projectDetail.ProjectManagerId = 1;
            projectDetail.TeamMemberIds = "[]";
            projectDetail.ArchitectId = 1;
            projectDetail.IsClientProject = true;
            projectDetail.ClientId = 1;
            projectDetail.HomePageUrl = "wwww";
            projectDetail.PageIndexDetail = "[]";
            projectDetail.KeywordDetail = "[]";
            projectDetail.DocumentationDetail = JsonConvert.SerializeObject(project);
            var result = _db.Execute<Project>("sp_project_detail_insupd", projectDetail, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("fail to insert or update");
            return result;
        }
        public List<Project> GetAllProjectService()
        {
            var result = _db.GetList<Project>("sp_getAll_Project");
            return result;
        }
    }
}
