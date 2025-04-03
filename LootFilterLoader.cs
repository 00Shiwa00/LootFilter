using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ParticleEffect;

namespace LootFilter
{
	public static class LootFilterLoader
	{
		public static String modPath;
		public static List<LootFilter> LootFilterFromXML(String gameLocation)
		{
			List < LootFilter > loadedLF = new List < LootFilter >();
			try
			{
				if(!File.Exists(gameLocation + "/LootFilterConfig.xml"))
				{				
					XDocument nxml = new XDocument();
					//nxml.Load(gameLocation + "/LootFilterConfig.xml");
					XElement xmlEl = new XElement("LootFilterConfig");
					xmlEl.SetAttributeValue("worldPath", GameIO.GetWorldDir());
					xmlEl.SetAttributeValue("savePath", gameLocation);
					xmlEl.Add(new XElement("LootFilters"));
					nxml.Add(xmlEl);
					nxml.Save(gameLocation + "/LootFilterConfig.xml");
					nxml = null;
				}
				XDocument xml = XDocument.Load(gameLocation + "/LootFilterConfig.xml");
				List<XElement> lfs = xml.Descendants("LootFilters").First().Elements().ToList();
				foreach(XElement lf in lfs)
				{
					LootFilterManager.LootFilters.Add(new LootFilter(lf));
				}
			}
			catch (Exception e) { Log.Error(e.ToString()); }	
			foreach(LootFilter lf in LootFilterManager.LootFilters)
				if(lf.isDirty)
					LootFilterManager.updateLootFilterNoUI(lf);
			return loadedLF;
		}
	}
}
