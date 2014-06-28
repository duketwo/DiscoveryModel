/*
 * ---------------------------------------
 * User: duketwo
 * Date: 26.03.2014
 * Time: 09:06
 * 
 * ---------------------------------------
 */
using System;

namespace EveModel
{
	/// <summary>
	/// Description of EveSkill.
	/// </summary>
	public class EveSkill : EveItem
	{
		
		private int? _level;
		public int? Level {
			get {
				if(_level == null && this.IsValid) {
					_level = Frame.Client.GetItem(this)["skillLevel"].GetValueAs<int>();
				}
				return _level;
			}
			set {
				_level = value;
			}
		}
		
		private int? _skillPoints;
		public int? SkillPoints  {
			get {
				if(_skillPoints == null && this.IsValid){
					_skillPoints = Frame.Client.GetItem(this)["skillPoints"].GetValueAs<int>();
				}
				return _skillPoints;
			}
			set {
				_skillPoints = value;
			}
		}
		
		private int? _skillTimeConstant;
		public int? SkillTimeConstant {
			get {
				if(_skillTimeConstant == null && this.IsValid){
					_skillTimeConstant = Frame.Client.GetItem(this)["skillTimeConstant"].GetValueAs<int>();
				}
				return _skillTimeConstant;
			}
			set {
				_skillTimeConstant = value;
			}
		}
		
		
		public EveSkill(EveObject obj) : base(obj)
		{
			
		}
		
		public bool InTraining
		{
			get
			{
				return base.FlagId == Frame.Client.Const["flagSkillInTraining"].GetValueAs<int>();
			}
		}
		
		public bool AddSkillToEndOfQueue(){
			return Frame.Client.Skills.AddSkillToEndOfQueue(this);
		}
		
		public bool TrainNow() {
			return Frame.Client.Skills.TrainSkillNow(this);
		}
		
	}

}