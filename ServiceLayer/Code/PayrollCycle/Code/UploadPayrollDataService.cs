using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Code.PayrollCycle.Code
{
    public class UploadPayrollDataService
    {
        public async Task ReadPayrollDataService(IFormFileCollection file)
        {
            DataTable dataTable = null;
            List<string> header = new List<string>();
            List<object> excelData = new List<object>();

            using (var ms = new MemoryStream())
            {
                await file.GetFile("payrolldata").CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = ExcelReaderFactory.CreateReader(ms))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    });

                    dataTable = result.Tables[0];

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        if (!header.Contains(column.ColumnName))
                            header.Add(column.ColumnName);
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (DataColumn column in dataTable.Columns)
                        {
                            excelData.Add(row[column]);
                        }
                    }
                }
            }
        }
    }
}
