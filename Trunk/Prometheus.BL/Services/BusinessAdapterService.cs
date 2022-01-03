using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using Prometheus.Dal;
using Serilog;
using Prometheus.Common.Enums;
using System.IO;
using CsvHelper;
using System.Text;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System.Diagnostics;
using Prometheus.Model.Models.NeoAdapterModel;
using OfficeOpenXml.Style;
using Prometheus.Model.Models.LitecoinBlockModel;

namespace Prometheus.BL.Services
{
    public class BusinessAdapterService : IBusinessAdapterService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public BusinessAdapterService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }
        

        public IResponse<NoValue> CreateBusinessAdapter(BusinessAdapterModel businessAdapter, long userProfileId)
        {
            var response = new Response<NoValue>();
            try
            {
                //refactor with properties
                Adapter newAdapter = new Adapter
                {
                    Name = businessAdapter.Name,
                    UserProfileId = userProfileId,
                    AdapterTypeItemId = (int)businessAdapter.BusinessAdapterType,
                    StatusId = (int)Statuses.Active,
                    DirectionId = (int)businessAdapter.Direction
                };

                BusinessAdapter newBusinessAdapter = new BusinessAdapter
                {
                    AdapterId = newAdapter.Id,
                    Filename = businessAdapter.FileName
                };

                _entity.Adapter.Add(newAdapter);
                _entity.BusinessAdapter.Add(newBusinessAdapter);

                _entity.SaveChanges();

                response.Message = "Business adapter added successfully.";
                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateBusinessAdapter(userProfileId: {userProfileId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<List<BusinessAdapterModel>> GetBusinessAdapters(long userProfileId)
        {
            var result = new Response<List<BusinessAdapterModel>>();

            try
            {
                result.Value = _entity.BusinessAdapter.Where(a => a.Adapter.UserProfileId == userProfileId && a.Adapter.StatusId == (int)Statuses.Active)
                    .Select(a => new BusinessAdapterModel
                    {
                        Id = a.Id,
                        Name = a.Adapter.Name,
                        BusinessAdapterType = (AdapterTypeItemEnum)a.Adapter.AdapterTypeItemId,
                        Direction = (DirectionEnum)a.Adapter.DirectionId
                    }).ToList();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"BusinessAdapterService.GetBusinessAdapters(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<BusinessAdapterModel> UpdateBusinessAdapter(BusinessAdapterModel model)
        {
            var result = new Response<BusinessAdapterModel>();
            try
            {
                var businessAdapter = _entity.BusinessAdapter.Find(model.Id);

                var jobDefinitionExits = _entity.JobDefinition.Any(jd => jd.From == businessAdapter.Adapter.Id || jd.To == businessAdapter.Adapter.Id);

                if (jobDefinitionExits)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Unable to update business adapter if it is used in job definition.";
                    return result;
                }

                businessAdapter.Adapter.Name = model.Name;
                businessAdapter.Adapter.DirectionId = (int)model.Direction;
                businessAdapter.Filename = model.FileName;
                businessAdapter.Adapter.AdapterTypeItemId = (int)model.BusinessAdapterType;
                
                _entity.SaveChanges();
                result.Status = StatusEnum.Success;
                result.Message = Message.ChangesSaved;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"BusinessAdapterService.UpdateBusinessAdapter(model: {model})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> DeleteBusinessAdapter(long businessAdapterId)
        {
            var result = new Response<NoValue>();
            try
            {
                var businessAdapter = _entity.BusinessAdapter.Find(businessAdapterId);
                var jobDefinitionExists = _entity.JobDefinition.Any(jd => jd.From == businessAdapter.Adapter.Id || jd.To == businessAdapter.Adapter.Id);

                if (jobDefinitionExists)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Unable to delete business adapter if it is used in job definition.";
                    return result;
                }

                businessAdapter.Adapter.StatusId = (int)Statuses.Deleted;
                _entity.SaveChanges();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"BusinessAdapterService.DeleteBusinessAdapter(businessAdapterId: {businessAdapterId})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> CreateXlsxFile(List<EthereumBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var excel = new ExcelPackage())
                {
                    var stopwatch = Stopwatch.StartNew();

                    excel.Workbook.Worksheets.Add("Prometheus");

                    var headerRow = new List<string[]>()
                    {
                        new string[]{"BlockNumber", "TimeStamp", "Hash", "From", "To", "Value", "Status"}
                    };

                    var headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    var worksheet = excel.Workbook.Worksheets["Prometheus"];

                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);
                    worksheet.Cells[headerRange].Style.Font.Bold = true;

                    var cellData = list.SelectMany(b => b.BlockTransactions,
                                                        (b, t) => new ExcelEthereumBlockModel
                                                        {
                                                            BlockNumber = b.BlockNumber,
                                                            TimeStamp = b.TimeStamp,
                                                            Hash = t.Hash,
                                                            From = t.From,
                                                            To = t.To,
                                                            Value = t.Value,
                                                            Status = t.Status.ToString()
                                                        });

                    var type = typeof(ExcelEthereumBlockModel);
                    var numberOfColumns = type.GetProperties().Length;

                    var cellDataRange = "A2:" + Char.ConvertFromUtf32(numberOfColumns + 64) + $"{cellData.Count() + 1}";

                    worksheet.Cells[cellDataRange].LoadFromCollection(cellData);
                    worksheet.Cells["B2:B" + $"{cellData.Count() + 1}"].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var excelFileByteContent = excel.GetAsByteArray();

                    var businessFile = new BusinessFile
                    {
                        File = excelFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                    stopwatch.Stop();
                    _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> CreateXlsxFile(List<BitcoinBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var excel = new ExcelPackage())
                {
                    var stopwatch = Stopwatch.StartNew();

                    excel.Workbook.Worksheets.Add("tx");
                    excel.Workbook.Worksheets.Add("txIn");
                    excel.Workbook.Worksheets.Add("txOut");

                    var worksheetTx = excel.Workbook.Worksheets["tx"];
                    worksheetTx.SetValue("A1", "BlockNumber");
                    worksheetTx.SetValue("B1", "txHash");
                    worksheetTx.SetValue("C1", "Timestamp");
                    worksheetTx.SetValue("D1", "TotalOutValue");
                    worksheetTx.Row(1).Style.Font.Bold = true;

                    var worksheetTxIn = excel.Workbook.Worksheets["txIn"];
                    worksheetTxIn.SetValue("A1", "txHash");
                    worksheetTxIn.SetValue("B1", "Address");
                    worksheetTxIn.Row(1).Style.Font.Bold = true;

                    var worksheetTxOut = excel.Workbook.Worksheets["txOut"];
                    worksheetTxOut.SetValue("A1", "txHash");
                    worksheetTxOut.SetValue("B1", "Address");
                    worksheetTxOut.SetValue("C1", "Value");
                    worksheetTxOut.Row(1).Style.Font.Bold = true;

                    int rowCounterTx = 2;
                    int rowCounterTxIn = 2;
                    int rowCounterTxOut = 2;

                    foreach (var block in list)
                    {
                        foreach (var transaction in block.TransactionList)
                        {
                            worksheetTx.SetValue(rowCounterTx, 1, block.BlockNumber);
                            worksheetTx.SetValue(rowCounterTx, 2, transaction.TransactionHash);
                            worksheetTx.SetValue(rowCounterTx, 3, block.Time);
                            worksheetTx.SetValue(rowCounterTx, 4, transaction.TotalOutValue);

                            foreach (var txIn in transaction.TransactionInputs)
                            {
                                worksheetTxIn.SetValue(rowCounterTxIn, 1, transaction.TransactionHash);
                                worksheetTxIn.SetValue(rowCounterTxIn, 2, txIn.Address);

                                rowCounterTxIn++;
                            }

                            foreach (var txOut in transaction.TransactionOutputs)
                            {
                                worksheetTxOut.SetValue(rowCounterTxOut, 1, transaction.TransactionHash);
                                worksheetTxOut.SetValue(rowCounterTxOut, 2, txOut.Address);
                                worksheetTxOut.SetValue(rowCounterTxOut, 3, txOut.Value);

                                rowCounterTxOut++;
                            }

                            rowCounterTx++;
                        }
                    }

                    worksheetTx.Column(3).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                    worksheetTx.Cells[worksheetTx.Dimension.Address].AutoFitColumns();
                    worksheetTxIn.Cells[worksheetTxIn.Dimension.Address].AutoFitColumns();
                    worksheetTxOut.Cells[worksheetTxOut.Dimension.Address].AutoFitColumns();

                    var excelFileByteContent = excel.GetAsByteArray();

                    var businessFile = new BusinessFile
                    {
                        File = excelFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                    stopwatch.Stop();
                    _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;

            }

            return response;
        }

        public IResponse<NoValue> CreateCsvFile(List<EthereumBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var ms = new MemoryStream())
                using (var sw = new StreamWriter(ms))
                {
                    var stopwatch = Stopwatch.StartNew();

                    var writer = new CsvWriter(sw);

                    writer.Configuration.Delimiter = ";";

                    writer.WriteHeader<ExcelEthereumBlockModel>();

                    writer.NextRecord();

                    foreach (var block in list)
                    {
                        foreach (var transaction in block.BlockTransactions)
                        {

                            writer.WriteField(block.BlockNumber);
                            writer.WriteField(block.TimeStamp);
                            writer.WriteField(transaction.Hash);
                            writer.WriteField(transaction.From);
                            writer.WriteField(transaction.To);
                            writer.WriteField(transaction.Value);
                            writer.WriteField(transaction.Status.ToString());

                            writer.NextRecord();
                        }
                    }

                    writer.Flush();
                    sw.Flush();

                    var csvFileByteContent = ms.ToArray();

                    var businessFile = new BusinessFile
                    {
                        File = csvFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                    stopwatch.Stop();
                    _logger.Information($"BusinessAdapterService.CreateCsvFile(list: {list.Count}, jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");
                }

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateCsvFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<BusinessAdapterModel> GetBusinessAdapter(long businessAdapterId)
        {
            var response = new Response<BusinessAdapterModel>();

            try
            {
                var businessAdapter = _entity.BusinessAdapter.Find(businessAdapterId);

                response.Value = new BusinessAdapterModel
                {
                    Id = businessAdapter.Id,
                    Name = businessAdapter.Adapter.Name,
                    BusinessAdapterType = (AdapterTypeItemEnum)businessAdapter.Adapter.AdapterTypeItemId,
                    Direction = (DirectionEnum)businessAdapter.Adapter.DirectionId,
                    FileName = businessAdapter.Filename
                };
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.GetBusinessAdapter(businessAdapterId: {businessAdapterId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<BusinessFileModel> GetExcelFile(long jobId)
        {
            var response = new Response<BusinessFileModel>();

            try
            {
                var excelFile = _entity.BusinessFile.Where(bf => bf.JobTimelineId == jobId).Select(f => f.File).FirstOrDefault();

                var businessAdapter = _entity.JobTimeline.Find(jobId).Schedule.JobDefinition.Adapter1;

                response.Status = StatusEnum.Success;
                response.Value = new BusinessFileModel
                {
                    File = excelFile,
                    BusinessAdapterType = (AdapterTypeItemEnum)businessAdapter.AdapterTypeItemId,
                    FileName = businessAdapter.BusinessAdapter.FirstOrDefault().Filename
                };

            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.GetExcelFile(jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> CreateXlsxFile(List<NeoBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add("Tx");
                    excel.Workbook.Worksheets.Add("TxIn");
                    excel.Workbook.Worksheets.Add("TxOut");

                    var worksheetTx = excel.Workbook.Worksheets["Tx"];
                    worksheetTx.SetValue("A1", "BlockNumber");
                    worksheetTx.SetValue("B1", "TxId");
                    worksheetTx.SetValue("C1", "TxType");
                    worksheetTx.SetValue("D1", "Timestamp");
                    worksheetTx.Row(1).Style.Font.Bold = true;

                    var worksheetTxIn = excel.Workbook.Worksheets["TxIn"];
                    worksheetTxIn.SetValue("A1", "TxId");
                    worksheetTxIn.SetValue("B1", "TxInId");
                    worksheetTxIn.Row(1).Style.Font.Bold = true;

                    var worksheetTxOut = excel.Workbook.Worksheets["TxOut"];
                    worksheetTxOut.SetValue("A1", "TxId");
                    worksheetTxOut.SetValue("B1", "Address");
                    worksheetTxOut.SetValue("C1", "Asset");
                    worksheetTxOut.SetValue("D1", "Value");
                    worksheetTxOut.Row(1).Style.Font.Bold = true;

                    int rowCounterTx = 2;
                    int rowCounterTxIn = 2;
                    int rowCounterTxOut = 2;

                    foreach (var block in list)
                    {
                        foreach (var transaction in block.TransactionList)
                        {
                            worksheetTx.SetValue(rowCounterTx, 1, block.BlockNumber);
                            worksheetTx.SetValue(rowCounterTx, 2, transaction.TransactionId);
                            worksheetTx.SetValue(rowCounterTx, 3, transaction.TransactionType);
                            worksheetTx.SetValue(rowCounterTx, 4, block.Time);

                            foreach (var txIn in transaction.TransactionInputs)
                            {
                                worksheetTxIn.SetValue(rowCounterTxIn, 1, transaction.TransactionId);
                                worksheetTxIn.SetValue(rowCounterTxIn, 2, txIn.TransactionId);

                                rowCounterTxIn++;
                            }

                            foreach (var txOut in transaction.TransactionOutputs)
                            {
                                worksheetTxOut.SetValue(rowCounterTxOut, 1, transaction.TransactionId);
                                worksheetTxOut.SetValue(rowCounterTxOut, 2, txOut.Address);
                                worksheetTxOut.SetValue(rowCounterTxOut, 3, txOut.Asset);
                                worksheetTxOut.SetValue(rowCounterTxOut, 4, txOut.Value);

                                rowCounterTxOut++;
                            }

                            rowCounterTx++;
                        }
                    }

                    worksheetTx.Column(4).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                    worksheetTxOut.Column(4).Style.Numberformat.Format = "@";
                    worksheetTxOut.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    worksheetTx.Cells[worksheetTx.Dimension.Address].AutoFitColumns();
                    worksheetTxIn.Cells[worksheetTxIn.Dimension.Address].AutoFitColumns();
                    worksheetTxOut.Cells[worksheetTxOut.Dimension.Address].AutoFitColumns();

                    var excelFileByteContent = excel.GetAsByteArray();

                    var businessFile = new BusinessFile
                    {
                        File = excelFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> CreateXlsxFile(List<LitecoinBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var excel = new ExcelPackage())
                {
                    var stopwatch = Stopwatch.StartNew();

                    excel.Workbook.Worksheets.Add("tx");
                    excel.Workbook.Worksheets.Add("txIn");
                    excel.Workbook.Worksheets.Add("txOut");

                    var worksheetTx = excel.Workbook.Worksheets["tx"];
                    worksheetTx.SetValue("A1", "BlockNumber");
                    worksheetTx.SetValue("B1", "txHash");
                    worksheetTx.SetValue("C1", "Timestamp");
                    worksheetTx.SetValue("D1", "TotalOutValue");
                    worksheetTx.Row(1).Style.Font.Bold = true;

                    var worksheetTxIn = excel.Workbook.Worksheets["txIn"];
                    worksheetTxIn.SetValue("A1", "txHash");
                    worksheetTxIn.SetValue("B1", "Address");
                    worksheetTxIn.Row(1).Style.Font.Bold = true;

                    var worksheetTxOut = excel.Workbook.Worksheets["txOut"];
                    worksheetTxOut.SetValue("A1", "txHash");
                    worksheetTxOut.SetValue("B1", "Address");
                    worksheetTxOut.SetValue("C1", "Value");
                    worksheetTxOut.Row(1).Style.Font.Bold = true;

                    int rowCounterTx = 2;
                    int rowCounterTxIn = 2;
                    int rowCounterTxOut = 2;

                    foreach (var block in list)
                    {
                        foreach (var transaction in block.TransactionList)
                        {
                            worksheetTx.SetValue(rowCounterTx, 1, block.BlockNumber);
                            worksheetTx.SetValue(rowCounterTx, 2, transaction.TransactionHash);
                            worksheetTx.SetValue(rowCounterTx, 3, block.Time);
                            worksheetTx.SetValue(rowCounterTx, 4, transaction.TotalOutValue);

                            foreach (var txIn in transaction.TransactionInputs)
                            {
                                worksheetTxIn.SetValue(rowCounterTxIn, 1, transaction.TransactionHash);
                                worksheetTxIn.SetValue(rowCounterTxIn, 2, txIn.Address);

                                rowCounterTxIn++;
                            }

                            foreach (var txOut in transaction.TransactionOutputs)
                            {
                                worksheetTxOut.SetValue(rowCounterTxOut, 1, transaction.TransactionHash);
                                worksheetTxOut.SetValue(rowCounterTxOut, 2, txOut.Address);
                                worksheetTxOut.SetValue(rowCounterTxOut, 3, txOut.Value);

                                rowCounterTxOut++;
                            }

                            rowCounterTx++;
                        }
                    }

                    worksheetTx.Column(3).Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";

                    worksheetTx.Cells[worksheetTx.Dimension.Address].AutoFitColumns();
                    worksheetTxIn.Cells[worksheetTxIn.Dimension.Address].AutoFitColumns();
                    worksheetTxOut.Cells[worksheetTxOut.Dimension.Address].AutoFitColumns();

                    var excelFileByteContent = excel.GetAsByteArray();

                    var businessFile = new BusinessFile
                    {
                        File = excelFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                    stopwatch.Stop();
                    _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;

            }

            return response;
        }
        public IResponse<NoValue> CreateXlsxFile(List<SolanaBlockModel> list, long jobId)
        {
            var response = new Response<NoValue>();

            try
            {
                using (var excel = new ExcelPackage())
                {
                    var stopwatch = Stopwatch.StartNew();

                    excel.Workbook.Worksheets.Add("Prometheus");

                    var headerRow = new List<string[]>()
                    {
                        new string[]{"BlockNumber", "TimeStamp", "Hash", "From", "To", "Value", "Status"}
                    };

                    var headerRange = "A1:" + Char.ConvertFromUtf32(headerRow[0].Length + 64) + "1";

                    var worksheet = excel.Workbook.Worksheets["Prometheus"];

                    worksheet.Cells[headerRange].LoadFromArrays(headerRow);
                    worksheet.Cells[headerRange].Style.Font.Bold = true;

                    var cellData = list.SelectMany(b => b.BlockTransactions,
                                                        (b, t) => new ExcelEthereumBlockModel
                                                        {
                                                            BlockNumber = b.BlockNumber,
                                                            TimeStamp = b.TimeStamp,
                                                            Hash = t.Hash,
                                                            From = t.From,
                                                            To = t.To,
                                                            Value = t.Value,
                                                            Status = t.Status.ToString()
                                                        });

                    var type = typeof(ExcelEthereumBlockModel);
                    var numberOfColumns = type.GetProperties().Length;

                    var cellDataRange = "A2:" + Char.ConvertFromUtf32(numberOfColumns + 64) + $"{cellData.Count() + 1}";

                    worksheet.Cells[cellDataRange].LoadFromCollection(cellData);
                    worksheet.Cells["B2:B" + $"{cellData.Count() + 1}"].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var excelFileByteContent = excel.GetAsByteArray();

                    var businessFile = new BusinessFile
                    {
                        File = excelFileByteContent,
                        JobTimelineId = jobId
                    };

                    _entity.BusinessFile.Add(businessFile);
                    _entity.SaveChanges();

                    stopwatch.Stop();
                    _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BusinessAdapterService.CreateXlsxFile(list: {list.Count}, jobId: {jobId}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
