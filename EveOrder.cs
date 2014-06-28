/*
 * ---------------------------------------
 * User: duketwo
 * Date: 16.12.2013
 * Time: 00:16
 * 
 * ---------------------------------------
 */
using System;
using System.Collections.Generic;

namespace EveModel
{
	/// <summary>
	/// Description of EveOrder.
	/// </summary>
	/// 
	public enum OrderRange
	{
		Station,
		SolarSystem,
		Constellation,
		Region
	}
	
	public class EveOrder : EveObject
	{
		//public long? BookmarkId { get; private set; }
		public int? Jumps { get; set; }
		public int? SolarSystemId { get; set; }
		public int? RegionId { get; set; }
		public int? StationId { get; set; }
		public int? Duration { get; set; }
		public DateTime IssuedOn { get; set; }
		public bool IsBid { get; set; }
		public int? MinimumVolume { get; set; }
		public int? VolumeEntered { get; set; }
		public long? OrderId { get; set; }
		public OrderRange Range { get; set; }
		public int? RangeAbsolute { get; set; }
		public int? TypeId { get; set; }
		public int? VolumeRemaining { get; set; }
		public double? Price { get; set; }
		
		public EveOrder(IntPtr ptr, Boolean isMyOrder = false) : base()
		{
			PointerToObject = ptr;
			if(this.IsValid) {
				this.OrderId = -1L;
				this.OrderId = this["orderID"].GetValueAs<long>();
				this.VolumeEntered = this["volEntered"].GetValueAs<int>();
				this.MinimumVolume = this["minVolume"].GetValueAs<int>(); // minVolume
				this.IsBid =  this["bid"].GetValueAs<bool>(); // bid
				try { this.IssuedOn = new DateTime(this["issueDate"].GetValueAs<long>()).AddYears(1600); } catch (Exception) {} // issued
				this.Duration = this["duration"].GetValueAs<int>(); // duration
				this.StationId = this["stationID"].GetValueAs<int>(); // stationID
				this.RegionId = this["regionID"].GetValueAs<int>(); // regionID
				this.SolarSystemId = this["solarSystemID"].GetValueAs<int>(); // solarSystemID
				if(!isMyOrder) this.Jumps = this["jumps"].GetValueAs<int>();// jumps
				this.Price = this["price"].GetValueAs<double>(); // price
				this.VolumeRemaining = this["volRemaining"].GetValueAs<int>(); // volRemaining
				this.TypeId = this["typeID"].GetValueAs<int>(); //  typeID
				
				
				int tmpRange = this["range"].GetValueAs<int>();
				if(tmpRange == 0) {
					this.Range = OrderRange.SolarSystem;
				} else {
					if(tmpRange == 4) {
						this.Range = OrderRange.Constellation;
					} else {
						if(tmpRange == 32767) {
							this.Range = OrderRange.Region;
						} else {
							if(tmpRange == -1) {
								this.Range = OrderRange.Station;
							} else {
								this.RangeAbsolute = tmpRange;
							}
						}
					}
				}
			} else {
				Frame.Log("eveorder not valid");
			}
		}
		
		private Dictionary<long?,DateTime> _lastOrderIdModified;
		private Dictionary<long?,DateTime> LastOrderIdModified {
			get{
				if(_lastOrderIdModified == null)
					_lastOrderIdModified = new Dictionary<long?, DateTime>();
				return _lastOrderIdModified;
			}
			set {
				_lastOrderIdModified = value;
			}
		}
		
		public bool CancelOrder(){
			
			bool result= false;
			if (this.OrderId == -1L || !this.IsValid)
			{
				Frame.Log("Trying to cancel an invalid order");
				result = false;
			}
			else
			{
				if(CanBeModified) {
					LastOrderIdModified.Remove(this.OrderId);
					result = Frame.Client.MarketQuote.CallMethod("CancelOrder",  new object[] { this.OrderId,this.RegionId }, true).GetValueAs<bool>();
					IssuedOn = DateTime.UtcNow;
				} else {
					Frame.Log("[CancelOrder] Can't be modified < 5 minutes");
				}
			}
			
			return result;
			
		}
		public bool CanBeModified{
			get {
				if(!LastOrderIdModified.ContainsKey(this.OrderId)){
					return DateTime.UtcNow >= (this.IssuedOn.AddMinutes(5));
				}
				else {
					return DateTime.UtcNow >= (this.IssuedOn.AddMinutes(5)) &&(LastOrderIdModified[this.OrderId].AddMinutes(5) < DateTime.UtcNow);
				}
			}
		}
		
		public bool ModifyOrder(double? newPrice)
		{
			bool result= false;
			if (this.OrderId == -1L || !this.IsValid || newPrice == null || this.StationId != Frame.Client.Session.StationId){
				Frame.Log("[ModifyOrder] Trying to modify an invalid order || newPrice == null || this.StationId != Frame.Client.Session.StationId");
				result = false;
				
			}
			else
			{
				if(CanBeModified) {
					LastOrderIdModified[this.OrderId] = DateTime.UtcNow;
					result = Frame.Client.MarketQuote.CallMethod("ModifyOrder",  new object[] { this,newPrice }, true).GetValueAs<bool>();
					IssuedOn = DateTime.UtcNow;
				} else {
					Frame.Log("[ModifyOrder] can't be modified < 5 minutes");
				}
			}
			
			return result;
		}

		public bool Buy(int quantity)
		{
			return Frame.Client.MarketQuote.CallMethod("BuyStuff", new object[]{this.StationId,this.TypeId,this.Price,quantity,(int)-1},true).GetValueAs<bool>();
		}
		
	}
}
