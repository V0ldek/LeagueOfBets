using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BetsWorkerBase.EventQueue
{
    public class RabbitMqObjectEventConsumer<T> : IObjectEventConsumer<T>
    {
        private readonly IConnection _connection;
        protected readonly IModel Model;
        private readonly ILogger _logger;

        public event Handler<T> Event;

        public ushort PrefetchLimit
        {
            get => _prefetchLimit;
            set
            {
                _prefetchLimit = value;
                Model.BasicQos(0, _prefetchLimit, global: false);
            }
        }

        private ushort _prefetchLimit;

        public RabbitMqObjectEventConsumer(string hostName, ILogger logger)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName
            };

            _connection = factory.CreateConnection();
            Model = _connection.CreateModel();
            _logger = logger;
        }

        public void BeginConsuming(string exchange, string queue)
        {
            Model.ExchangeDeclare(exchange, "fanout", durable: true);
            Model.QueueDeclare(queue, durable: true);

            Model.QueueBind(queue, exchange, "");
            var basicConsumer = new EventingBasicConsumer(Model);
            basicConsumer.Received += (sender, message) =>
            {
                try
                {
                    HandleReceived(sender, message);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message.AddTimestamp());
                }
            };

            Model.BasicConsume(queue, autoAck: false, basicConsumer);
        }

        public virtual void Ack(ulong deliveryTag) => Model.BasicAck(deliveryTag, false);

        protected void Raise(T arg, ulong deliveryTag) => Event?.Invoke(arg, deliveryTag);

        protected virtual void HandleReceived(object sender, BasicDeliverEventArgs message)
        {
            Raise(Parse(message), message.DeliveryTag);
            Model.BasicAck(message.DeliveryTag, false);
        }

        protected static T Parse(BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _connection?.Dispose();
            Model?.Dispose();
        }
    }
}