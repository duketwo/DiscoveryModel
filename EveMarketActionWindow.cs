/*
 * ---------------------------------------
 * User: duketwo
 * Date: 16.12.2013
 * Time: 00:34
 * 
 * ---------------------------------------
 */
using System;
using System.Linq;
using System.Collections.Generic;

namespace EveModel
{
	/// <summary>
	/// Description of EveMarketActionWindow.
	/// </summary>
	public class EveMarketActionWindow : EveWindow
	{
		
		public new bool IsReady { get; private set; }
		public bool IsBuyAction { get; private set; }
		public bool IsSellAction { get; private set; }
		public EveItem Item { get; private set; }
		public EveOrder Order { get; private set; }
		public double? Price { get; private set; }
		public double? VolumenRemaining { get; private set; }
		public int? Range { get; private set; }
		public long? OrderId { get; private set; }
		public int? VolumeEntered { get; private set; }
		public DateTime IssuedOn;
		public int? MinimumVolume { get; private set; }
		public bool? IsBid { get; private set; }
		public DateTime? Issued { get; private set; }
		public int? Duration { get; private set; }
		public long? StationId { get; private set; }
		public long? RegionId { get; private set; }
		public long? SolarSystemId { get; private set; }
		public int? Jumps { get; private set; }
		
		public EveMarketActionWindow(IntPtr ptr) : base()
		{
			this.PointerToObject = ptr;

			
			this.IsReady = this["ready"].GetValueAs<bool>();
			this.IsBuyAction = (base.Name == "marketbuyaction");
			this.IsSellAction = (base.Name == "marketsellaction");

			this.Item = null;
			if(this["sr"]["sellItem"].IsValid && this.IsSellAction && !this.IsBuyAction && !this.isAdvanced)
			{
				
				this.Item = new EveItem(this["sr"]["sellItem"]);
			}
			
			
			this.Order = null;
			if(this["sr"]["currentOrder"].IsValid && this.IsBuyAction && !this.IsSellAction && !this.isAdvanced)
			{
				
				this.Order = new EveOrder(this["sr"]["currentOrder"].PointerToObject);
			}
			
			
			this.Price = this["sr"]["price"].GetValueAs<double>();
			this.OrderId = -1L;
			this.OrderId = this["sr"]["orderID"].GetValueAs<long>();
			this.VolumeEntered = this["sr"]["volEntered"].GetValueAs<int>();
			this.MinimumVolume = this["sr"]["minVolume"].GetValueAs<int>(); // minVolume
			this.IsBid =  this["sr"]["bid"].GetValueAs<bool>(); // bid
			try { this.IssuedOn = new DateTime(this["sr"]["issueDate"].GetValueAs<long>()).AddYears(1600); } catch (Exception) {} // issued
			this.Duration = this["sr"]["duration"].GetValueAs<int>(); // duration
			this.StationId = this["sr"]["stationID"].GetValueAs<int>(); // stationID
			this.RegionId = this["sr"]["regionID"].GetValueAs<int>(); // regionID
			this.SolarSystemId = this["sr"]["solarSystemID"].GetValueAs<int>(); // solarSystemID
			this.Jumps = this["sr"]["jumps"].GetValueAs<int>();// jumps
			this.Price = this["sr"]["price"].GetValueAs<double>(); // price
			this.VolumenRemaining = this["sr"]["volRemaining"].GetValueAs<int>(); // volRemaining
			this.Range= this["sr"]["range"].GetValueAs<int>();
			
			
			
		}
		public bool isAdvanced{
			get { return this["sr"]["price"]["GetValue"].IsValid ? true : false; }
		}
		
		public bool Accept()
		{
			string attribute = this.IsBuyAction ? "Buy" : "Sell";
			return this.CallMethod(attribute, new object[] {},true).GetValueAs<bool>();
		}
		public bool Cancel()
		{
			return this.CallMethod("Cancel", new object[] {},true).GetValueAs<bool>();
		}
		
		public void SetAdvanced(){
			if(IsBuyAction && this.Order.IsValid && this.IsReady) {
				//Frame.Log("IsBuyAction - call GoPlaceBuyOrder" + this.Order.TypeId.ToString() + " " + this.Order.StationId.ToString());
				// func=self.GoPlaceBuyOrder, args=(typeID,order,0,locationID)
				if(this.IsValid) this.CallMethod("GoPlaceBuyOrder", new object[] { this.Order.TypeId, this.Order, 0, Order.StationId },false) ;
			}
			if (IsSellAction && this.Item.IsValid && this.IsReady){
				//func=self.GoPlaceSellOrder, args=invItem
				//Frame.Log("IsBuyAction - call GoPlaceSellOrder");
				if (this.IsValid) this.CallMethod("GoPlaceSellOrder", new object[] { this.Item }, false);
			}
			
		}
		public double GetPrice(){
			return isAdvanced && this["sr"]["price"].IsValid ? this["sr"]["price"].CallMethod("GetValue", new object[] {}, false).GetValueAs<double>() : -1.0;
		}
		
		public bool SetPrice(double newPrice){
			return  isAdvanced && this["sr"]["price"].IsValid ? this["sr"]["price"].CallMethod("SetValue", new object[] { newPrice }, false).GetValueAs<bool>() : false ;
		}

		public bool SetQuantity(int quantity){
			return  isAdvanced && this["sr"]["quantity"].IsValid ? this["sr"]["quantity"].CallMethod("SetValue", new object[] { quantity }, false).GetValueAs<bool>() : false;
		}
		
		public int GetQuantity(){
			return isAdvanced && this["sr"]["quantity"].IsValid ? this["sr"]["quantity"].CallMethod("GetValue", new object[] {}, false).GetValueAs<int>() : -1;
		}
		/// <summary>
		/// can only be set to 1, 14 or 90
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public bool SetDuration(int duration){
			if(duration == 1 || duration == 90 ||duration == 14) {
			return  isAdvanced && this["sr"]["duration"].IsValid ? this["sr"]["duration"].CallMethod("SetValue", new object[] { duration }, true).GetValueAs<bool>() : false;
			} else {
				return false;
			}
		}
		
		public int GetDuration(){
			return isAdvanced && this["sr"]["duration"].IsValid ? this["sr"]["duration"].CallMethod("GetValue", new object[] {}, false).GetValueAs<int>() : -1;
		}
		
		/// <summary>
		/// can only be set to -1 ( station ) or 32767 ( region )
		/// </summary>
		/// <param name="range"></param>
		/// <returns></returns>
		public bool SetOrderRange(int range){
			// rangeStation = -1 // rangeRegion = 32767
			if(range == -1 || range == 32767) {
			return  isAdvanced && this["sr"]["duration"].IsValid ? this["sr"]["range"].CallMethod("SetValue", new object[] { range }, true).GetValueAs<bool>() : false;
			} else {
				return false;
			}
		}
		
		public int GetOrderRange(){
			return isAdvanced && this["sr"]["range"].IsValid ? this["sr"]["range"].CallMethod("GetValue", new object[] {}, false).GetValueAs<int>() : -1;
		}
	}
}
