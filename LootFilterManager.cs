using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HarmonyLib;
using UnityEngine;
using GameEvent.SequenceActions;
using UnityEngine.Events;
//using System.Xml;
using WorldGenerationEngineFinal;
using System.Xml.Schema;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Audio;

namespace LootFilter
{

	
	public static class LootFilterManager
	{
		//per Game Load
		public static KeyCode[] lootFilterHotkeys;
		public static KeyCode[] lootFilterDropMarkingHotkeys;
		public static KeyCode[] lootFilterScrapMarkingHotkeys;
		public static KeyCode[] lootFilternoneLootContainerHotkeys;
		public static KeyCode[] openLootPanelHotkeys;
		public static string modPath;

		public static event EventHandler LootFilterRemoved;
		public delegate void LootFilterRemovedHandler(object sender, EventHandler e);

		public static event LootFilterChangedHandler LootFilterChanged;
		public delegate void LootFilterChangedHandler(LootFilter lootFilter);

		//public List<LootFilter> lootFilterList;

		//public  List<int> dropItemCodes_list;
		//public  List<int> scrapItemCodes_list;
		//public  List<String> noneLootContainer_list;
		//public  Dictionary<int, List<int>> dropItemCodes_;

		//per Savegame Load
		public static string saveGameDir = GameIO.GetSaveGameDir();
		public static List<LootFilter> LootFilters = new List<LootFilter>();

		private static EntityPlayerLocal getLocalPlayer()
		{
			int EntityId = GameManager.Instance.GetPersistentLocalPlayer().EntityId;
			EntityPlayerLocal localPlayer = GameManager.Instance.World.GetLocalPlayerFromID(EntityId);
			return localPlayer;
		}
		public static void filterLoot()
		{
			EntityPlayerLocal localPlayer = getLocalPlayer();
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);
			XUiC_LootWindowGroup lootWindowGroup = (XUiC_LootWindowGroup)((XUiWindowGroup)playerUI.windowManager.GetWindow("looting")).Controller;
			String lootContainer = lootWindowGroup.lootContainerName;
			if(lootWindowGroup == null)
				return;
			if(!lootWindowGroup.IsOpen)
				return;
			var alf = getActiveLootFilter();
			XUiC_ItemStack[] itemstacksControlers = lootWindowGroup.lootWindow.lootContainer.itemControllers;
			foreach(XUiC_ItemStack itemstack in itemstacksControlers)
			{
				List<LootFilter.LootFilterAction> actions = new List<LootFilter.LootFilterAction>();
				if(itemstack.itemStack.IsEmpty())
					continue;
				foreach(LootFilter lootFilter in alf)
				{
					actions.Add(lootFilter.getActionForItem(itemstack.itemStack, lootContainer));
				}
				if(actions.Contains(LootFilter.LootFilterAction.Scrap))
				{
					new ItemActionEntryScrap(itemstack).OnActivated();
					new ActionDelay().OnClientPerform(localPlayer);
					continue;
				}
				if(actions.Contains(LootFilter.LootFilterAction.Drop))
				{
					new ItemActionEntryDrop(itemstack).OnActivated();
					new ActionDelay().OnClientPerform(localPlayer);
					continue;
				}
				new ItemActionEntryTake(itemstack).OnActivated();
			}
			int i = 0;
			foreach(XUiC_ItemStack itemstack in itemstacksControlers)
			{
				if(!itemstack.itemStack.IsEmpty())
				{
					i = i + 1;
				}
			}
			if(i == 0)
				playerUI.windowManager.CloseAllOpenWindows();
			else
				WarnQueueFull();
			//TODO: OR inventory full!

		}
		public static void WarnQueueFull()
		{
			EntityPlayerLocal localPlayer = getLocalPlayer();
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);
			GameManager.ShowTooltip(playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull"));
			Manager.PlayInsidePlayerHead("ui_denied");
		}
		public static void changeDropItem()
		{
			var alf = getActiveLootFilter();
			if(alf.Count == 1)
			{
				changeDropItem4LF(alf[0]);
			}

		}
		public static void changeDropItem4LF(LootFilter lootFilter)
		{
			EntityPlayerLocal localPlayer = getLocalPlayer();
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);

