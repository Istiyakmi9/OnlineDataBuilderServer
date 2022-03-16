using ModalLayer.Modal;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Interface
{
    public interface IRolesAndMenuService
    {
        string AddUpdatePermission(RolesAndMenu rolesAndMenus);
        DataSet GetsRolesandMenu(int accessLevelId);
        DataSet GetRoles();
        DataSet AddRole(AddRole addRole);
    }
}
