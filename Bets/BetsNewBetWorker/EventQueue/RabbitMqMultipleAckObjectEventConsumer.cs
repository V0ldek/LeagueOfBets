using BetsWorkerBase.EventQueue;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace BetsNewBetWorker.EventQueue
{
    public class RabbitMqMultipleAckObjectEventConsumer<T> : RabbitMqObjectEventConsumer<T>
    {
        public RabbitMqMultipleAckObjectEventConsumer(string hostName, ILogger logger) : base(hostName, logger)
        {
        }

        public override void Ack(ulong deliveryTag) => Model.BasicAck(deliveryTag, true);

        protected override void HandleReceived(object sender, BasicDeliverEventArgs message) => Raise(Parse(message), message.DeliveryTag);
    }
}