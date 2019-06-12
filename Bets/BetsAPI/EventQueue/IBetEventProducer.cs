using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BetsData.Entities;

namespace BetsAPI.EventQueue
{
    public interface IBetEventProducer
    {
        void PublishNewBet(Bet bet);
    }
}
