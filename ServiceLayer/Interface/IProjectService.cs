using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IProjectService
    {
        string AddProjectService (WikiDetail project);
        List<Project> GetAllProjectService();
    }
}
