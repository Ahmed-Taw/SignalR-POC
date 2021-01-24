using Microsoft.AspNetCore.SignalR;
using SignalR_POC.Helpers;
using SignalR_POC.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace SignalR_POC.StockMarket
{
	public class StockTicker
	{
		private readonly SemaphoreSlim _marketStateLock = new SemaphoreSlim(1, 1);

		private readonly SemaphoreSlim _UpdateStockPrices = new SemaphoreSlim(1, 1);
		private readonly ConcurrentDictionary<string, Stock> _stocks = new ConcurrentDictionary<string, Stock>();

		private readonly Subject<Stock> _stocksSubject = new Subject<Stock>();



		private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(500);
		private readonly Random _updateOrNotRandom = new Random();

		private Timer _timer;

		private readonly IHubContext<StockHub> _hub;
		private bool marketOpen = false;

		public StockTicker(IHubContext<StockHub> hub)
		{
			this._hub = hub;
			LoadDefaultStocks();

		}
		
		private void LoadDefaultStocks()
		{
			SampleData.Stocks.ForEach(st => _stocks.TryAdd(st.Symbol, st));

		}

		internal IEnumerable<Stock> GetAllStocks()
		{
			return _stocks.Values;
		}

		internal string GetMarketStatus()
		{
			if (marketOpen)
				return "Open";

			return "Closed";
		}

		internal IObservable<Stock> StreamStocks()
		{
			return _stocksSubject;
		}

		internal async Task OpenMarket()
		{
			await _marketStateLock.WaitAsync();
			try
			{
				if (!marketOpen)
				{
					_timer = new Timer(UpdateStockPrices, null, _updateInterval, _updateInterval);
					this.marketOpen = true;
					await _hub.Clients.All.SendAsync("marketOpened");
				}
				
			}
			finally
			{
				_marketStateLock.Release();
			}
		}

		internal async Task CloseMarket()
		{
			await _marketStateLock.WaitAsync();
			try
			{
				if (_timer != null)
					_timer.Dispose();
				this.marketOpen = false;

				await _hub.Clients.All.SendAsync("marketClosed");
			}
			finally
			{
				_marketStateLock.Release();
			}
		}

		internal Task Reset()
		{
			throw new NotImplementedException();
		}

		// this function is a tmer handler should be with this signature 
		public async void UpdateStockPrices(object state)
		{
			await _UpdateStockPrices.WaitAsync();
			try
			{
				foreach (var stock in _stocks.Values)
				{
					TryUpdateStockPrice(stock);
					_stocksSubject.OnNext(stock);
				}
			}
			finally 
            {
				_UpdateStockPrices.Release();
            }

		}

		private bool TryUpdateStockPrice(Stock stock)
		{
			// Randomly choose whether to udpate this stock or not
			var r = _updateOrNotRandom.NextDouble();
			if (r > 0.1)
			{
				return false;
			}

			// Update the stock price by a random factor of the range percent
			var random = new Random((int)Math.Floor(stock.Price));
			var percentChange = random.NextDouble() * SampleData.UpdatePricesRangePercentage;
			var pos = random.NextDouble() > 0.51;
			var change = Math.Round(stock.Price * (decimal)percentChange, 2);
			change = pos ? change : -change;

			stock.Price += change;
			return true;
		}
	}
}
