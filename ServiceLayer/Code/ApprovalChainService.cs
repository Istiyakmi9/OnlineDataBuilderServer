using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Threading.Tasks;
using System.Linq;
using BottomhalfCore.Services.Code;
using ModalLayer;
using System.Text;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using static ApplicationConstants;

namespace ServiceLayer.Code
{
    public class ApprovalChainService : IApprovalChainService
    {
        private readonly IDb _db;
        private readonly CurrentSession _currentSession;
        public ApprovalChainService(IDb db, CurrentSession currentSession)
        {
            _db = db;
            _currentSession = currentSession;
        }

        public async Task<ApprovalWorkFlowModal> GetApprovalChainService(FilterModel filterModel)
        {
            ApprovalWorkFlowModal approvalWorkFlowModal = null;
            return await Task.FromResult(approvalWorkFlowModal);
        }

        private string GetSelectQuery(ApprovalWorkFlowChain approvalWorkFlowModal)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("1=1 ");

            if (approvalWorkFlowModal.ApprovalWorkFlowId > 0)
                sb.Append($"and w.ApprovalWorkFlowId = {approvalWorkFlowModal.ApprovalWorkFlowId} ");

            if (approvalWorkFlowModal.ApprovalChainDetailId > 0)
                sb.Append($"and w.ApprovalChainDetailId = {approvalWorkFlowModal.ApprovalChainDetailId} ");

            if (!string.IsNullOrEmpty(approvalWorkFlowModal.Title))
                sb.Append($"and f.Title = '{approvalWorkFlowModal.Title}' ");

            return sb.ToString();
        }

        public async Task<string> InsertApprovalChainService(ApprovalWorkFlowChain approvalWorkFlowModal)
        {
            int approvalWorkFlowId = approvalWorkFlowModal.ApprovalWorkFlowId;
            ValidateApprovalWorkFlowDetail(approvalWorkFlowModal);
            DbResult result = new DbResult
            {
                rowsEffected = 0,
                statusMessage = "Failed"
            };

            try
            {
                var approvalWorkFlowModalExisting = _db.GetList<ApprovalWorkFlowChainFilter>("sp_approval_chain_detail_filter", new
                {
                    SearchString = GetSelectQuery(approvalWorkFlowModal),
                });

                if (approvalWorkFlowModalExisting.Count > 0)
                {
                    var firstRecord = approvalWorkFlowModalExisting.First();
                    approvalWorkFlowModal.ApprovalWorkFlowId = firstRecord.ApprovalWorkFlowId;

                    ApprovalChainDetail chainDetail = null;
                    approvalWorkFlowModal.ApprovalChainDetails.ForEach(item =>
                    {
                        chainDetail = approvalWorkFlowModalExisting.FirstOrDefault(x => x.AssignieId == item.AssignieId);

                        if (chainDetail != null)
                        {
                            item.ApprovalChainDetailId = chainDetail.ApprovalChainDetailId;
                            item.ApprovalWorkFlowId = chainDetail.ApprovalWorkFlowId;
                        }
                    });
                }

                //_db.StartTransaction(System.Data.IsolationLevel.ReadUncommitted);

                //result = await _db.ExecuteAsync("sp_approval_work_flow_insupd", new
                //{
                //    approvalWorkFlowModal.ApprovalWorkFlowId,
                //    approvalWorkFlowModal.Title,
                //    approvalWorkFlowModal.TitleDescription,
                //    approvalWorkFlowModal.Status,
                //    approvalWorkFlowModal.IsAutoExpiredEnabled,
                //    approvalWorkFlowModal.AutoExpireAfterDays,
                //    approvalWorkFlowModal.IsSilentListner,
                //    approvalWorkFlowModal.ListnerDetail,
                //    AdminId = _currentSession.CurrentUserDetail.UserId
                //}, true);

                //if (!string.IsNullOrEmpty(result.statusMessage))
                //{
                //    approvalWorkFlowId = Convert.ToInt32(result.statusMessage);
                //    var data = (from n in approvalWorkFlowModal.ApprovalChainDetails
                //                select new ApprovalChainDetail
                //                {
                //                    ApprovalChainDetailId = n.ApprovalChainDetailId,
                //                    ApprovalWorkFlowId = approvalWorkFlowId,
                //                    AssignieId = n.AssignieId,
                //                    IsRequired = n.IsRequired,
                //                    IsForwardEnabled = n.IsForwardEnabled,
                //                    ForwardWhen = n.ForwardWhen,
                //                    ForwardAfterDays = n.ForwardAfterDays,
                //                    ApprovalStatus = n.ApprovalStatus
                //                }
                //    ).ToList<ApprovalChainDetail>();

                //    var rowsEffected = await _db.BulkExecuteAsync("sp_approval_chain_detail_insupd", data, true);
                //    if (rowsEffected == 0)
                //        throw HiringBellException.ThrowBadRequest("Fail to insert/update record. Please contact to admin.");

                //    _db.Commit();
                //}
                //else
                //{
                //    throw HiringBellException.ThrowBadRequest("Fail to insert/update record. Please contact to admin.");
                //}
                var data = (from n in approvalWorkFlowModal.ApprovalChainDetails
                            select new
                            {
                                ApprovalChainDetailId = ApplicationConstants.LastInsertedNumericKey,
                                ApprovalWorkFlowId = DbProcedure.getParentKey(approvalWorkFlowId),
                                AssignieId = n.AssignieId,
                                IsRequired = n.IsRequired,
                                IsForwardEnabled = n.IsForwardEnabled,
                                ForwardWhen = n.ForwardWhen,
                                ForwardAfterDays = n.ForwardAfterDays,
                                LastUpdatedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                ApprovalStatus = n.ApprovalStatus
                            }
                    ).ToList<object>();

                await _db.ConsicutiveBatchInset("sp_approval_work_flow_insupd",
                    new
                    {
                        approvalWorkFlowModal.ApprovalWorkFlowId,
                        approvalWorkFlowModal.Title,
                        approvalWorkFlowModal.TitleDescription,
                        approvalWorkFlowModal.Status,
                        approvalWorkFlowModal.IsAutoExpiredEnabled,
                        approvalWorkFlowModal.AutoExpireAfterDays,
                        approvalWorkFlowModal.IsSilentListner,
                        approvalWorkFlowModal.ListnerDetail,
                        AdminId = _currentSession.CurrentUserDetail.UserId
                    },
                    DbProcedure.Test,
                    data);
            }
            catch (Exception)
            {
                _db.RollBack();
                await _db.ExecuteAsync("sp_approval_workflow_chain_del", new
                {
                    ApprovalWorkFlowId = approvalWorkFlowId
                });
                throw HiringBellException.ThrowBadRequest("Fail to insert/update record. Please contact to admin.");
            }

            return result.statusMessage;
        }

