/*
 * ---------------------------------------
 * User: duketwo
 * Date: 26.03.2014
 * Time: 09:06
 * 
 * ---------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Linq;

namespace EveModel
{
	/// <summary>
	/// Description of EveSkills.
	/// </summary>
	public class EveSkills : EveObject
	{
		
		private List<EveInvType> _allSkills { get; set; }
		private List<EveSkill> _mySkills { get; set; }
		private List<EveSkill> _mySkillQueue { get; set; }
		private TimeSpan? _skillQueueLength { get; set; }
		
		public EveSkills() : base()
		{
			
		}
		
		
		public TimeSpan? SkillQueueLength
		{
			get
			{
				TimeSpan? timeSpan = this._skillQueueLength;
				return timeSpan.HasValue ? new TimeSpan?(timeSpan.GetValueOrDefault()) : this._skillQueueLength = new TimeSpan?( new TimeSpan(Frame.Client.GetService("skillqueue").CallMethod("GetTrainingLengthOfQueue", new object[0]).GetValueAs<long>()));
			}
		}
		
		
		public List<EveInvType> AllSkills
		{
			get
			{
				if (this._allSkills == null)
				{
					this._allSkills = new List<EveInvType>();
					List<EveObject> list = Frame.Client.GetService("skills").CallMethod("GetAllSkills", new object[0],false).GetList<EveObject>();
					foreach (EveObject current in list)
					{
						EveInvType eveInvType = new EveInvType(current); //???
						eveInvType.TypeId = (int)current["typeID"].GetValueAs<int>();
						this._allSkills.Add(eveInvType);
					}
				}
				return this._allSkills;
			}
		}
		
		
		public bool AreMySkillsReady
		{
			get
			{
				return Frame.Client.GetService("skills")["myskills"].IsValid;
			}
		}
		
		
		public List<EveSkill> MySkills
		{
			get
			{
				if (this._mySkills == null)
				{
					this._mySkills = Frame.Client.GetService("skills")["myskills"].GetList<EveObject>().Select(new Func<EveObject,EveSkill>(this.EveObjectToEveSkill)).ToList<EveSkill>();
				}
				return this._mySkills;
			}
		}
		
		
		public List<EveSkill> MySkillQueue
		{
			get
			{
				if (this._mySkillQueue == null)
				{
					List<EveObject> list = Frame.Client.GetService("skillqueue")["skillQueue"].GetList<EveObject>();
					this._mySkillQueue = new List<EveSkill>();
					foreach (EveObject current in list)
					{
						
						
						EveSkill eveSkill = new EveSkill(current);
						eveSkill.TypeId = new EveObject(PyCall.PyTuple_GetItem(current.PointerToObject, 0),null,false).GetValueAs<int>(); // change caching here
						eveSkill.Level = new EveObject(PyCall.PyTuple_GetItem(current.PointerToObject, 1),null,false).GetValueAs<int>();
						this._mySkillQueue.Add(eveSkill);
					}
				}
				return this._mySkillQueue;
			}
		}
		
		public bool IsReady
		{
			get
			{
				return Frame.Client.GetService("skillqueue").IsValid && Frame.Client.GetService("skills").IsValid;
			}
		}
		
		
		public bool RefreshMySkills()
		{
			return Frame.Client.GetService("skills").CallMethod("MySkills", new object[] { 1 },true).GetValueAs<bool>();
			
		}
		
		public bool TrainSkillNow(EveInvType skill) {
			
			if(this.AreMySkillsReady && skill.IsValid){
				
				EveSkill eveskill = new EveSkill(skill);
				int? currentLevel = eveskill.Level;
				
				if(this.MySkills.Where( s => s.TypeId == eveskill.TypeId).Any() && currentLevel <= 4 && currentLevel >= 0){
					Frame.Client.GetService("skillqueue").CallMethod("TrainSkillNow", new object[] { eveskill.TypeId, currentLevel }, true);
					return true;
				}
			}
			return false;
		}
		
		public bool AddSkillToEndOfQueue(EveInvType skill){
			
			if(this.AreMySkillsReady && skill.IsValid){
				
				EveSkill eveskill = new EveSkill(skill);
				
				int? currentLevel = eveskill.Level;
				int? nextLevel = eveskill.Level+1;
				
				if(this.MySkillQueue.Where( s => s.TypeId == skill.TypeId ).Any()){
					nextLevel = this.MySkillQueue.Where( s => s.TypeId == skill.TypeId ).OrderByDescending( s => s.Level ).FirstOrDefault().Level+1;
					Frame.Log("[AddSkillToEndOfQueue] NextLevel: " + nextLevel.ToString());
				}
				
				if(this.MySkills.Where( s => s.TypeId == eveskill.TypeId).Any() && currentLevel <= 4 && currentLevel >= 0 && nextLevel > 0 && nextLevel <= 5){
					Frame.Client.GetService("skillqueue").CallMethod("AddSkillToEnd", new object[] { eveskill.TypeId, currentLevel, nextLevel }, true);
					return true;
				}
			}
			return false;
		}
		
		
		
		
		private EveSkill EveObjectToEveSkill(EveObject eveObject)
		{
			return new EveSkill(eveObject);
		}
		
		private EveSkill EveInvTypeToEveSkill(EveInvType eveInvType)
		{
			return new EveSkill(eveInvType);
		}
	}
}
