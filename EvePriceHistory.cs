/*
 * ---------------------------------------
 * User: duketwo
 * Date: 24.12.2013
 * Time: 16:26
 * 
 * ---------------------------------------
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EveModel
{
	/// <summary>
	/// Description of EveOrderData.
	/// </summary>
	public class EvePriceHistory : EveObject
	{
		
		public DateTime Date;
		public int? Orders;
		public long? Quantity;
		public double? Low;
		public double? High;
		public double? Avg;
		
		
		
		//date/orders/quantity/low/high/average
		//public event PropertyChangedEventHandler PropertyChanged;
		
		// sort_Date sort_Orders  sort_Quantity sort_Low sort_High  sort_Avg.

		public EvePriceHistory(IntPtr ptr) : base()
		{
			this.PointerToObject = ptr;
			try {Date = new DateTime(this["sort_Date"].GetValueAs<long>()).AddYears(1600); } catch (Exception) {}
			Orders = this["sort_Orders"].GetValueAs<int>();
			Quantity = this["sort_Quantity"].GetValueAs<long>();
			Low = this["sort_Low"].GetValueAs<double>();
			High = this["sort_High"].GetValueAs<double>();
			Avg = this["sort_Avg."].GetValueAs<double>();
		}
		
		
	}
}
