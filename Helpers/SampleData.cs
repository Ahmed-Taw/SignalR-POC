using SignalR_POC.StockMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalR_POC.Helpers
{
	public static class SampleData
	{
		public static List<Stock> Stocks { get; set; } = new List<Stock>
		{
			    new Stock { Symbol = "MICROSOFT", Price = 107.56m },
				new Stock { Symbol = "GOOGLE", Price = 1221.16m },
				new Stock { Symbol = "AMZON", Price = 1444.16m },
				new Stock { Symbol = "MATDINY", Price = 600.16m },
				new Stock { Symbol = "ORASCOM", Price = 121.16m }


		};

		public static double UpdatePricesRangePercentage { get; } = 0.002;
	}
}
