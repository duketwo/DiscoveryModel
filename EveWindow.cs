using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EveModel
{
	public class EveWindow : EveObject
	{
		public enum Button
		{
			Accept,
			Browse,
			Close,
			CompleteMission,
			Decline,
			Delay,
			LocateCharacter,
			RequestMission,
			ViewMission,
			MarketActionWindowAdvanced,
			Yes,
			No
		}
		
		string GetButtonText(Button btn)
		{
			string btnText = string.Empty;
			switch (btn)
			{
				case Button.Accept:
					btnText = "Accept";
					break;
				case Button.Yes:
					btnText = "Yes";
					break;
				case Button.No:
					btnText = "No";
					break;
				case Button.Browse:
					btnText = "Browse";
					break;
				case Button.Close:
					btnText = "Close";
					break;
				case Button.CompleteMission:
					btnText = "Complete Mission";
					break;
				case Button.Decline:
					btnText = "Decline";
					break;
				case Button.Delay:
					btnText = "Delay";
					break;
				case Button.LocateCharacter:
					btnText = "Locate Character";
					break;
				case Button.RequestMission:
					btnText = "Request Mission";
					break;
				case Button.ViewMission:
					btnText = "View Mission";
					break;
				case Button.MarketActionWindowAdvanced:
					btnText = "Advanced  >>";
					break;
			}
			return btnText;
		}
		
		//ID_NONE = 0
		//ID_OK = 1
		//ID_CANCEL = 2
		//ID_YES = 6
		//ID_NO = 7
		//ID_CLOSE = 8
		//ID_HELP = 9
		
		public enum ModalResultType {
			NONE,
			OK,
			CANCEL,
			YES,
			NO,
			CLOSE,
			HELP
		}
		
		public int GetModalResult(ModalResultType mr){
			int modalResult = 0;
			switch(mr) {
				case ModalResultType.NONE:
					modalResult = 0;
					break;
				case ModalResultType.OK:
					modalResult = 1;
					break;
				case ModalResultType.CANCEL:
					modalResult = 2;
					break;
				case ModalResultType.YES:
					modalResult = 6;
					break;
				case ModalResultType.NO:
					modalResult = 7;
					break;
				case ModalResultType.CLOSE:
					modalResult = 8;
					break;
				case ModalResultType.HELP:
					modalResult = 9;
					break;
			}
			
			return modalResult;
		}
		
		
		
		public void ClickButton(Button btn)
		{
			if (!HasButtons) {
				Frame.Log("has no buttons");
				return;
			}
			var button = this["buttonGroup"].CallMethod("GetBtnByLabel", new object[] { GetButtonText(btn) });
			if (button != null && button.IsValid)
			{
				button.CallMethod("OnClick", new object[0], true);
			}
		}
		public bool HasButton(Button btn)
		{
			if (!HasButtons)
				return false;
			var btnExists = this["buttonGroup"].CallMethod("GetBtnByLabel", new object[] { GetButtonText(btn) })["state"].IsValid;
			if (PyCall.PyErr_Occurred() != IntPtr.Zero)
				PyCall.PyErr_Clear();
			
			return btnExists;
		}
		public void Close()
		{
			this.CallMethod("CloseByUser", new object[0], true);
		}
		
		public enum EveWindowType
		{
			AgentDialog,
			Assets,
			Chat,
			Drones,
			FittingWindow,
			Inventory,
			LootContainer,
			Overview,
			PopUp,
			SelectedItem,
			Scanner,
			Stack,
			StationServices,
			Telecom,
			Market,
			MarketActionWindow,
			Journal,
			Wallet,
			CharacterSheet,
			Unknown
		}
		public bool IsReady
		{
			get { return this["_open"].GetValueAs<bool>(); }
		}

		public int Id
		{
			get { return this["windowID"].GetValueAs<int>(); }
		}
		public string Guid
		{
			get { return this["__guid__"].GetValueAs<string>(); }
		}
		public string Name
		{
			get { return this["name"].GetValueAs<string>(); }
		}

		public bool HasButtons {
			get {
				return this["buttonGroup"].IsValid;
			}
		}

		public void SuppressPopUpMessage()
		{
			if (this.Type == EveWindowType.PopUp && this["sr"]["suppCheckbox"] != null && this["sr"]["suppCheckbox"].IsValid)
			{
				this["sr"]["suppCheckbox"].CallMethod("SetChecked", new object[] { 1 });
			}
		}
		public string GetPopUpMessage
		{
			get { return this.Type == EveWindowType.PopUp ? this["edit"].CallMethod("GetValue", new object[0]).GetValueAs<string>() : string.Empty; }
		}
		public bool IsKillable
		{
			get { return this["_killable"].GetValueAs<bool>(); }
		}
		public bool IsDialog
		{
			get { return this["isDialog"].GetValueAs<bool>(); }
		}
		
		public void SetModalResult(ModalResultType mr){
			int modalResult = GetModalResult(mr);
			this.CallMethod("SetModalResult", new object[] { modalResult },true);
		}
		
		public bool IsModal
		{
			get { return this["isModal"].GetValueAs<bool>(); }
		}
		public string Caption
		{
			get { return this.CallMethod("GetCaption", new object[0]).GetValueAs<string>(); }
		}
		public string WindowCaption
		{
			get { return this["windowCaption"].GetValueAs<string>(); }
		}
		public EveWindowType Type
		{
			get
			{
				EveWindowType windowType = EveWindowType.Unknown;
				switch (Guid)
				{
						case "form.MarketActionWindow": windowType = EveWindowType.MarketActionWindow; break;
						case "form.RegionalMarket": windowType = EveWindowType.Market; break;
						case "form.ActiveItem": windowType = EveWindowType.SelectedItem; break;
						case "form.AgentDialogueWindow": windowType = EveWindowType.AgentDialog; break;
						case "form.AssetsWindow": windowType = EveWindowType.Assets; break;
						case "form.InventoryPrimary": windowType = EveWindowType.Inventory; break;
						case "form.Inventory": windowType = EveWindowType.Inventory; break;
						case "form.Lobby": windowType = EveWindowType.StationServices; break;
						case "form.LSCChannel": windowType = EveWindowType.Chat; break;
						case "form.LSCStack": windowType = EveWindowType.Stack; break;
						case "form.MessageBox": windowType = EveWindowType.PopUp; break;
						case "form.OverView": windowType = EveWindowType.Overview; break;
						case "form.Scanner": windowType = EveWindowType.Scanner; break;
						case "form.LootCargoView": windowType = EveWindowType.LootContainer; break;
						case "form.DroneView": windowType = EveWindowType.Drones; break;
						case "form.FittingWindow": windowType = EveWindowType.FittingWindow; break;
						case "form.Telecom": windowType = EveWindowType.Telecom; break;
						case "form.Journal": windowType = EveWindowType.Journal; break;
						case "form.Wallet": windowType = EveWindowType.Wallet; break;
						case "form.CharacterSheet": windowType = EveWindowType.CharacterSheet; break;
				}
				return windowType;
			}
		}
		public EveInventoryContainer ToInventoryContainer
		{
			get { return new EveInventoryContainer(this); }
		}
		public EveMarketWindow ToMarketWindow
		{
			get {
				if (this.Guid == "form.RegionalMarket")
					return new EveMarketWindow(this.PointerToObject);
				else return null;
			}

		}
		
		public EveMarketActionWindow ToMarketActionWindow
		{
			get {
				if (this.Guid == "form.MarketActionWindow")
					return new EveMarketActionWindow(this.PointerToObject);
				else return null;
			}

		}
		
		public EveChatWindow ToChatWindow{
			get {
				if (this.Guid == "form.LSCChannel")
					return new EveChatWindow() { PointerToObject = this.PointerToObject };
				else return null;
				
			}
		}
	}
}

