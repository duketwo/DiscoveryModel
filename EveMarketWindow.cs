/*
 * ---------------------------------------
 * User: duketwo
 * Date: 16.12.2013
 * Time: 00:33
 * 
 * ---------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EveModel
{
	/// <summary>
	/// Description of EveMarketWindow.
	/// </summary>
	public class EveMarketWindow : EveWindow
	{
		private static DateTime lastLoadOders = DateTime.MinValue;
		private static DateTime lastLoadPriceHistory = DateTime.MinValue;
		private static Random rnd = new Random();
		
		public new bool IsReady { get; internal set; }
		
		public int? DetailTypeId { get; internal set; }
		
		public IEnumerable<EveOrder> SellOrders
		{
			get { try { return Frame.Client.MarketQuote["orderCache"].GetDictionary<int>().FirstOrDefault(e => e.Key == this.DetailTypeId).Value.GetList<EveObject>().ElementAt(0).GetList<EveObject>().Select(new Func<EveObject,EveOrder>(this.EveObjectToEveOrder)); } catch (Exception) { return new List<EveOrder>();  } }
		}
		public IEnumerable<EveOrder> BuyOrders
		{
			get { try { return Frame.Client.MarketQuote["orderCache"].GetDictionary<int>().FirstOrDefault(e => e.Key == this.DetailTypeId).Value.GetList<EveObject>().ElementAt(1).GetList<EveObject>().Select(new Func<EveObject,EveOrder>(this.EveObjectToEveOrder)); } catch (Exception) { return new List<EveOrder>();  } }
		}
		
		
		public EveMarketWindow(IntPtr ptr) : base() {
			this.PointerToObject = ptr;
			this.IsReady = !this["sr"]["market"]["loadingType"].GetValueAs<bool>();
			this.DetailTypeId = this["sr"]["market"]["sr"]["detailTypeID"].GetValueAs<int>();
		}
		
		public void LoadTypeId(int typeId) {
			this["sr"]["market"].CallMethod("LoadTypeID_Ext", new object[]{ typeId },true);
		}
		
		
		public List<EveOrder> GetMySellOrders {
			get {
				try {
				EveObject obj = this["sr"]["market"]["sr"]["myorders"]["sr"]["sellScroll"]["sr"]["entries"];
				return (obj.IsValid) ? obj.GetList<EveObject>().Select(new Func<EveObject,EveOrder>(this.EveObjectOrderAttrToEveOrder)).ToList() : new List<EveOrder>();	
				} catch (Exception e) {
					Frame.Log("[GetMySellOrders] - Exception: " + e.ToString());
					return new List<EveOrder>();
				}
			}
		}
		
		public List<EveOrder> GetMyBuyOrders {
			get {
				try {
				EveObject obj = this["sr"]["market"]["sr"]["myorders"]["sr"]["buyScroll"]["sr"]["entries"];
				return (obj.IsValid) ? obj.GetList<EveObject>().Select(new Func<EveObject,EveOrder>(this.EveObjectOrderAttrToEveOrder)).ToList() : new List<EveOrder>();
				} catch (Exception e) {
					Frame.Log("[GetMyBuyOrders] - Exception: " + e.ToString());
					return new List<EveOrder>();
				}
			}
		}
		
		public List<EvePriceHistory> GetPriceHistory {
			
			get {
				try {
					
					EveObject eobj = this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["typerecord"];
					if(eobj.IsValid && this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["typerecord"]["typeID"].GetValueAs<int>() != this.DetailTypeId) return new List<EvePriceHistory>();
					
					EveObject obj = this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["children"]["_childrenObjects"];
					return (obj.IsValid) ? obj.GetList<EveObject>().ElementAt(1)["sr"]["entries"].GetList<EveObject>().Select(new Func<EveObject,EvePriceHistory>(this.EveObjectToEvePriceHistory)).ToList() : new List<EvePriceHistory>();
					
				} catch (Exception e) {
					Frame.Log("GetPriceHistory - Exception: " + e.ToString());
					return new List<EvePriceHistory>();
				}
			}
		}
		
		public bool MyOrderInited {
			get { return this["sr"]["market"]["ordersInited"].GetValueAs<bool>(); }
		}
		
		public bool PriceHistoryInited {
			get { return this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["optionsinited"].GetValueAs<bool>(); }
		}
		
		public bool PriceHistoryRendering {
			get { return this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["rendering"].GetValueAs<bool>(); }
		}
		
		public bool IsPriceHistoryTabLoadedOnce {
			get { return this["sr"]["market"]["sr"]["pricehistory"]["sr"]["pricehistory"]["rendering"].IsValid; }
		}
		
		public void SelectByIdx(int tab){
			this["sr"]["market"]["sr"]["detailtabs"].CallMethod("SelectByIdx", new object[] { tab },true);
		}
		
		public int GetSelectedIdx {
			
			get { return this["sr"]["market"]["sr"]["detailtabs"].CallMethod("GetSelectedIdx", new object[0] {}).GetValueAs<int>(); }
		}
		
		public bool IsMarketDataTabSelected {
			get { return GetSelectedIdx == 0 ? true : false; }
		}
		
		public bool IsPirceHistoryTabSelected {
			get { return GetSelectedIdx == 1 ? true : false; }
		}
		
		public void SelectPriceHistoryTab() {
			
			SelectByIdx(1);
		}
		
		public void SelectMarketDataTab() {
			
			SelectByIdx(0);
		}
		
//		limits['cnt'] = maxOrderCount
//		limits['fee'] = commissionPercentage
		//        limits['acc'] = transactionTax
		//        limits['ask'] = jumpsPerSkillLevel[marketingLevel]
		//        limits['bid'] = jumpsPerSkillLevel[procurementLevel]
		//        limits['vis'] = jumpsPerSkillLevel[visibilityLevel]
		//        limits['mod'] = jumpsPerSkillLevel[daytradingLevel]
		//        limits['esc'] = 0.75 ** marginTradingLevel
		
		public static int? OrderLimit  { get; private set; }
		public static double? TransactionTax  { get; private set; }
		public static double? BrokerFee  { get; private set; }
		public static double? Escrow  { get; private set; }
		private static Dictionary<string,EveObject> _getSkillLimits;
		public Dictionary<string,EveObject> GetSkillLimits {
			get {
				if(_getSkillLimits == null){
					_getSkillLimits = Frame.Client.MarketQuote.CallMethod("GetSkillLimits", new object[0] {}).GetDictionary<string>();
					OrderLimit =  _getSkillLimits["cnt"].GetValueAs<int>();
					TransactionTax = _getSkillLimits["acc"].GetValueAs<double>();
					BrokerFee = _getSkillLimits["fee"].GetValueAs<double>();
					Escrow = _getSkillLimits["esc"].GetValueAs<double>();
				}
				return _getSkillLimits;
			}
		}
		
		private int loadOrderDelay = 7;
		public bool LoadOrders(){
			if(DateTime.UtcNow > lastLoadOders.AddSeconds(loadOrderDelay) && this.IsReady){
				loadOrderDelay = rnd.Next(5,10);
				lastLoadOders = DateTime.UtcNow;
				this["sr"]["market"].CallMethod("LoadOrders", new object[] {},true);
				return true;
			} else {
				Frame.Log("[LoadOrders] - not ready or can't refresh orders again that fast.");
				return false;
			}
		}
		
		
		public bool ClickCreateBuyOrder(){
			
			return (this.IsValid && this.IsReady) ? Frame.Client.GetService("marketutils").CallMethod("Buy", new object[] { this.DetailTypeId },true).GetValueAs<bool>() : false;
		}
		
		private EveOrder EveObjectToEveOrder(EveObject eveObject)
		{
			return new EveOrder(eveObject.PointerToObject);
		}
		
		private EvePriceHistory EveObjectToEvePriceHistory(EveObject eveObject)
		{
			return new EvePriceHistory(eveObject.PointerToObject);
		}
		
		private EveOrder EveObjectOrderAttrToEveOrder(EveObject eveObject)
		{
			return new EveOrder(eveObject["order"].PointerToObject, true);
		}
		
	}
}
