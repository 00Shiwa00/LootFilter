using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace LootFilter
{
	[Preserve]
	public class XUiC_LootFilterFilterList : XUiController
	{
		public void Print(string s)
		{
			Log.Out(s);
		}
	}
}
