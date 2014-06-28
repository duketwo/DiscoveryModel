using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveModel
{
	public class EveMe
	{
		public double MaxLockedTargets
		{
			get
			{
				return Frame.Client.GodmaService.CallMethod("GetItem", new object[] {Frame.Client.Session.CharId })["maxLockedTargets"].GetValueAs<double>();
				
			}
		}
		
		private DateTime getRemainingSubscriptionTime;
		public DateTime GetRemainingSubscriptionTime {
			get {
				
				var subsEndDict =  Frame.Client.Builtin["uicore"]["layer"]["charsel"]["subscriptionEndTimes"].GetDictionary<int>();
				
				if(getRemainingSubscriptionTime != DateTime.MinValue)
					return getRemainingSubscriptionTime;
				
				getRemainingSubscriptionTime = DateTime.MinValue;
				
				foreach(KeyValuePair<int,EveObject> kv in subsEndDict){
					
					var d = kv.Value.GetValueAs<DateTime>();
					if(d >= getRemainingSubscriptionTime)
						getRemainingSubscriptionTime = d;
				}
				
				if(getRemainingSubscriptionTime <= DateTime.UtcNow)
					getRemainingSubscriptionTime = DateTime.UtcNow.AddDays(31);
				
				return getRemainingSubscriptionTime;
			}
		}
		
		private bool DisableResourceLoading {
			get {
				try {
					
					
					return
						Frame.Client.Import("trinity")["device"]["disableGeometryLoad"].GetValueAs<bool>()
						&&	Frame.Client.Import("trinity")["device"]["disableEffectLoad"].GetValueAs<bool>()
						&& Frame.Client.Import("trinity")["device"]["disableTextureLoad"].GetValueAs<bool>();
					//&& Frame.Client.Import("trinity")["device"]["DisableResourceLoad"].GetValueAs<bool>();
				} catch (Exception) {
					
					return false;
				}
			}
			
			set{
				try {
					
					
					Frame.Client.Import("trinity")["device"].SetValueAs<bool>("disableGeometryLoad",value);
					Frame.Client.Import("trinity")["device"].SetValueAs<bool>("disableEffectLoad",value);
					Frame.Client.Import("trinity")["device"].SetValueAs<bool>("disableTextureLoad",value);
					//Frame.Client.Import("trinity")["device"].SetValueAs<bool>("DisableResourceLoad",value);
				} catch (Exception) {
					
					
				}
			}
		}
		
//		public void GiveCash(int charId, double amount){
//			Frame.Client.
//		}

	}
	

}