			XUiC_LootWindowGroup lootWindowGroup = (XUiC_LootWindowGroup)((XUiWindowGroup)playerUI.windowManager.GetWindow("looting")).Controller;
			if(lootWindowGroup != null && lootWindowGroup.IsOpen)
			{
				XUiC_ItemStack[] itemstacksControlers = lootWindowGroup.lootWindow.lootContainer.itemControllers;
				foreach(XUiC_ItemStack itemstack in itemstacksControlers)
				{
					if(itemstack.Selected)
					{
						
						if(lootFilter.containsDropItem(itemstack.itemStack))
						{
							Log.Out("remove dropItem: " + itemstack.itemStack.itemValue.ItemClass.Name);
							lootFilter.removeDropItem(itemstack.itemStack);
							return;
						}
						else
						{
							Log.Out("add dropItem: " + itemstack.itemStack.itemValue.ItemClass.Name);
							lootFilter.addDropItem(itemstack.itemStack);
							return;
						}
					}
				}
			}
		}
		
		public static void changeScrapItem()
		{
			var alf = getActiveLootFilter();
			if(alf.Count == 1)
			{
				changeScrapItem4LF(alf[0]);
			}
		}
		public static void changeScrapItem4LF(LootFilter lootFilter)
		{
			EntityPlayerLocal localPlayer = getLocalPlayer();
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);

			XUiC_LootWindowGroup lootWindowGroup = (XUiC_LootWindowGroup)((XUiWindowGroup)playerUI.windowManager.GetWindow("looting")).Controller;
			if(lootWindowGroup != null && lootWindowGroup.IsOpen)
			{
				XUiC_ItemStack[] itemstacksControlers = lootWindowGroup.lootWindow.lootContainer.itemControllers;
				foreach(XUiC_ItemStack itemstack in itemstacksControlers)
				{
					if(itemstack.Selected)
					{
						
						if(lootFilter.containsScrapItem(itemstack.itemStack))
						{
							Log.Out("remove scrapItem: " + itemstack.itemStack.itemValue.ItemClass.Name);
							lootFilter.removeScrapItem(itemstack.itemStack);
							return;
						}
						else
						{
							Log.Out("add scrapItem: " + itemstack.itemStack.itemValue.ItemClass.Name);
							lootFilter.addScrapItem(itemstack.itemStack);
							return;
						}
					}
				}
			}
		}

		public static void changeNoneLootContainer()
		{
			/*EntityPlayerLocal localPlayer = getLocalPlayer();
			LocalPlayerUI playerUI = LocalPlayerUI.GetUIForPlayer(localPlayer);
			global::XUiC_LootWindowGroup lootWindowGroup = (global::XUiC_LootWindowGroup)((XUiWindowGroup)playerUI.windowManager.GetWindow("looting")).Controller;
			if(lootWindowGroup != null && lootWindowGroup.IsOpen)
			{
				String windowName = lootWindowGroup.lootContainerName;
				if(noneLootContainer_list.Contains(windowName))
				{
					removeNoneLootContainer(windowName);
				}
				else
				{
					addNoneLootContainer(windowName);
				}
			}*/
		}
		public static void addNoneLootContainer(string containerName)
		{
			/*if(LootFilterManager.noneLootContainer_list != null)
			{
				LootFilterManager.noneLootContainer_list.Add(containerName);
				Log.Out("add to noneLootContainer_list: " + containerName);
				XmlDocument xml = getXml();
				XmlElement noneLootContainer = xml.CreateElement("ContainerName");
				noneLootContainer.InnerText = containerName;
				xml.GetElementsByTagName("noneLootContainer")[0].AppendChild(noneLootContainer);
				saveXml(xml);
				return;
			}
			else
				Log.Warning("noneLootContainer_list is null. report to developer!");*/
		}
		public static void removeNoneLootContainer(string containerName)
		{
			/*foreach(LootFilter lf in LootFilterManager.LootFilters)
			{
				if(!Boolean.Parse(lf.isActive))
					continue;
				if(lf.noneLootContainer != null)
				{
					lf.noneLootContainer.Remove(containerName);
					Log.Out("removed from noneLootContainer_list: " + containerName);
					XmlDocument xml = getXml();
					XmlNodeList chilnodes = xml.GetElementsByTagName("noneLootContainer")[0].ChildNodes;
					foreach(XmlNode node in chilnodes)
						if(node.InnerText == containerName)
						{
							xml.GetElementsByTagName("noneLootContainer")[0].RemoveChild(node);
						}
					saveXml(xml);
					return;
				}
				else
				{
					Log.Warning("noneLootContainer_list is null. report to developer!");
				}
			}			
			return;*/
		}

		

		private static XDocument getXml()
		{			
			return XDocument.Load(saveGameDir + "/LootFilterConfig.xml");
		}
		private static void saveXml(XDocument xml)
		{
			xml.Save(saveGameDir + "/LootFilterConfig.xml");
		}

		public static LootFilter CreateLootFilter()
		{
			Log.Error("CREATE ITEM");
			LootFilter lf = new LootFilter();
			lf.setName(Time.time.ToString());
			lf.isFavorite = false;
			lf.isActive = false;
			try
			{
				LootFilters.Add(lf);
				XDocument xml = getXml();
				xml.Descendants("LootFilters").First().Add(lf.toXml());
				saveXml(xml);
			}
			catch(Exception ex) { Log.Error(ex.ToString()); }
			return lf;
			
		}
		public static void updateLootFilter(LootFilter lootFilter)
		{
			XDocument xml = getXml();
			var element = xml.Descendants()
					.Where(x => (string)x.Attribute("name") == lootFilter.getName())
					.First();
			if(element == null)
				return;
			XElement xel = lootFilter.toXml();
			element.ReplaceWith(xel);
			saveXml(xml);
			LootFilterManager.LootFilterChanged.Invoke(lootFilter);
			lootFilter.isDirty = false;
		}
		public static void updateLootFilterNoUI(LootFilter lootFilter)
		{
			if(lootFilter.oldname != "") {
				updateLootFilter(lootFilter, lootFilter.oldname);
				return;
			}
				
			XDocument xml = getXml();
			var element = xml.Descendants()
					.Where(x => (string)x.Attribute("name") == lootFilter.getName())
					.First();
			if(element == null)
				return;
			XElement xel = lootFilter.toXml();
			element.ReplaceWith(xel);
			saveXml(xml);
			lootFilter.isDirty = false;
		}
		public static void updateLootFilter(LootFilter lootFilter, String oldName)
		{
			XDocument xml = getXml();
			var element = xml.Descendants()
					.Where(x => (string)x.Attribute("name") == oldName)
					.First();
			if(element == null)
				return;
			XElement xel = lootFilter.toXml();
			element.ReplaceWith(xel);
			saveXml(xml);
			LootFilterManager.LootFilterChanged.Invoke(lootFilter);
			lootFilter.oldname = "";
			lootFilter.isDirty = false;
		}
		public static void removeLootFilter(LootFilter lootFilter)
		{
			LootFilters.Remove(lootFilter);
			try { 
				XDocument xml = getXml();
				xml.Descendants()
						.Where(x => (string)x.Attribute("name") == lootFilter.getName())
						.FirstOrDefault().Remove();
				saveXml(xml);
			}
			catch(Exception ex) { Log.Error(ex.ToString()); }
			LootFilterRemoved.Invoke(lootFilter, null);
		}

		public static List<LootFilter> getActiveLootFilter()
		{
			List<LootFilter> alf = new List<LootFilter>();
			foreach(LootFilter l in LootFilters)
			{
				if(l.isActive)
					alf.Add(l);
			}
			return alf;
		}

		internal static bool checkLootFilterNameAvailability(string name)
		{
			return !LootFilters.Where(x => x.getName() == name).Any();
		}
	}
}
