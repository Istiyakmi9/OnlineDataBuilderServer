using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.Interface
{
    public interface IProjectService
    {
        string AddProjectService (Project project);
        List<Project> GetAllProjectService();
    }
}
