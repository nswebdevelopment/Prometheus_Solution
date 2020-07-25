using Prometheus.Model.Models.EnterpriseAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.DbAdapter.Queries
{
    public class QueryRead
    {
        public ParentTableModel ParentTable { get; set; }
        public List<ChildTableModel> ChildTables { get; set; }


        public string SelectQuery
        {
            get
            {
                if(!ChildTables.Any())
                {
                    return $"SELECT {SelectColumns()} FROM {ParentTable.TableName}";
                }
                else
                {
                    return $"SELECT {SelectColumns()} FROM {ParentTable.TableName} {MultipleTables()}";
                }
            }
        }

        private string SelectColumns()
        {
            string delimeter = ",";
            var queryString = new List<string>();
            
            foreach(var column in ParentTable.Columns)
            {
                queryString.Add($"{ParentTable.TableName}.{column.ColumnName} {Cast(column.PropertyName)}");
            }
            
            for (var i = 0; i < ChildTables.Count; i++)
            {
                for (var j = 0; j < ChildTables[i].Columns.Count; j++)
                {
                   queryString.Add($"{ChildTables[i].TableName}.{ChildTables[i].Columns[j].ColumnName} {Cast(ChildTables[i].Columns[j].PropertyName)}");

                }
            }
            
            return queryString.Aggregate((c, i) => c + delimeter + i);
        }

        private string MultipleTables()
        {
            var queryString = new List<string>();
            
            foreach(var childTable in ChildTables)
            {
                string parentTableFK = "";

                foreach (var item in ParentTable.ForeignKeys)
                {
                    if (item.Key == childTable.TableName)
                    {
                        parentTableFK = item.Value;
                    }
                }
                queryString.Add($"INNER JOIN {childTable.TableName} ON {ParentTable.TableName}.{parentTableFK} = {childTable.TableName}.{childTable.PrimaryKey}");
            }
            
            return queryString.Aggregate((c, i) => c + " " + i); 
        }

        private string Cast(string propertyName)
        {
            return propertyName != null ? ("as " + propertyName) : null;
        }
    }
}
