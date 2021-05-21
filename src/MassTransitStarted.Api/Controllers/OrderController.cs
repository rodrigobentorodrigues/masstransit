using MassTransit;
using MassTransitStarted.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MassTransitStarted.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<SubmitOrder> _submitOrderRequest;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> submitOrderRequest)
        {
            _logger = logger;
            _submitOrderRequest = submitOrderRequest;
        }

        [HttpPost]
        [SwaggerResponse(HttpStatusCode.Accepted, type: typeof(OrderSubmissionAccepted))]
        [SwaggerResponse(HttpStatusCode.BadRequest, type: typeof(OrderSubmissionRejected))]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            _logger.LogInformation($"Receive submit order: {id} - {customerNumber}");

            var (accepted, rejected) = await _submitOrderRequest.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
            {
                Id = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Accepted(response.Message);
            } else
            {
                var response = await rejected;
                return BadRequest(response.Message);
            }
        }

    }
}
