using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.DbAdapter
{
    public static class Map
    {
        /// <summary>
        /// Mapping datatable data with our model('GenericModel')
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="response">Add properties to list</param>
        public static void ToModel(this DataTable dataTable, Response<List<GenericModel>> response)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var model = new GenericModel();
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (StringCompare(column.ColumnName, PropertyName.TransactionId.ToString()))
                    {
                        model.TransactionId = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, PropertyName.TransactionAccount.ToString()))
                    {
                        model.TransactionAccount = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, PropertyName.TransactionType.ToString()))
                    {
                        model.TransactionType = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, PropertyName.TransactionAmount.ToString()))
                    {
                        model.TransactionAmount = row[column].ToString();
                    }
                }
                response.Value.Add(model);
            }
        }

        static private bool StringCompare (string str1, string str2)
        {
            return String.Compare(str1.Trim(), str2.Trim(), true) == 0 ? true : false;
        }
    }
}
