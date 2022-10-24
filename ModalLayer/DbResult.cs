using System.Data;

namespace ModalLayer
{
    public class DbResult
    {
        public void Build(int rowsEffected = 0, string statusMessage = null, DataTable dataTable = null, DataSet dataSet = null)
        {
            this.rowsEffected = rowsEffected;
            this.statusMessage = statusMessage;
            this.dataTable = dataTable;
            this.dataSet = dataSet;
        }

        public int rowsEffected { set; get; }
        public string statusMessage { set; get; }
        public DataTable dataTable { set; get; }
        public DataSet dataSet { set; get; }
    }
}
