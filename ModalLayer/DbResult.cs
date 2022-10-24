using System.Data;

namespace ModalLayer
{
    public class DbResult
    {
        public static DbResult Build(int _rowsEffected = 0, string _statusMessage = null, DataTable _dataTable = null, DataSet _dataSet = null)
        {
            return new DbResult
            {
                rowsEffected = _rowsEffected,
                statusMessage = _statusMessage,
                dataTable = _dataTable,
                dataSet = _dataSet
            };
        }

        public int rowsEffected { set; get; }
        public string statusMessage { set; get; }
        public DataTable dataTable { set; get; }
        public DataSet dataSet { set; get; }
    }
}
