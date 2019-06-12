using System;

namespace BetsWorkerBase.EventQueue
{
    public delegate void Handler<in TObject>(TObject @object, ulong deliveryTag);

    public interface IObjectEventConsumer<out TObject> : IDisposable
    {
        event Handler<TObject> Event;

        void BeginConsuming(string exchange, string queue);

        void Ack(ulong deliveryTag);

        ushort PrefetchLimit { get; set; }
    }
}
