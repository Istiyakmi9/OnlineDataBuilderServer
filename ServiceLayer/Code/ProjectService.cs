using BottomhalfCore.DatabaseLayer.Common.Code;
using Microsoft.AspNetCore.Hosting;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ApplicationConstants;

namespace ServiceLayer.Code
{
    public class ProjectService : IProjectService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IHostingEnvironment _hostingEnvironment;
        public ProjectService(IDb db, CurrentSession currentSession, FileLocationDetail fileLocationDetail, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _currentSession = currentSession;
            _fileLocationDetail = fileLocationDetail;
            _hostingEnvironment = hostingEnvironment;
        }
        public string AddWikiService(WikiDetail project)
        {
            if (project.ProjectId <= 0)
                throw new HiringBellException("Invalid project id");

            Project projectDetail = _db.Get<Project>("sp_project_detail_getby_id", new { project.ProjectId });
            if (projectDetail == null)
                throw new HiringBellException("Invalid project selected");

            var folderPath = Path.Combine(_fileLocationDetail.DocumentFolder, _fileLocationDetail.CompanyFiles, "project_document");
            if (!Directory.Exists(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath)))
                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.ContentRootPath, folderPath));
            string filename = projectDetail.ProjectName.Replace(" ", "") + ".txt";
            var filepath = Path.Combine(folderPath, filename);
            if (File.Exists(filepath))
                File.Delete(filepath);

            var txt = new StreamWriter(filepath);
            txt.Write(project.SectionDescription);
            txt.Close();

            projectDetail.PageIndexDetail = "[]";
            projectDetail.KeywordDetail = "[]";
            projectDetail.DocumentPath = filepath;
            projectDetail.AdminId = _currentSession.CurrentUserDetail.UserId;

            var result = _db.Execute<Project>("sp_wiki_detail_upd", projectDetail, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("fail to insert or update");

            return result;
        }

        public async Task<string> AddUpdateProjectDetailService(Project projectDetail)
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
                project.PreviousProjectManagerId = projectDetail.ProjectManagerId == project.ProjectManagerId ? 0 : project.ProjectManagerId;
                project.ProjectManagerId = projectDetail.ProjectManagerId;
                project.TeamMemberIds = project.TeamMemberIds == null ? "[]"
                                        : project.TeamMemberIds;
                project.PreviousArchitectId = projectDetail.ArchitectId == project.ArchitectId ? 0 : project.ArchitectId;
                project.ArchitectId = projectDetail.ArchitectId;
                project.IsClientProject = projectDetail.IsClientProject;
                project.ClientId = projectDetail.ClientId;
                project.HomePageUrl = projectDetail.HomePageUrl;
                project.ProjectStartedOn = projectDetail.ProjectStartedOn;
                project.ProjectEndedOn = projectDetail.ProjectEndedOn;
                project.CompanyId = projectDetail.CompanyId;
                project.DocumentPath = projectDetail.DocumentPath;
            }

            projectDetail.AdminId = _currentSession.CurrentUserDetail.UserId;


            var data = (from n in projectDetail.TeamMembers
                        select new ProjectMemberDetail
                        {
                            ProjectMemberDetailId = n.ProjectMemberDetailId > 0 ? n.ProjectMemberDetailId : 0,
                            ProjectId = Convert.ToInt32(DbProcedure.getParentKey(projectDetail.ProjectId)),
                            EmployeeId = n.EmployeeId,
                            DesignationId = n.DesignationId,
                            FullName = n.FullName,
                            Email = n.Email,
                            IsActive = n.IsActive,
                            AssignedOn = DateTime.UtcNow,
                            LastDateOnProject = null
                        }).ToList<object>();

            var result = await _db.BatchInsetUpdate(
                "sp_project_detail_insupd",
                project,
                data
            );


            // var result = _db.Execute<Project>("sp_project_detail_insupd", project, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to Insert or Update");

            return result;
        }

        public Project GetAllWikiService(long ProjectId)
        {
            var result = _db.Get<Project>("sp_project_detail_getby_id", new { ProjectId });
            if (File.Exists(result.DocumentPath))
            {
                var txt = File.ReadAllText(result.DocumentPath);
                result.DocumentationDetail = txt;
            }
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

        public DataSet GetProjectPageDetailService(long ProjectId)
        {
            var result = _db.FetchDataSet("sp_project_get_page_data", new { ProjectId = ProjectId });

            if (result.Tables.Count != 3)
                throw HiringBellException.ThrowBadRequest("Project detail not found. Please contact to admin.");

            result.Tables[0].TableName = "Project";
            result.Tables[1].TableName = "Clients";
            result.Tables[2].TableName = "Employees";
            return result;
        }
    }
}
