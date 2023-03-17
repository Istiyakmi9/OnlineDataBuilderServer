using ModalLayer.Modal;
using ModalLayer.Modal.Profile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IProjectService
    {
        string AddWikiService(WikiDetail project);
        Project GetAllWikiService(long ProjectId);
        string AddUpdateProjectDetailService(Project projectDetail);
        List<Project> GetAllProjectDeatilService(FilterModel filterModel);
        DataSet GetProjectPageDetailService(long ProjectId);
    }
}
