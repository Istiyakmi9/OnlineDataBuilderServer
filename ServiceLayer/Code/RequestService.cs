using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System.Data;

namespace ServiceLayer.Code
{
    public class RequestService : IRequestService
    {
        private readonly IDb _db;

        public RequestService(IDb db)
        {
            _db = db;
        }

        public DataSet FetchPendingRequests(int employeeId)
        {
            DataSet result = null;

            if (employeeId < 0)
                throw new HiringBellException("Invalid employee id.");

            if (employeeId == 0)
                employeeId = 0;

            DbParam[] param = new DbParam[]
            {
                new DbParam(employeeId, typeof(long), "_ManagerId"),
                new DbParam(0, typeof(int), "_StatusId")
            };

            result = _db.GetDataset("sp_attendance_get_pending_requests", param);
            return result;
        }
    }
}
