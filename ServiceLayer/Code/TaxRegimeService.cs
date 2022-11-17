using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class TaxRegimeService: ITaxRegimeService
    {
        private readonly IDb _db;

        public TaxRegimeService(IDb db)
        {
            _db = db;
        }
        public TaxRegimeDesc AddUpdateTaxRegimeDescService(TaxRegimeDesc taxRegimeDesc)
        {
            if (string.IsNullOrEmpty(taxRegimeDesc.RegimeName))
                throw new HiringBellException("Regime Name is null or empty");

            if (string.IsNullOrEmpty(taxRegimeDesc.Description))
                throw new HiringBellException("Description is null or empty");


            TaxRegimeDesc oldTaxRegimeDesc = _db.Get<TaxRegimeDesc>("sp_tax_regime_desc_getbyId", new { TaxRegimeDescId = taxRegimeDesc.TaxRegimeDescId });
            if (oldTaxRegimeDesc != null)
            {
                oldTaxRegimeDesc.Description = taxRegimeDesc.Description;
                oldTaxRegimeDesc.RegimeName = taxRegimeDesc.RegimeName;
            } else
            {
                oldTaxRegimeDesc = taxRegimeDesc;
            }
            var result = _db.Execute<TaxRegimeDesc>("sp_tax_regime_desc_insupd", oldTaxRegimeDesc, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update tax regime description");
            taxRegimeDesc.TaxRegimeDescId = Convert.ToInt32(result);
            return taxRegimeDesc;
        }
        public dynamic GetAllRegimeService()
        {
            var resultSet = _db.GetDataset("sp_tax_regime_desc_getall");
            if (resultSet != null && resultSet.Tables.Count > 3)
                throw new HiringBellException("Fail to get tax regime");

            DataTable taxRegimeDesc = null;
            DataTable taxRegime = null;
            DataTable ageGroup = null;

            if (resultSet.Tables[0].Rows.Count > 0)
                taxRegimeDesc = resultSet.Tables[0];

            if (resultSet.Tables[1].Rows.Count > 0)
                taxRegime = resultSet.Tables[1];

            if (resultSet.Tables[2].Rows.Count > 0)
                ageGroup = resultSet.Tables[2];

            return new {taxRegimeDesc, taxRegime, ageGroup };
        }

        public TaxAgeGroup AddUpdateAgeGroupService(TaxAgeGroup taxAgeGroup)
        {
            if (taxAgeGroup.StartAgeGroup <= 0)
                throw new HiringBellException("Start age must be greater than zero");

            if (taxAgeGroup.EndAgeGroup <= 0)
                throw new HiringBellException("End ange must be greater than zero");

            if (taxAgeGroup.StartAgeGroup >= taxAgeGroup.EndAgeGroup)
                throw new HiringBellException("Please select a valid age group");

            TaxAgeGroup oldAgeGroup = _db.Get<TaxAgeGroup>("sp_tax_age_group_getby_id", new { AgeGroupId = taxAgeGroup.AgeGroupId });
            if (oldAgeGroup != null)
            {
                oldAgeGroup.StartAgeGroup = taxAgeGroup.StartAgeGroup;
                oldAgeGroup.EndAgeGroup = taxAgeGroup.EndAgeGroup;
            }
            else
            {
                oldAgeGroup = taxAgeGroup;
            }
            var result = _db.Execute<TaxAgeGroup>("sp_tax_age_group_insupd", oldAgeGroup, true);
            if (string.IsNullOrEmpty(result))
                throw new HiringBellException("Fail to insert or update tax age group");
            taxAgeGroup.AgeGroupId = Convert.ToInt32(result);
            return taxAgeGroup;
        }
        public async Task<dynamic> AddUpdateTaxRegimeService(List<TaxRegime> taxRegimes)
        {
            ValidateTaxRegime(taxRegimes);
            List<TaxRegime> oldTaxRegimes = _db.GetList<TaxRegime>("sp_tax_regime_getall");
            foreach (var taxRegime in taxRegimes)
            {
                if (taxRegime.TaxRegimeId > 0)
                {
                    var oldRegime = oldTaxRegimes.Find(x => x.TaxRegimeId == taxRegime.TaxRegimeId);
                    if (oldRegime != null)
                    {
                        oldRegime.RegimeDescId = taxRegime.RegimeDescId;
                        oldRegime.StartAgeGroup = taxRegime.StartAgeGroup;
                        oldRegime.EndAgeGroup = taxRegime.EndAgeGroup;
                        oldRegime.MinTaxSlab = taxRegime.MinTaxSlab;
                        oldRegime.MaxTaxSlab = taxRegime.MaxTaxSlab;
                        oldRegime.TaxRatePercentage = taxRegime.TaxRatePercentage;
                        oldRegime.TaxAmount = taxRegime.TaxAmount;
                    } 

                }
                else
                {
                    oldTaxRegimes.Add(taxRegime);
                }
            }
            var regime = (from n in oldTaxRegimes
                          select new
                          {
                              n.TaxRegimeId,
                              n.RegimeDescId,
                              n.StartAgeGroup,
                              n.EndAgeGroup,
                              n.MinTaxSlab,
                              n.MaxTaxSlab,
                              n.TaxRatePercentage,
                              n.TaxAmount
                          });
            var table = Converter.ToDataTable(regime);
            _db.StartTransaction(IsolationLevel.ReadUncommitted);
            var status = await _db.BatchInsertUpdateAsync("sp_tax_regime_insupd", table, true);
            _db.Commit();
            return this.GetAllRegimeService();
        }
        private void ValidateTaxRegime(List<TaxRegime> taxRegimes) 
        {
            taxRegimes = taxRegimes.OrderBy(x => x.RegimeIndex).ToList();
            decimal taxAmount = 0;
            decimal minTaxSlab = 0;
            int i = 0;
            while (i < taxRegimes.Count)
            {
                if (taxRegimes[i].RegimeDescId <= 0)
                    throw new HiringBellException("Please select a vlid Tax regime");

                if (taxRegimes[i].StartAgeGroup >= taxRegimes[i].EndAgeGroup)
                    throw new HiringBellException("Invalid age group selected");

                if (taxRegimes[i].MinTaxSlab > taxRegimes[i].MinTaxSlab)
                    throw new HiringBellException("Invalid taxslab enter");
                
                if (i > 0)
                {
                    if (taxRegimes[i].MinTaxSlab - taxRegimes[i - 1].MaxTaxSlab != 1)
                        throw new HiringBellException("Please enter a valid taxslab range");

                    
                }
                if (taxRegimes[i].MinTaxSlab > 0)
                    minTaxSlab = taxRegimes[i].MinTaxSlab - 1;
                else
                    minTaxSlab = taxRegimes[i].MinTaxSlab;

                taxAmount = taxAmount + Math.Abs(((taxRegimes[i].MaxTaxSlab - minTaxSlab) * taxRegimes[i].TaxRatePercentage) / 100);
                if (taxRegimes[i].TaxAmount != taxAmount)
                    throw new HiringBellException("Tax amount calculation is mismatch");
                i++;
            }
        }
    }
}
