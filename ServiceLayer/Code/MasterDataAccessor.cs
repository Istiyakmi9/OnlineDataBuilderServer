using BottomhalfCore.CacheManagement.Caching;
using BottomhalfCore.CacheManagement.CachingInterface;
using BottomhalfCore.DatabaseLayer.Common.Code;
using BottomhalfCore.DatabaseLayer.MsSql.Code;
using BottomhalfCore.FactoryContext;
using BottomhalfCore.Services.Code;
using BottomhalfCore.Services.Interface;
using ModalLayer.Modal;
using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceLayer.Code
{
    public class MasterDataAccessor : ICacheManagerAccessor<MasterDataAccessor>
    {
        private readonly BeanContext context;
        public string KeyName { set; get; }
        private readonly ICacheManager<CacheManager> cacheManager;
        private readonly IDb db;
        public MasterDataAccessor()
        {
            this.cacheManager = CacheManager.GetInstance();
            this.KeyName = "MasterData";
            this.context = BeanContext.GetInstance();
            this.db = new Db(context.GetConnectionString());
        }

        public Object LoadData()
        {
            DataSet ds = this.db.GetDataset("sp_OnlineDatabuilder_MasterData");
            IAutoMapper<TableAutoMapper> autoMapper = new TableAutoMapper();// this.context.GetBean<TableAutoMapper>();
            List<MasterQuaryClouse> masterQuaryClouse = autoMapper.AutoMapToObjectList<MasterQuaryClouse>(ds.Tables[0]);
            SampleSelecteQuery sampleSelecteQuery = autoMapper.AutoMapToObject<SampleSelecteQuery>(ds.Tables[1]);
            List<MappedUISqlDataType> mappedUISqlDataType = autoMapper.AutoMapToObjectList<MappedUISqlDataType>(ds.Tables[2]);
            MasterTables masterTables = new MasterTables();
            masterTables.masterQuaryClouse = masterQuaryClouse;
            masterTables.sampleSelecteQuery = sampleSelecteQuery;
            masterTables.mappedUISqlDataType = mappedUISqlDataType;
            this.cacheManager.Put(this.KeyName, masterTables);
            return masterTables;
        }
    }
}
