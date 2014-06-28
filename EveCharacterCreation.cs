/*
* ---------------------------------------
* User: duketwo
* Date: 26.03.2014
* Time: 11:50
* 
* ---------------------------------------
*/
using System;
using System.Collections.Generic;

namespace EveModel
{
	/// <summary>
	/// Description of EveCharacterCreation.
	/// </summary>
	///
	
	//RacesIDs
	//raceCaldari = 1
	//raceMinmatar = 2
	//raceAmarr = 4
	//raceGallente = 8
	
	
	//StepIDs
	// 1 Race
	// 2 Bloodline
	// 3 Customization
	// 4 Portrait
	// 5 Identity
	
	//GenderIDs
	// not set -1
	// female 0
	// male 1
	
	//BloodlineIDs
	// not set -1
	
	public class EveCharacterCreation : EveObject
	{
		
		public enum Race { CALDARI, GALLENTE, AMARR, MINMATAR, NONE };
		private static Random rnd = new Random();
		
		public EveCharacterCreation() : base ()
		{
			
			PointerToObject = Frame.Client.Builtin["uicore"]["layer"].PointerToObject;
		}
		
		
		public bool IsCharacterCreationOpen {
			get {
				return this["charactercreation"]["isopen"].GetValueAs<bool>() || this["charactercreation"]["isopening"].GetValueAs<bool>();
			}
		}
		
		public bool IsCharacterCreationLoading {
			get {
				return this["charactercreation"]["isopening"].GetValueAs<bool>();
			}
			
		}
		
		public int GetStepID {
			get {
				return this["charactercreation"]["stepID"].GetValueAs<int>();
			}
		}
		
		
		public void NextStep() {
			
			if(GetStepID >= 1 && GetStepID < 5)
				this["charactercreation"].CallMethod("SwitchStep", new object[] { this.GetStepID+1 }, true);
		}
		
		public Race GetRace {
			get {
				int raceID = this["charactercreation"]["raceID"].GetValueAs<int>();
				return raceID == 1 ? Race.CALDARI : raceID == 2 ? Race.MINMATAR : raceID == 4 ? Race.AMARR : raceID == 8 ? Race.GALLENTE : Race.NONE;
			}
		}
		
		public void SetRaceID(Race race) {
			
			int raceID = race == Race.CALDARI ? 1 : race == Race.MINMATAR ? 2 : race == Race.AMARR ? 4 : race == Race.GALLENTE ? 8 : -1;
			if(raceID != -1)
				this["charactercreation"].CallMethod("SelectRace", new object[] { raceID }, false);
			
		}
		
		public bool IsBloodlineSet { 
			get {
				return !this["charactercreation"]["bloodlineID"].GetValueAs<int>().Equals(-1);
			}
		}
		
		public void SetRandomBloodlineID() {
			
			EveObject obj = this["charactercreation"]["sr"]["step"]["bloodlineIDs"];
			if(obj.IsValid){
				
				List<int> bloodlineIDs = obj.GetList<int>();
				if(bloodlineIDs.Count > 0){
					
					int rndBloodlineID = bloodlineIDs[rnd.Next(0,bloodlineIDs.Count)];
					string btnName = "bloodlineBtn_" + rndBloodlineID.ToString();
					
					this["charactercreation"]["sr"]["step"]["sr"][btnName].CallMethod("OnClick", new object[] {}, true);
				}	
			}
		}
		
		private int GetBloodlineID{
			get {
				return this["charactercreation"]["bloodlineID"].GetValueAs<int>();
			}
		}
		
		
		public void SetRandomGenderID(){
			int genderID = rnd.Next(0,2); 
			int bloodlineID = GetBloodlineID;
			if((genderID == 0 || genderID == 1) && bloodlineID != -1) {
				
				string btnName = "genderBtn_" + bloodlineID.ToString() + "_" + genderID.ToString();
					
				this["charactercreation"]["sr"]["step"]["sr"][btnName].CallMethod("OnClick", new object[] {}, true);
			}
		}
		
		public bool IsGenderSet{
			get {
				return !this["charactercreation"]["genderID"].GetValueAs<int>().Equals(-1);
			}
		}
		
		
		public void RandomizeCharacter(){
			this["charactercreation"]["sr"]["step"].CallMethod("RandomizeCharacter", new object[0] { }, true);
		}
		
		public bool ValidateStepComplete {
			get {
				return this["charactercreation"]["sr"]["step"].CallMethod("ValidateStepComplete", new object[] { }, false).GetValueAs<bool>();
			}
		}
		
		
		public void SetFirstname(string name) {
			this["charactercreation"]["sr"]["step"]["sr"]["firstNameEdit"].CallMethod("SetText", new object[] { name });
		}
		
		public String GetFirstName() {
			string name = this["charactercreation"]["sr"]["step"]["sr"]["firstNameEdit"].CallMethod("GetText", new object[] {}).GetValueAs<string>();
			if(string.IsNullOrEmpty(name))
				name = string.Empty;
			return name;
		}
		
		public void FinalizeCharacter() {
			this["charactercreation"].CallMethod("Save", new object[] { }, true);
		}
		
	}
}