        private void ValidateApprovalWorkFlowDetail(ApprovalWorkFlowChain approvalWorkFlowModal)
        {
            if (string.IsNullOrEmpty(approvalWorkFlowModal.Title))
                throw HiringBellException.ThrowBadRequest("Tite is null or empty");

            if (string.IsNullOrEmpty(approvalWorkFlowModal.TitleDescription))
                throw HiringBellException.ThrowBadRequest("Title description is null or empty");

            if (approvalWorkFlowModal.IsAutoExpiredEnabled)
            {
                if (approvalWorkFlowModal.AutoExpireAfterDays <= 0)
                    throw HiringBellException.ThrowBadRequest("Please add auto expire after days");
            }

            if (approvalWorkFlowModal.ApprovalChainDetails.Count > 0)
            {
                foreach (var item in approvalWorkFlowModal.ApprovalChainDetails)
                {
                    if (item.AssignieId <= 0)
                        throw HiringBellException.ThrowBadRequest("Please add assigne first");

                    if (item.IsForwardEnabled)
                    {
                        if (item.ForwardWhen <= 0)
                            throw HiringBellException.ThrowBadRequest("Invalid reason selected");

                        if (item.ForwardAfterDays <= 0)
                            throw HiringBellException.ThrowBadRequest("Please add forward after days");
                    }
                }
            }
        }

        public async Task<List<ApprovalWorkFlowModal>> GetPageDateService(FilterModel filterModel)
        {
            if (string.IsNullOrEmpty(filterModel.SearchString))
                filterModel.SearchString = "1=1";

            var result = _db.GetList<ApprovalWorkFlowModal>("sp_approval_work_flow_filter", new
            {
                filterModel.SearchString,
                filterModel.SortBy,
                filterModel.PageSize,
                filterModel.PageIndex
            });

            return await Task.FromResult(result);
        }

        public async Task<ApprovalWorkFlowChain> GetApprovalChainData(int ApprovalWorkFlowId)
        {
            string searchString = string.Empty;
            if (ApprovalWorkFlowId == 0)
                throw HiringBellException.ThrowBadRequest("Invalid approval chain detail id passed");

            var result = _db.GetList<ApprovalWorkFlowChainFilter>("sp_approval_chain_detail_by_id", new
            {
                ApprovalWorkFlowId
            });

            if (result.Count == 0)
                throw HiringBellException.ThrowBadRequest("Fail to get record.");

            var firstRecord = result.First();
            ApprovalWorkFlowChain approvalWorkFlowChain = new ApprovalWorkFlowChain
            {
                ApprovalChainDetailId = firstRecord.ApprovalChainDetailId,
                ApprovalWorkFlowId = firstRecord.ApprovalWorkFlowId,
                Title = firstRecord.Title,
                TitleDescription = firstRecord.TitleDescription,
                Status = firstRecord.Status,
                IsAutoExpiredEnabled = firstRecord.IsAutoExpiredEnabled,
                AutoExpireAfterDays = firstRecord.AutoExpireAfterDays,
                IsSilentListner = firstRecord.IsSilentListner,
                ListnerDetail = firstRecord.ListnerDetail,
                ApprovalChainDetails = new List<ApprovalChainDetail>()
            };

            approvalWorkFlowChain.ApprovalChainDetails = (
                from n in result
                select new ApprovalChainDetail
                {
                    ApprovalChainDetailId = n.ApprovalChainDetailId,
                    ApprovalWorkFlowId = n.ApprovalWorkFlowId,
                    AssignieId = n.AssignieId,
                    IsRequired = n.IsRequired,
                    IsForwardEnabled = n.IsForwardEnabled,
                    ForwardWhen = n.ForwardWhen,
                    ForwardAfterDays = n.ForwardAfterDays,
                    LastUpdatedOn = n.LastUpdatedOn,
                    ApprovalStatus = n.ApprovalStatus
                }
             ).ToList<ApprovalChainDetail>();

            return await Task.FromResult(approvalWorkFlowChain);
        }

    }
}
