using System;
using BetsData.Entities;

namespace BetsWorker.EventQueue
{
    public delegate void NewMatchHandler(Match match);

    internal interface IMatchEventConsumer : IDisposable
    {
        event NewMatchHandler NewMatch;

        void BeginConsuming();
    }
}
