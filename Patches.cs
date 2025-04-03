using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Audio;
using GameEvent.SequenceActions;
using HarmonyLib;
using LootFilter;
using UnityEngine;
using UnityEngine.Scripting;

namespace LootFilter
{

	internal class Patches
	{
		//This patch is used to initialize the UI functionallity for the LootFilter and restock buttons.
		[HarmonyPatch(typeof(XUiC_ContainerStandardControls), "Init")]
		private class QS_01
		{
			public static void Postfix(XUiC_LootWindow __instance)
			{
				//Log.Out("is instance");
				if(__instance.Parent.Parent is XUiC_LootWindow)
				{
					//LootFilter.playerControls = __instance;

					XUiController childById = __instance.GetChildById("btnMoveLootFilter");
					//Log.Out("Found Button");
					if(childById != null)
					{
						childById.OnPress += delegate (XUiController _sender, int _args)
						{
							//Log.Out("doing stuff");
							/*int EntityId = GameManager.Instance.GetPersistentLocalPlayer().EntityId;
							EntityPlayerLocal localPlayer = GameManager.Instance.World.GetLocalPlayerFromID(EntityId);
							LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);
							playerUI.xui.GetChildByType<XUiC_LootFilterWindowGroup>().lootfilterManager.filterLoot();*/
							LootFilterManager.filterLoot();
						};
					}
				}
			}
		}

		[HarmonyPatch(typeof(World), "LoadWorld")]
		private class QS_02
		{
			public static void Postfix(World __instance)
			{
				//load LootFilter From Save
				
				Log.Out("load lootfilter for game " + GameIO.GetSaveGameDir());
				LootFilterManager.saveGameDir = GameIO.GetSaveGameDir();
				List<LootFilter> lfs = LootFilterLoader.LootFilterFromXML(GameIO.GetSaveGameDir());
				/*foreach(LootFilter filter in lfs ) {
					LootFilterManager.LootFilters.Add(filter);
				}*/
				Log.Out(LootFilterManager.LootFilters.Count().ToString());
			}
		}
		[HarmonyPatch(typeof(World), "UnloadWorld")]
		private class QS_03
		{
			public static void Postfix(World __instance)
			{
				//unload LootFilters from current Game
				LootFilterManager.LootFilters.Clear();
				LootFilterManager.saveGameDir = "";
			}
		}

		//LootFilter and Restock functionallity by pressing hotkeys (useful if other mods remove the UI buttons)
		[HarmonyPatch(typeof(GameManager), "UpdateTick")]
		private class QS_04
		{
			public static void Postfix()
			{
				if(UICamera.GetKeyDown(LootFilterManager.lootFilterHotkeys[LootFilterManager.lootFilterHotkeys.Length - 1]))
				{
					for(int i = 0; i < LootFilterManager.lootFilterHotkeys.Length - 1; i++)
					{
						if(!UICamera.GetKey(LootFilterManager.lootFilterHotkeys[i]))
							return;
					}
					//XUi.GetChildById("btnMoveLootFilter");
					LootFilterManager.filterLoot();
					Manager.PlayButtonClick();
				}
				if(UICamera.GetKeyDown(LootFilterManager.lootFilterDropMarkingHotkeys[LootFilterManager.lootFilterDropMarkingHotkeys.Length - 1]))
				{
					for(int i = 0; i < LootFilterManager.lootFilterDropMarkingHotkeys.Length - 1; i++)
					{
						if(!UICamera.GetKey(LootFilterManager.lootFilterDropMarkingHotkeys[i]))
							return;
					}
					LootFilterManager.changeDropItem();
					Manager.PlayButtonClick();
				}
				if(UICamera.GetKeyDown(LootFilterManager.lootFilterScrapMarkingHotkeys[LootFilterManager.lootFilterScrapMarkingHotkeys.Length - 1]))
				{
					for(int i = 0; i < LootFilterManager.lootFilterScrapMarkingHotkeys.Length - 1; i++)
					{
						if(!UICamera.GetKey(LootFilterManager.lootFilterScrapMarkingHotkeys[i]))
							return;
					}
					LootFilterManager.changeScrapItem();
					Manager.PlayButtonClick();
				}
				if(UICamera.GetKeyDown(LootFilterManager.lootFilternoneLootContainerHotkeys[LootFilterManager.lootFilternoneLootContainerHotkeys.Length - 1]))
				{
					for(int i = 0; i < LootFilterManager.lootFilternoneLootContainerHotkeys.Length - 1; i++)
					{
						if(!UICamera.GetKey(LootFilterManager.lootFilternoneLootContainerHotkeys[i]))
							return;
					}
					LootFilterManager.changeNoneLootContainer();
					Manager.PlayButtonClick();
				}
				if(UICamera.GetKeyDown(LootFilterManager.openLootPanelHotkeys[LootFilterManager.openLootPanelHotkeys.Length - 1]))
				{
					for(int i = 0; i < LootFilterManager.openLootPanelHotkeys.Length - 1; i++)
					{
						if(!UICamera.GetKey(LootFilterManager.openLootPanelHotkeys[i]))
							return;
					}
						int EntityId = GameManager.Instance.GetPersistentLocalPlayer().EntityId;
						EntityPlayerLocal localPlayer = GameManager.Instance.World.GetLocalPlayerFromID(EntityId);
						LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);
						playerUI.windowManager.OpenIfNotOpen("lootfilter", true);
						playerUI.windowManager.OpenIfNotOpen("lootfilterdraganddrop", false);
				}
			}
		}
		//This patch is used to initialize the UI functionallity for the LootFilter and restock buttons.
		
			
	}
}
