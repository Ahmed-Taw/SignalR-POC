using Microsoft.AspNetCore.SignalR;
using SignalR_POC.StockMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SignalR_POC.Hubs
{
	public class StockHub : Hub
	{
		private readonly StockTicker _stockTicker;

		public StockHub(StockTicker  stockTicker)
		{
			_stockTicker = stockTicker;
		}

		public IEnumerable<Stock> GetAllStocks()
		{
			//Clients.All.SendAsync("marketOpened");
			return _stockTicker.GetAllStocks();
		}

		public string GetMarketState()
		{
			return _stockTicker.GetMarketStatus();
		}
		public ChannelReader<Stock> StreamStocks()
		{
			return _stockTicker.StreamStocks().AsChannelReader(10);
		}

		public async Task OpenMarket()
		{
			await _stockTicker.OpenMarket();
		}

		public async Task CloseMarket()
		{
			await _stockTicker.CloseMarket();
		}

		public async Task Reset()
		{
			await _stockTicker.Reset();
		}

	}
}
