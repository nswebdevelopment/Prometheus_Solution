using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Common.Extensiosns;


namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class JobTimelineController : Controller
    {
        private readonly IJobTimelineService _jobTimelineService;
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly ITransactionService _transactionService;
        private readonly IBlockTransactionService _blockTransactionService;
        private readonly IBusinessAdapterService _businessAdapterService;

        public JobTimelineController(IJobTimelineService jobTimelineService, IJobDefinitionService jobDefinitionService, ITransactionService transactionService,  
             IBlockTransactionService blockTransactionService, IBusinessAdapterService businessAdapterService)
        {
            _jobTimelineService = jobTimelineService;
            _jobDefinitionService = jobDefinitionService;
            _transactionService = transactionService;
            _blockTransactionService = blockTransactionService;
            _businessAdapterService = businessAdapterService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var result = _jobTimelineService.GetJobTimeline(User.Identity.GetUserProfileId() ?? default(long));
            if (result.Status != StatusEnum.Success)
            {
                return BadRequest();
            }
            return View(result.Value);
        }


        [HttpGet]
        public IActionResult ShowTransactions(long jobId)
        {

            var jobDefintion = _jobDefinitionService.GetJobDefinitionByJobId(jobId);
            if (jobDefintion.Status == StatusEnum.Success)
            {
                if (jobDefintion.Value.FromCategory != AdapterType.Crypto)
                {
                    var response = _transactionService.GetTransactions(jobId);

                    if (response.Status != StatusEnum.Success)
                    {
                        return BadRequest();
                    }

                    return View(response.Value);
                }
                else
                {
                    var response = _blockTransactionService.ShowBlockTransactions(jobId);

                    if (response.Status != StatusEnum.Success)
                    {
                        return BadRequest();
                    }

                    return View("ShowBlockTransactions", response.Value);
                }
            }
            else
            {
                return BadRequest();
            }

            
            
        }

        [HttpGet]
        public IActionResult DownloadFile(long jobId)
        {
            var response = _businessAdapterService.GetExcelFile(jobId);

            if (response.Status == StatusEnum.Success && response.Value.BusinessAdapterType == AdapterTypeItemEnum.Excel)
            {
                return File(response.Value.File, "application/xlsx", $"{response.Value.FileName}.xlsx");
            }
            else if(response.Status == StatusEnum.Success && response.Value.BusinessAdapterType == AdapterTypeItemEnum.MATLAB)
            {
                return File(response.Value.File, "application/csv", $"{response.Value.FileName}.csv");
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
