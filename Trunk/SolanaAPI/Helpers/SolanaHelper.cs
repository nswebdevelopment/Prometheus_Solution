using Prometheus.Common;
using Prometheus.Model.Models;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Wallet;
using System.Data;
using System.Data.SqlClient;

namespace Prometheus.SolanaAPI.Helpers
{
    public static class SolanaHelper
    {
        public static List<GenericModel> ConnectAndRead()
        {

            var response = new Response<List<GenericModel>>();
            response.Value = new List<GenericModel>();

            using (var conn = new SqlConnection("Server=NSWD-LT055\\SQLEXPRESS;Database=Retail_Bank_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM [Transaction]";
                    var adapter = new SqlDataAdapter(query, conn);

                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    ToModel(dataTable, response);
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;

                }
                finally
                {
                    conn.Close();
                }
            }

            return response.Value;
        }

        public static void ToModel(DataTable dataTable, Response<List<GenericModel>> response)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var model = new GenericModel();
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (StringCompare(column.ColumnName, "Id"))
                    {
                        model.TransactionId = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, "AccountId"))
                    {
                        model.TransactionAccount = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, "TransactionTypeId"))
                    {
                        model.TransactionType = row[column].ToString();
                    }
                    else if (StringCompare(column.ColumnName, "Amount"))
                    {

                        model.TransactionAmount = row[column].ToString();
                    }
                }
                response.Value.Add(model);
            }
        }

        static private bool StringCompare(string str1, string str2)
        {
            return String.Compare(str1.Trim(), str2.Trim(), true) == 0 ? true : false;
        }

        public static RequestResult<string> SendTransaction(Wallet wallet, IRpcClient rpcClient, ulong amount)
        {
            // Get the source account
            var fromAccount = wallet.GetAccount(0);

            // Get a recent block hash to include in the transaction
            var blockHash = rpcClient.GetRecentBlockHash();

            // Initialize a transaction builder and chain as many instructions as you want before building the message
            var tx = new TransactionBuilder().
                    SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                    SetFeePayer(fromAccount).
                    AddInstruction(MemoProgram.NewMemo(fromAccount, "Hello from Sol.Net :)")).
                    AddInstruction(SystemProgram.Transfer(fromAccount, new PublicKey("T9yhg3Ymr7owHnNsEFZpkKj6bpxqYKTouiNqLDnfdam"), amount)).
                    Build(fromAccount);

            var firstSig = rpcClient.SendTransaction(tx);
            return firstSig;

        }
    }
}
