using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootFilter
{
	public class LootFilterItemActionChangeFilterType : BaseItemActionEntry
	{
		public LootFilterItemActionChangeFilterType(XUiController itemController, string actionName, string spriteName, GamepadShortCut shortcut = GamepadShortCut.None, string soundName = "crafting/craft_click_craft", string disabledSoundName = "ui/ui_denied") : base(itemController, actionName, spriteName, shortcut, soundName, disabledSoundName)
		{


		}

		public LootFilterItemActionChangeFilterType(string actionName, string spriteName, XUiController itemController, GamepadShortCut shortcut = GamepadShortCut.None, string soundName = "crafting/craft_click_craft", string disabledSoundName = "ui/ui_denied") : base(actionName, spriteName, itemController, shortcut, soundName, disabledSoundName)
		{


		}

	}
}
