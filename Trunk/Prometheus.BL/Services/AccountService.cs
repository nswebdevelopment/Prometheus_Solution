using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Dal.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Prometheus.Dal;
using Serilog;
using Prometheus.Common.Enums;

namespace Prometheus.BL.Services
{
    public class AccountService : IAccountService
    {
        private readonly IPrometheusEntities _entity;
        private readonly IBlockchainService _blockchainService;
        private readonly ILogger _logger;

        public AccountService(IPrometheusEntities entity, IBlockchainService blockchainService, ILogger logger)
        {
            _entity = entity;
            _blockchainService = blockchainService;
            _logger = logger;
        }
        public async Task<IResponse<Account>> GetAccount(string transactionAccount, long enterpriseAdapterId, long cryptoAdapterId)
        {
            var response = new Response<Account>();

            try
            {
                var accountModel = _entity.Account.Where(t => t.TransactionAccount == transactionAccount && t.EnterpriseAdapterId == enterpriseAdapterId && t.CryptoAdapterId == cryptoAdapterId).FirstOrDefault();

                if (accountModel == null)
                {
                    var blockchainAccountResponse = await _blockchainService.NewAccount(cryptoAdapterId);
                    if (blockchainAccountResponse.Status == StatusEnum.Success)
                    {
                        var account = new Account
                        {
                            TransactionAccount = transactionAccount,
                            EnterpriseAdapterId = enterpriseAdapterId,
                            CryptoAdapterId = cryptoAdapterId,
                            CreditAddress = blockchainAccountResponse.Value.CreditAddress,
                            DebitAddress = blockchainAccountResponse.Value.DebitAddress
                        };

                        response.Value = account;
                        response.Status = StatusEnum.Success;

                        _entity.Account.Add(account);
                        _entity.SaveChanges();
                    }
                    else
                    {
                        //logger
                    }
                }
                else
                {
                    response.Value = accountModel;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"AccountService.GetAccount(transactionAccount: {transactionAccount}, enterpriseAdapterId: {enterpriseAdapterId}, cryptoAdapterId: {cryptoAdapterId})");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
            }

            return response;
        }
    }
}
