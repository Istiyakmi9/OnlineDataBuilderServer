using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IDb _db;

        public ServiceRequestService(IDb db)
        {
            _db = db;
        }

        public Task<List<ServiceRequest>> GetServiceRequestService(FilterModel filter)
        {
            List<ServiceRequest> ServiceRequests = _db.GetList<ServiceRequest>("sp_service_request_filter", new
            {
                filter.SearchString,
                filter.SortBy,
                filter.PageIndex,
                filter.PageSize
            });

            return Task.FromResult(ServiceRequests);
        }
    }
}
