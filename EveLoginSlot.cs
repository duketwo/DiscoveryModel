/*
 * ---------------------------------------
 * User: duketwo
 * Date: 03.01.2014
 * Time: 19:43
 * 
 * ---------------------------------------
 */
using System;

namespace EveModel
{
	/// <summary>
	/// Description of EveLoginSlot.
	/// </summary>
	public class EveLoginSlot : EveObject
	{
		public EveLoginSlot(IntPtr ptr) : base()
		{
			this.PointerToObject = ptr;
		}
		
		
		public long CharId
		{
			get { return this["characterDetails"]["charDetails"]["characterID"].GetValueAs<long>(); }
		}
		public string CharName
		{
			get { return this["characterDetails"]["charDetails"]["characterName"].GetValueAs<string>(); }
		}

		public bool Activate()
		{
			return Frame.Client.Builtin["uicore"]["layer"]["charsel"].CallMethod("EnterGameWithCharacter", new object[] { this },true).GetValueAs<bool>();
		}
		
		
	}
}
