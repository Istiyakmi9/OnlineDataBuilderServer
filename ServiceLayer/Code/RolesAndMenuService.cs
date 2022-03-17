using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Data;
using System.Linq;

namespace ServiceLayer.Code
{
    public class RolesAndMenuService : IRolesAndMenuService
    {
        private readonly IDb _db;
        public RolesAndMenuService(IDb db)
        {
            _db = db;
        }
        public string AddUpdatePermission(RolesAndMenu rolesAndMenus)
        {
            int result = 0;
            var permissionMenu = (from n in rolesAndMenus.Menu
                                  select new RoleAccessibilityMapping
                                  {
                                      RoleAccessibilityMappingId = -1,
                                      AccessLevelId = rolesAndMenus.AccessLevelId,
                                      AccessCode = n.AccessCode,
                                      AccessibilityId = n.Permission
                                  }).ToList<RoleAccessibilityMapping>();

            DataSet ds = Converter.ToDataSet<RoleAccessibilityMapping>(permissionMenu);
            result = _db.BatchInsert("sp_role_accessibility_mapping_InsUpd", ds, false);
            if (permissionMenu.Count <= 0)
            {
                return "Fail to inserted or updated";
            }

            return "Inserted or updated successfully";
        }

        public DataSet GetsRolesandMenu(int accessLevelId)
        {
            DataSet result = null;
            if (accessLevelId > 0)
            {
                DbParam[] dbParam = new DbParam[]
                {
                    new DbParam(accessLevelId, typeof(int), "_accesslevelId")
                };

                result = _db.GetDataset("sp_RolesAndMenu_GetAll", dbParam);
            }
            return result;
        }

        public DataSet GetRoles()
        {
            DataSet result = _db.GetDataset("sp_AccessLevel_Sel");
            return result;
        }

        public DataSet AddRole (AddRole addRole)
        {
            string accessLevelId = "-1";
            DbParam[] dbParams = new DbParam[]
            {
                new DbParam(addRole.RoleName, typeof(string), "_RoleName"),
                new DbParam(addRole.AccessCodeDefination, typeof(string), "_AccessCodeDefination"),
                new DbParam(accessLevelId, typeof(string), "_AccessLevelId")
            };

            string message = string.Empty;
            var result = _db.GetDataset("sp_AccessLevel_InsUpd", dbParams, true, ref message);
            return result;
        }
    }
}
