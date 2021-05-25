using MassTransit;
using MassTransitStarted.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace MassTransitStarted.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<SubmitOrder> _submitOrderRequest;
        private readonly ISendEndpointProvider _sendEndpoint;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> submitOrderRequest, ISendEndpointProvider sendEndpoint)
        {
            _logger = logger;
            _submitOrderRequest = submitOrderRequest;
            _sendEndpoint = sendEndpoint;
        }

        [HttpPost]
        [SwaggerResponse(202, type: typeof(OrderSubmissionAccepted))]
        [SwaggerResponse(400, type: typeof(OrderSubmissionRejected))]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            _logger.LogInformation($"Receive post submit order: {id} - {customerNumber}");

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
            }
            else
            {
                var response = await rejected;
                return BadRequest(response.Message);
            }
        }

        [HttpPut]
        [SwaggerResponse(202, type: typeof(AcceptedResult))]
        [SwaggerResponse(400, type: typeof(BadRequestResult))]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            _logger.LogInformation($"Receive put submit order: {id} - {customerNumber}");
            var endpoint = await _sendEndpoint.GetSendEndpoint(new Uri($"exchange:submit-order"));
            await endpoint.Send<SubmitOrder>(new
            {
                Id = id,
                Timestamp = InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }

    }
}
