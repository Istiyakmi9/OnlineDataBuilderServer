using System;

namespace BottomhalfCore.DatabaseLayer.Common.Code
{
    public class DbParam
    {
        public DbParam(dynamic Data, Type DataType, int size, string Paramname)
        {
            Value = Data;
            Type = DataType;
            Size = size;
            ParamName = Paramname;
            IsTypeDefined = true;
        }

        public DbParam(dynamic Data, Type DataType, string Paramname)
        {
            Value = Data;
            Type = DataType;
            ParamName = Paramname;
            IsTypeDefined = true;
        }

        public DbParam(dynamic Data, string Paramname)
        {
            Value = Data;
            ParamName = Paramname;
            IsTypeDefined = false;
        }

        public dynamic Value { set; get; }
        public Type Type { set; get; }
        public int Size { set; get; }
        public string ParamName { set; get; }
        public bool IsTypeDefined { set; get; } = false;

    }

    public class NotDynamic
    {
        public string NotDynamicProp { set; get; }
    }
}
