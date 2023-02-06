using BottomhalfCore.DatabaseLayer.Common.Code;
using ModalLayer.Modal;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ServiceLayer.Code
{
    public class LiveUrlService : ILiveUrlService
    {
        private readonly IDb db;
        public LiveUrlService(IDb db)
        {
            this.db = db;
        }

        public DataSet LoadPageData(FilterModel filterModel)
        {
            if (string.IsNullOrEmpty(filterModel.SearchString))
                filterModel.SearchString = "1=1";
            if (filterModel.PageIndex <= 0)
                filterModel.PageIndex = 1;
            if (filterModel.PageSize < 10)
                filterModel.PageSize = 10;

            DbParam[] param = new DbParam[]
            {
                new DbParam(filterModel.SearchString, typeof(string), "@searchString"),
                new DbParam(filterModel.PageIndex, typeof(int), "@pageIndex"),
                new DbParam(filterModel.PageSize, typeof(int), "@pageSize")
            };

            DataSet ds = this.db.GetDataset("SP_liveurl_get", param);
            return ds;
        }

        public DataSet SaveUrlService(LiveUrlModal liveUrlModal)
        {
            if (string.IsNullOrEmpty(liveUrlModal.method))
                return null;
            if (string.IsNullOrEmpty(liveUrlModal.url))
                return null;

            this.db.Execute<string>("SP_liveurl_InsUpd", new
            {
                savedUrlId = liveUrlModal.savedUrlId,
                method = liveUrlModal.method,
                parameter = liveUrlModal.paramters,
                url = liveUrlModal.url
            }, false);
            DataSet ds = LoadPageData(new FilterModel { SearchString = "1=1" });
            return ds;
        }
    }
}
