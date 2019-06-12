using System;
using System.Text;
using RabbitMQ.Client;
using MatchesData.Entities;
using MatchesData.Entities.Enums;
using Newtonsoft.Json;
using RabbitMQ.Client.Framing;

namespace MatchesWorker.EventQueue
{
    internal sealed class RabbitMqMatchEventProducer : IMatchEventProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;

        public RabbitMqMatchEventProducer(string hostName)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName
            };

            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.ExchangeDeclare("match_new", "fanout", durable: true);
            _model.ExchangeDeclare("match_update", "fanout", durable: true);
        }

        public void PublishNewMatch(Match match)
        {
            var properties = new BasicProperties
            {
                Persistent = true
            };
            var body = Serialize(match);

            _model.BasicPublish("match_new", "", properties, body);
        }

        public void PublishMatchUpdate(Match match)
        {
            var properties = new BasicProperties
            {
                Persistent = true
            };
            var body = Serialize(match);

            _model.BasicPublish("match_update", "", properties, body);
        }

        private static byte[] Serialize(Match match)
        {
            var json = JsonConvert.SerializeObject(new
            {
                match.Id,
                match.BestOf,
                match.BlueScore,
                match.RedScore
            });
            return Encoding.UTF8.GetBytes(json);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _model?.Dispose();
        }
    }
}
