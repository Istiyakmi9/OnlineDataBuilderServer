using BottomhalfCore.DatabaseLayer.Common.Code;
using ServiceLayer.Interface;

namespace ServiceLayer.Code
{
    public class CompanyService : ICompanyService
    {
        private readonly IDb _db;

        public CompanyService(IDb db)
        {
            _db = db;
        }
    }
}
