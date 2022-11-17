using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ITaxRegimeService
    {
        TaxRegimeDesc AddUpdateTaxRegimeDescService(TaxRegimeDesc taxRegimeDesc);
        dynamic GetAllRegimeService();
        TaxAgeGroup AddUpdateAgeGroupService(TaxAgeGroup taxAgeGroup);
        Task<dynamic> AddUpdateTaxRegimeService(List<TaxRegime> taxRegimes);
    }
}
