using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.Services.Code;
using ModalLayer.Modal;
using ModalLayer.Modal.Accounts;
using Newtonsoft.Json;
using ServiceLayer.Interface;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer.Code
{
    public class SettingService : ISettingService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        private readonly FileLocationDetail _fileLocationDetail;
        private readonly IFileService _fileService;
        public SettingService(IDb db, CurrentSession currentSession, FileLocationDetail fileLocationDetail, IFileService fileService)
        {
            _db = db;
            _currentSession = currentSession;
            _fileLocationDetail = fileLocationDetail;
            _fileService = fileService;
        }

        public string AddUpdateComponentService(SalaryComponents salaryComponents)
        {
            salaryComponents = _db.Get<SalaryComponents>("");
            return null;
        }

        public dynamic GetSalaryComponentService()
        {
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get", false);
            PfEsiSetting pfEsiSettings = _db.Get<PfEsiSetting>("sp_pf_esi_setting_get");
            var value = new { SalaryComponent = salaryComponents, PfEsiSettings = pfEsiSettings };
            return value;
        }

        public string PfEsiSetting(SalaryComponents PfSetting, SalaryComponents EsiSetting, PfEsiSetting PfesiSetting)
        {
            string value = string.Empty;
            List<SalaryComponents> salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get", false);
            var pfsetting = salaryComponents.Find(x => x.ComponentId == PfSetting.ComponentId);
            var esisetting = salaryComponents.Find(x => x.ComponentId == EsiSetting.ComponentId);

            if (pfsetting == null)
                pfsetting = PfSetting;
            else
            {
                pfsetting.CalculateInPercentage = PfSetting.CalculateInPercentage;
                pfsetting.EmployerContribution = PfSetting.EmployerContribution;
                pfsetting.IsActive = PfSetting.IsActive;
                pfsetting.MaxLimit = PfSetting.MaxLimit;
                pfsetting.DeclaredValue = PfSetting.DeclaredValue;
                pfsetting.IncludeInPayslip = PfSetting.IncludeInPayslip;
            }

            if (esisetting == null)
                esisetting = EsiSetting;
            else
            {
                esisetting.MaxLimit = EsiSetting.MaxLimit;
                esisetting.EmployerContribution = EsiSetting.EmployerContribution;
                esisetting.IsActive = EsiSetting.IsActive;
                esisetting.MaxLimit = EsiSetting.MaxLimit;
                esisetting.DeclaredValue = EsiSetting.DeclaredValue;
                esisetting.EmployeeContribution = EsiSetting.EmployeeContribution;
                esisetting.IncludeInPayslip = EsiSetting.IncludeInPayslip;
            }

            value = _db.Execute<SalaryComponents>("sp_salary_components_insupd", new
            {
                pfsetting.ComponentId,
                pfsetting.ComponentFullName,
                pfsetting.CalculateInPercentage,
                pfsetting.EmployeeContribution,
                pfsetting.IsActive,
                pfsetting.TaxExempt,
                pfsetting.ComponentTypeId,
                pfsetting.IncludeInPayslip,
                pfsetting.ComponentDescription,
                pfsetting.MaxLimit,
                pfsetting.ComponentCatagoryId,
                pfsetting.DeclaredValue,
                pfsetting.AcceptedAmount,
                pfsetting.RejectedAmount,
                pfsetting.UploadedFileIds,
                pfsetting.EmployerContribution,
                pfsetting.IsOpted,
                pfsetting.PercentageValue,
                pfsetting.Formula,
                pfsetting.IsAdHoc,
                pfsetting.AdHocId,
                pfsetting.Section,
                pfsetting.SectionMaxLimit,
                pfsetting.IsAffectInGross,
                pfsetting.RequireDocs,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");


            value = _db.Execute<SalaryComponents>("sp_salary_components_insupd", new
            {
                esisetting.ComponentId,
                esisetting.CalculateInPercentage,
                esisetting.EmployeeContribution,
                esisetting.IsActive,
                esisetting.TaxExempt,
                esisetting.ComponentTypeId,
                esisetting.ComponentCatagoryId,
                esisetting.IncludeInPayslip,
                esisetting.ComponentFullName,
                esisetting.ComponentDescription,
                esisetting.MaxLimit,
                esisetting.DeclaredValue,
                esisetting.AcceptedAmount,
                esisetting.RejectedAmount,
                esisetting.UploadedFileIds,
                esisetting.EmployerContribution,
                esisetting.IsOpted,
                esisetting.PercentageValue,
                esisetting.Formula,
                esisetting.IsAdHoc,
                esisetting.AdHocId,
                esisetting.Section,
                esisetting.SectionMaxLimit,
                esisetting.IsAffectInGross,
                esisetting.RequireDocs,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");

            var param = new DbParam[]
            {
                new DbParam (PfesiSetting.PfEsi_setting_Id, typeof(int), "_PfEsi_setting_Id"),
                new DbParam (PfesiSetting.IsPF_Limit_Amount_Statutory, typeof(bool), "_IsPF_Limit_Amount_Statutory"),
                new DbParam (PfesiSetting.IsPF_Allow_overriding, typeof(bool), "_IsPF_Allow_overriding"),
                new DbParam (PfesiSetting.IsPF_EmployerContribution_Outside_GS, typeof(bool), "_IsPF_EmployerContribution_Outside_GS"),
                new DbParam (PfesiSetting.IsPF_OtherChgarges, typeof(bool), "_IsPF_OtherChgarges"),
                new DbParam (PfesiSetting.IsPFAllowVPF, typeof(bool), "_IsPFAllowVPF"),
                new DbParam (PfesiSetting.IsESI_Allow_overriding, typeof(bool), "_IsESI_Allow_overriding"),
                new DbParam (PfesiSetting.IsESI_EmployerContribution_Outside_GS, typeof(bool), "_IsESI_EmployerContribution_Outside_GS"),
                new DbParam (PfesiSetting.IsESI_Exclude_EmployerShare, typeof(bool), "_IsESI_Exclude_EmployerShare"),
                new DbParam (PfesiSetting.IsESI_Exclude_EmpGratuity, typeof(bool), "_IsESI_Exclude_EmpGratuity"),
                new DbParam (PfesiSetting.IsESI_Restrict_Statutory, typeof(bool), "_IsESI_Restrict_Statutory"),
                new DbParam (PfesiSetting.IsESI_IncludeBonuses_Eligibility, typeof(bool), "_IsESI_IncludeBonuses_Eligibility"),
                new DbParam (PfesiSetting.IsESI_IncludeBonuses_Calculation, typeof(bool), "_IsESI_IncludeBonuses_Calculation"),
                new DbParam (PfesiSetting.IsPF_Employer_LimitContribution, typeof(bool), "_IsPF_Employer_LimitContribution"),
                new DbParam (_currentSession.CurrentUserDetail.UserId, typeof(long), "_Admin")
            };
            value = _db.ExecuteNonQuery("sp_pf_esi_setting_insupd", param, true);
            if (string.IsNullOrEmpty(value))
                throw new HiringBellException("Unable to update PF Setting.");
            return value;
        }

        public List<OrganizationDetail> GetOrganizationInfo()
        {
            List<OrganizationDetail> organizations = _db.GetList<OrganizationDetail>("sp_organization_setting_get", false);
            return organizations;
        }

        public BankDetail GetOrganizationBankDetailInfoService(int OrganizationId)
        {
            BankDetail result = _db.Get<BankDetail>("sp_bank_accounts_get_by_orgId", new { OrganizationId });
            return result;
        }

        public Payroll GetPayrollSetting(int CompanyId)
        {
            var result = _db.Get<Payroll>("sp_payroll_cycle_setting_getById", new { CompanyId });

            if (result == null)
                throw new HiringBellException("No record found for given company.");

            return result;
        }

        public string InsertUpdatePayrollSetting(Payroll payroll)
        {
            var status = _db.Execute<Payroll>("sp_payroll_cycle_setting_intupd",
                new
                {
                    PayrollCycleSettingId = payroll.PayrollCycleSettingId,
                    CompanyId = payroll.CompanyId,
                    OrganizationId = payroll.OrganizationId,
                    PayFrequency = payroll.PayFrequency,
                    PayCycleMonth = payroll.PayCycleMonth,
                    PayCycleDayOfMonth = payroll.PayCycleDayOfMonth,
                    PayCalculationId = payroll.PayCalculationId,
                    IsExcludeWeeklyOffs = payroll.IsExcludeWeeklyOffs,
                    IsExcludeHolidays = payroll.IsExcludeHolidays,
                    AdminId = _currentSession.CurrentUserDetail.UserId,
                },
                true
            );

            if (string.IsNullOrEmpty(status))
            {
                throw new HiringBellException("Fail to insert or update.");
            }

            return status;
        }

        public string InsertUpdateSalaryStructure(List<SalaryStructure> salaryStructure)
        {
            var status = string.Empty;

            return status;
        }

        public string UpdateGroupSalaryComponentDetailService(string componentId, int groupId, SalaryComponents component)
        {
            var status = string.Empty;
            if (groupId <= 0)
                throw new HiringBellException("Invalid groupId");

            if (string.IsNullOrEmpty(componentId))
                throw new HiringBellException("Invalid component passed.");

            SalaryGroup salaryGroup = _db.Get<SalaryGroup>("sp_salary_group_getById", new { SalaryGroupId = groupId });
            if (salaryGroup == null)
                throw new HiringBellException("Unable to get salary group. Please contact admin");

            salaryGroup.GroupComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);

            var existingComponent = salaryGroup.GroupComponents.Find(x => x.ComponentId == component.ComponentId);
            if (existingComponent == null)
            {
                salaryGroup.GroupComponents.Add(component);
            }
            else
            {
                if (string.IsNullOrEmpty(component.Formula))
                    throw new HiringBellException("Given formula is not correct or unable to submit. Please try again or contact to admin");

                if (component.Formula.Contains('%'))
                {
                    int result = 0;
                    var value = int.TryParse(new string(component.Formula.SkipWhile(x => !char.IsDigit(x))
                     .TakeWhile(x => char.IsDigit(x))
                     .ToArray()), out result);
                    existingComponent.PercentageValue = result;
                    existingComponent.MaxLimit = 0;
                    existingComponent.DeclaredValue = 0;
                    existingComponent.CalculateInPercentage = true;
                }
                else
                {
                    int result = 0;
                    var value = int.TryParse(new string(component.Formula.SkipWhile(x => !char.IsDigit(x))
                     .TakeWhile(x => char.IsDigit(x))
                     .ToArray()), out result);
                    existingComponent.DeclaredValue = result;
                    existingComponent.MaxLimit = 0;
                    existingComponent.PercentageValue = 0;
                    existingComponent.CalculateInPercentage = false;
                }
                existingComponent.Formula = component.Formula;
            }

            salaryGroup.SalaryComponents = JsonConvert.SerializeObject(salaryGroup.GroupComponents);
            status = _db.Execute<SalaryComponents>("sp_salary_group_insupd", new
            {
                salaryGroup.SalaryGroupId,
                salaryGroup.SalaryComponents,
                salaryGroup.GroupName,
                salaryGroup.GroupDescription,
                salaryGroup.MinAmount,
                salaryGroup.MaxAmount,
                AdminId = _currentSession.CurrentUserDetail.UserId
            }, true);

            if (!ApplicationConstants.IsExecuted(status))
                throw new HiringBellException("Fail to update the record.");

            return status;
        }

        public List<SalaryComponents> ActivateCurrentComponentService(List<SalaryComponents> components)
        {
            var salaryComponent = _db.GetList<SalaryComponents>("sp_salary_components_get");
            if (salaryComponent != null)
            {
                SalaryComponents componentItem = null;
                Parallel.ForEach<SalaryComponents>(salaryComponent, x =>
                {
                    componentItem = components.Find(i => i.ComponentId == x.ComponentId);
                    if (componentItem != null)
                    {
                        x.IsOpted = componentItem.IsOpted;
                        x.ComponentCatagoryId = componentItem.ComponentCatagoryId;
                    }
                });

                var updateComponents = (from n in salaryComponent
                                        select new
                                        {
                                            n.ComponentId,
                                            n.ComponentFullName,
                                            n.ComponentDescription,
                                            n.CalculateInPercentage,
                                            n.TaxExempt,
                                            n.ComponentTypeId,
                                            n.ComponentCatagoryId,
                                            n.PercentageValue,
                                            n.MaxLimit,
                                            n.DeclaredValue,
                                            n.RejectedAmount,
                                            n.AcceptedAmount,
                                            n.UploadedFileIds,
                                            n.Formula,
                                            n.EmployeeContribution,
                                            n.EmployerContribution,
                                            n.IncludeInPayslip,
                                            n.IsAdHoc,
                                            n.AdHocId,
                                            n.Section,
                                            n.SectionMaxLimit,
                                            n.IsAffectInGross,
                                            n.RequireDocs,
                                            n.IsOpted,
                                            n.IsActive,
                                            AdminId = _currentSession.CurrentUserDetail.UserId
                                        }).ToList();

                DataTable table = Converter.ToDataTable(updateComponents);
                int statue = _db.BatchInsert("sp_salary_components_insupd", table, true);

                if (statue <= 0)
                    throw new HiringBellException("Unable to update detail");
                else
                    this.AddorRemoveListSalaryComponentfromSalaryGroup(components);
            }
            else
            {
                throw new HiringBellException("Invalid component passed.");
            }

            return salaryComponent;
        }

        private void AddorRemoveListSalaryComponentfromSalaryGroup(List<SalaryComponents> components)
        {
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll");
            if (salaryGroups.Count > 0)
            {
                List<SalaryComponents> salaryComponents = null;
                foreach (SalaryGroup salaryGroup in salaryGroups)
                {
                    salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
                    Parallel.For(0, components.Count, i =>
                    {
                        if (components[i].IsOpted == true)
                            salaryComponents.Add(components[i]);
                        else
                            salaryComponents.RemoveAll(x => x.ComponentId == components[i].ComponentId);

                    });
                    salaryGroup.SalaryComponents = JsonConvert.SerializeObject(salaryComponents);
                }
                DataTable table = Converter.ToDataTable(salaryGroups);
                int statue = _db.BatchInsert("sp_salary_group_insupd", table, true);
                if (statue <= 0)
                    throw new HiringBellException("Unable to update detail");
            }
        }

        private int AddorRemoveSalaryComponentfromSalaryGroup(SalaryComponents components)
        {
            int status = 0;
            List<SalaryGroup> salaryGroups = _db.GetList<SalaryGroup>("sp_salary_group_getAll");
            if (salaryGroups.Count > 0)
            {
                List<SalaryComponents> salaryComponents = null;
                foreach (SalaryGroup salaryGroup in salaryGroups)
                {
                    salaryComponents = JsonConvert.DeserializeObject<List<SalaryComponents>>(salaryGroup.SalaryComponents);
                        if (components.IncludeInPayslip == true)
                            salaryComponents.Add(components);
                        else
                            salaryComponents.RemoveAll(x => x.ComponentId == components.ComponentId);

                    salaryGroup.SalaryComponents = JsonConvert.SerializeObject(salaryComponents);
                }
                DataTable table = Converter.ToDataTable(salaryGroups);
                status = _db.BatchInsert("sp_salary_group_insupd", table, true);
                if (status <= 0)
                    throw new HiringBellException("Unable to update detail");
            }
            return status;
        }

        public List<SalaryComponents> EnableSalaryComponentDetailService(string componentId, SalaryComponents component)
        {
            List<SalaryComponents> salaryComponents = null;

            if (string.IsNullOrEmpty(componentId))
                throw new HiringBellException("Invalid component passed.");

            var salaryComponent = _db.Get<SalaryComponents>("sp_salary_components_get_byId", new { ComponentId = componentId });
            if (salaryComponent != null)
            {
                salaryComponent.CalculateInPercentage = component.CalculateInPercentage;
                salaryComponent.TaxExempt = component.TaxExempt;
                salaryComponent.IsActive = component.IsActive;
                salaryComponent.TaxExempt = component.TaxExempt;
                salaryComponent.RequireDocs = component.RequireDocs;
                salaryComponent.IncludeInPayslip = component.IncludeInPayslip;
                salaryComponent.AdminId = _currentSession.CurrentUserDetail.UserId;

                var status = _db.Execute<SalaryComponents>("sp_salary_components_insupd", new
                {
                    salaryComponent.ComponentId,
                    salaryComponent.ComponentFullName,
                    salaryComponent.ComponentDescription,
                    salaryComponent.ComponentCatagoryId,
                    salaryComponent.CalculateInPercentage,
                    salaryComponent.TaxExempt,
                    salaryComponent.ComponentTypeId,
                    salaryComponent.PercentageValue,
                    salaryComponent.MaxLimit,
                    salaryComponent.DeclaredValue,
                    salaryComponent.AcceptedAmount,
                    salaryComponent.RejectedAmount,
                    salaryComponent.UploadedFileIds,
                    salaryComponent.Formula,
                    salaryComponent.EmployeeContribution,
                    salaryComponent.EmployerContribution,
                    salaryComponent.IncludeInPayslip,
                    salaryComponent.IsAdHoc,
                    salaryComponent.AdHocId,
                    salaryComponent.Section,
                    salaryComponent.SectionMaxLimit,
                    salaryComponent.IsAffectInGross,
                    salaryComponent.RequireDocs,
                    salaryComponent.IsOpted,
                    salaryComponent.IsActive,
                    salaryComponent.AdminId
                }, true);

                if (!ApplicationConstants.IsExecuted(status))
                    throw new HiringBellException("Fail to update the record.");

                int returnstatus = this.AddorRemoveSalaryComponentfromSalaryGroup(component);
                if (returnstatus <= 0)
                    throw new HiringBellException("Unable to update detail");

                salaryComponents = _db.GetList<SalaryComponents>("sp_salary_components_get_type", new { ComponentTypeId = 0 });
                if (salaryComponents == null)
                    throw new HiringBellException("Fail to retrieve component detail.");
            }
            else
            {
                throw new HiringBellException("Invalid component passed.");
            }

            return salaryComponents;
        }

        public List<SalaryComponents> FetchComponentDetailByIdService(int componentTypeId)
        {
            if (componentTypeId < 0)
                throw new HiringBellException("Invalid component type passed.");

            List<SalaryComponents> salaryComponent = _db.GetList<SalaryComponents>("sp_salary_components_get_type", new { ComponentTypeId = componentTypeId });
            if (salaryComponent == null)
                throw new HiringBellException("Fail to retrieve component detail.");

            return salaryComponent;
        }

        public List<SalaryComponents> FetchActiveComponentService()
        {
            List<SalaryComponents> salaryComponent = _db.GetList<SalaryComponents>("sp_salary_components_get");
            if (salaryComponent == null)
                throw new HiringBellException("Fail to retrieve component detail.");

            return salaryComponent;
        }
    }
}
