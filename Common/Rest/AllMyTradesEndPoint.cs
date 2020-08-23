using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common.Models;

namespace Common.Rest
{
    public class AllMyTradesEndPoint
    {
        private readonly MyTradesEndPoint _myTradesEndPoint;

        public AllMyTradesEndPoint(MyTradesEndPoint myTradesEndPoint)
        {
            _myTradesEndPoint = myTradesEndPoint;
        }

        public IObservable<IList<Trade>> GetAllTrades(string symbol)
        {
            return Observable.Create<IList<Trade>>(async t =>
            {
                int lastId = 0;
                while (true)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));

                    var myTrades = await _myTradesEndPoint.GetMyTrades(symbol, lastId + 1);
                    t.OnNext(myTrades);

                    if (myTrades.Count < 1000 || myTrades[^1].Id == lastId)
                        break;

                    lastId = myTrades[^1].Id;
                }

                t.OnCompleted();
            });
        }
    }
}