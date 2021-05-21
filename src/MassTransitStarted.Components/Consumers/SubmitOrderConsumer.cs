using MassTransit;
using MassTransitStarted.Contracts;
using System.Threading.Tasks;

namespace MassTransitStarted.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            var submitOrder = context.Message;

            if (context.Message.CustomerNumber.Contains("TEST"))
            {
                await context.RespondAsync<OrderSubmissionRejected>(new
                {
                    InVar.Timestamp,
                    OrderId = submitOrder.Id,
                    CustomerNumber = submitOrder.CustomerNumber,
                    Reason = $"Test Customer cannot submit orders: {submitOrder.CustomerNumber}"
                });
                return;
            }

            await context.RespondAsync<OrderSubmissionAccepted>(new
            {
                InVar.Timestamp,
                OrderId = submitOrder.Id,
                CustomerNumber = submitOrder.CustomerNumber
            });
        }
    }
}
