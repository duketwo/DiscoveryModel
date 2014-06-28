/*
 * ---------------------------------------
 * User: duketwo
 * Date: 03.01.2014
 * Time: 19:42
 * 
 * ---------------------------------------
 */
using System;
using System.Linq;
using System.Collections.Generic;

namespace EveModel
{
	/// <summary>
	/// Description of EveLogin.
	/// </summary>
	public class EveLogin : EveObject
	{
		public EveLogin() : base()
		{
			PointerToObject = Frame.Client.Builtin["uicore"]["layer"].PointerToObject;
		}
		
		
		private List<EveLoginSlot> _loginSlots = null;
		
		public bool AtLogin
		{

			get { return Frame.Client.Builtin["uicore"]["layer"]["login"]["isopen"].GetValueAs<bool>() || Frame.Client.Builtin["uicore"]["layer"]["login"]["isopening"].GetValueAs<bool>();  }
		}
		public bool IsConnecting
		{

			get { return Frame.Client.Builtin["uicore"]["layer"]["login"]["connecting"].GetValueAs<bool>(); }
		}
		public bool IsLoading
		{

			get { return Frame.Client.Builtin["uicore"]["layer"]["login"]["isopening"].GetValueAs<bool>() || Frame.Client.Builtin["uicore"]["layer"]["charsel"]["isopening"].GetValueAs<bool>();  }
		}
		public bool AtCharacterSelection
		{
			

			get { return Frame.Client.Builtin["uicore"]["layer"]["charsel"]["isopen"].GetValueAs<bool>() || Frame.Client.Builtin["uicore"]["layer"]["charsel"]["isopening"].GetValueAs<bool>();  }
		}
		public bool IsCharacterSelectionReady
		{

			get { return Frame.Client.Builtin["uicore"]["layer"]["charsel"]["ready"].GetValueAs<bool>(); }
		}
		public string ServerStatus
		{

			get { return Frame.Client.Builtin["uicore"]["layer"]["login"]["serverStatusTextControl"]["text"].GetValueAs<string>(); }
		}
		public List<EveLoginSlot> CharacterSlots
		{

			get {
				if (_loginSlots == null) {
					this._loginSlots = new List<EveLoginSlot>();
					foreach(EveObject obj in Frame.Client.Builtin["uicore"]["layer"]["charsel"]["characterSlotList"].GetList<EveObject>() ){
						this._loginSlots.Add(new EveLoginSlot(obj.PointerToObject));
					}
				}
				return this._loginSlots;
			}
		}

		public bool Login(string username, string password)
		{
						
			Frame.Client.Builtin["uicore"]["layer"]["login"]["usernameEditCtrl"].CallMethod("SetValue", new object[] { username });
			Frame.Client.Builtin["uicore"]["layer"]["login"]["passwordEditCtrl"].CallMethod("SetValue", new object[] { password });
			return Frame.Client.Builtin["uicore"]["layer"]["login"].CallMethod("_Connect", new object[0] {}, true).GetValueAs<bool>();
			
			
		}
		
	}
}
