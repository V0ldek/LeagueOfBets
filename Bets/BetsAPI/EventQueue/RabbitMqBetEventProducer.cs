using System;
using System.Text;
using BetsData.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace BetsAPI.EventQueue
{
    internal sealed class RabbitMqBetEventProducer : IBetEventProducer, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;

        public RabbitMqBetEventProducer(string hostName)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName
            };

            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();

            _model.ExchangeDeclare("bet_new", "fanout", durable: true);
        }

        public void PublishNewBet(Bet bet)
        {
            var properties = new BasicProperties
            {
                Persistent = true
            };
            var body = Serialize(bet);

            _model.BasicPublish("bet_new", "", properties, body);
        }

        private static byte[] Serialize(Bet bet)
        {
            var json = JsonConvert.SerializeObject(new
            {
                bet.Amount,
                bet.StakeId
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
