using BottomhalfCore.CacheManagement.Caching;
using BottomhalfCore.CacheManagement.CachingInterface;
using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.FactoryContext;
using ModalLayer.Modal;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Data;

namespace ServiceLayer.Code
{
    public class ManageUserCommentService : IManageUserCommentService<ManageUserCommentService>
    {
        private readonly IDb db;
        private readonly ICacheManager<CacheManager> cacheManager;
        public ManageUserCommentService(IDb db)
        {
            this.db = db;
            this.cacheManager = CacheManager.GetInstance();
        }
        public string PostUserCommentService(UserComments userComments)
        {
            string Result = string.Empty;
            DbParam[] param = new DbParam[]
            {
                new DbParam(userComments.COMMENTSUID, typeof(System.Guid), "@COMMENTSUID"),
                new DbParam(userComments.UserUid, typeof(System.Guid), "@USERID"),
                new DbParam(userComments.USERNAME, typeof(System.String), "@USERNAME"),
                new DbParam(userComments.EMAILID, typeof(System.String), "@EMAILID"),
                new DbParam(userComments.TITLE, typeof(System.String), "@TITLE"),
                new DbParam(userComments.Company, typeof(System.String), "@COMPANY"),
                new DbParam(userComments.COMMENTS, typeof(System.String), "@COMMENTS")
            };
            DataSet ResultSet = db.GetDataset("SP_UserComments_INSUPD", param);
            if (ResultSet != null && ResultSet.Tables.Count > 0 && ResultSet.Tables[0].Rows.Count > 0)
                Result = JsonConvert.SerializeObject(ResultSet);
            return Result;
        }

        public DataSet GetCommentsService(string EmailId)
        {
            string Result = string.Empty;
            if (string.IsNullOrEmpty(EmailId))
                EmailId = "";
            DbParam[] param = new DbParam[]
            {
                new DbParam(EmailId, typeof(System.String), "@EMAILID")
            };
            DataSet ResultSet = db.GetDataset("SP_UserComments_Get", param);
            if (ResultSet != null && ResultSet.Tables.Count > 0 && ResultSet.Tables[0].Rows.Count > 0)
            {
                ResultSet.Tables[0].TableName = "Comments";
            }
            return ResultSet;
        }
    }
}
