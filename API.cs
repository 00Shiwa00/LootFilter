using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Xml;
using UnityEngine;
using LootFilter;

namespace LootFilter
{
	public class API : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			try
			{
				string path = modInstance.Path;
				XmlDocument xml = new XmlDocument();
				xml.Load(path + "/LootFilterConfig.xml");
				LootFilterManager.modPath = path;
				LootFilterLoader.modPath = path;
				string[] quickLockButtons = xml.GetElementsByTagName("LootFilterButtons")[0].InnerText.Split(' ');
				LootFilterManager.lootFilterHotkeys = new KeyCode[quickLockButtons.Length];
				for(int i = 0; i < quickLockButtons.Length; i++)
					LootFilterManager.lootFilterHotkeys[i] = (KeyCode)int.Parse(quickLockButtons[i]);

				quickLockButtons = xml.GetElementsByTagName("LootDropMarkingButtons")[0].InnerText.Split(' ');
				LootFilterManager.lootFilterDropMarkingHotkeys = new KeyCode[quickLockButtons.Length];
				for(int i = 0; i < quickLockButtons.Length; i++)
					LootFilterManager.lootFilterDropMarkingHotkeys[i] = (KeyCode)int.Parse(quickLockButtons[i]);

				quickLockButtons = xml.GetElementsByTagName("LootScrapMarkingButtons")[0].InnerText.Split(' ');
				LootFilterManager.lootFilterScrapMarkingHotkeys = new KeyCode[quickLockButtons.Length];
				for(int i = 0; i < quickLockButtons.Length; i++)
					LootFilterManager.lootFilterScrapMarkingHotkeys[i] = (KeyCode)int.Parse(quickLockButtons[i]);

				quickLockButtons = xml.GetElementsByTagName("noneLootContainerButtons")[0].InnerText.Split(' ');
				LootFilterManager.lootFilternoneLootContainerHotkeys = new KeyCode[quickLockButtons.Length];
				for(int i = 0; i < quickLockButtons.Length; i++)
					LootFilterManager.lootFilternoneLootContainerHotkeys[i] = (KeyCode)int.Parse(quickLockButtons[i]);

				LootFilterManager.openLootPanelHotkeys = new KeyCode[1];
				LootFilterManager.openLootPanelHotkeys[0] = (KeyCode)108;

			}
			catch { Log.Warning("FEHLER"); }
			Harmony harmony = new Harmony("00Shiwa00.7d2d.LootFilter");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

				
				
		}
	}
	
}