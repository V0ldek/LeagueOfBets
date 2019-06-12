using System;
using System.Text;
using BetsData.Entities;
using MatchesWorker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BetsWorker.EventQueue
{
    internal sealed class RabbitMqMatchEventConsumer : IMatchEventConsumer
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly EventingBasicConsumer _basicConsumer;

        public event NewMatchHandler NewMatch;

        public RabbitMqMatchEventConsumer(string hostName, ILogger logger)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName
            };

            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.ExchangeDeclare("match_new", "fanout", durable: true);
            _model.QueueDeclare("bets_worker_match_new", durable: true);
            _model.QueueBind("bets_worker_match_new", "match_new", "");
            _basicConsumer = new EventingBasicConsumer(_model);
            _basicConsumer.Received += (sender, message) =>
            {
                try
                {
                    NewMatch?.Invoke(Parse(message));
                    _model.BasicAck(message.DeliveryTag, false);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, exception.Message.AddTimestamp());
                }
            };
        }

        public void BeginConsuming() => _model.BasicConsume("bets_worker_match_new", autoAck: false, _basicConsumer);

        private static Match Parse(BasicDeliverEventArgs args)
        {
            var json = Encoding.UTF8.GetString(args.Body);
            return JsonConvert.DeserializeObject<Match>(json);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _model?.Dispose();
        }
    }
}
