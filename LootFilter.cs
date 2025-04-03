using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace LootFilter
{
	public class LootFilter : XUiListEntry
	{
		public bool isDirty=false;
		public enum LootFilterAction
		{
			Drop,
			Scrap,
			Leave,
			Take,
		}
		public string oldname = "";
		//public String name = "";
		private string name = "";
		public String getName()
		{
			if(name == null) 
				return "";
			return name;
		}
		public void setName(String value)
		{
			if(LootFilterManager.checkLootFilterNameAvailability(value))
				name = value;
			else
			{
				int i = 1;
				String nvalue = value + "_";
				while(!LootFilterManager.checkLootFilterNameAvailability(nvalue + i.ToString()))
				{
					i++;
				}
				oldname = name;
				name = nvalue + i.ToString();
				isDirty = true;
			}
		}

		public Boolean isFavorite = false;
		public Boolean isActive = false;
		public Boolean useQuality = false;

		public List<String> noneLootContainer = new List<String>();

		public List<LootFilterItemStack> dropItemStacks = new List<LootFilterItemStack>();
		public List<LootFilterItemStack> scrapItemStacks = new List<LootFilterItemStack>();

		public LootFilter(XElement xmlNode)
		{
			name = (xmlNode.Attribute("name") != null) ? xmlNode.Attribute("name").Value : "newFilter";
			isFavorite = (xmlNode.Attribute("isFavorite") != null) ? Boolean.Parse(xmlNode.Attribute("isFavorite").Value) : false;
			isActive = (xmlNode.Attribute("isActive") != null) ? Boolean.Parse(xmlNode.Attribute("isActive").Value) : false;
			useQuality = (xmlNode.Attribute("useQuality") != null) ? Boolean.Parse(xmlNode.Attribute("isActive").Value) : false;
			List<XElement> clist = xmlNode.Elements().ToList();
			try
			{
				foreach(XElement node in clist)
				{
					switch(node.Name.ToString())
					{
						case "DropItems":
							foreach(XElement cn in node.Elements().ToList())
							{
								Log.Out(cn.Value);
								LootFilterItemStack its;
								if(cn.HasElements)
								{
									var quality = ushort.Parse(cn.Element("Quality").Value);
									its = new LootFilterItemStack(new ItemValue(ItemClass.nameIdMapping.GetIdForName(cn.Value), quality, quality), 1, isDropItem: true);
								}
								else
									its = new LootFilterItemStack(new ItemValue(ItemClass.nameIdMapping.GetIdForName(cn.Value)), 1, isDropItem: true);
								its.isDropItem = true;
								var index = cn.Attribute("index");
								if(index != null)
									its.Index = int.Parse(index.Value);
								else
								{
									its.Index = getMaxItemIndex() + 1;
									isDirty = true;
								}
								dropItemStacks.Add(its);
							}
							continue;
						case "ScrapItems":
							foreach(XElement cn in node.Elements().ToArray())
							{
								Log.Out(cn.Value);
								LootFilterItemStack its;
								if(cn.HasElements)
								{
									var quality = ushort.Parse(cn.Element("Quality").Value);
									its = new LootFilterItemStack(new ItemValue(ItemClass.nameIdMapping.GetIdForName(cn.Value), quality, quality), 1, isScrapItem: true);
								}
								else
									its = new LootFilterItemStack(new ItemValue(ItemClass.nameIdMapping.GetIdForName(cn.Value)), 1, isScrapItem: true);
								its.isScrapItem = true;
								var index = cn.Attribute("index");
								if(index != null)
									its.Index = int.Parse(index.Value);
								else
								{
									its.Index = getMaxItemIndex() + 1;
									isDirty = true;
								}
								scrapItemStacks.Add(its);
							}
							continue;
						case "noneLootContainers":
							foreach(XNode cn in node.Nodes().ToArray())
							{
								noneLootContainer.Append(cn.ToString());
							}
							continue;
					}
				}
			}
			catch(Exception ex) { Log.Error(ex.ToString(), ex); }
		}
		public LootFilter() { }

		public override int CompareTo(object _otherEntry)
		{
			if(_otherEntry == null) return -1;
			if(_otherEntry is LootFilter)
				if(((LootFilter)_otherEntry).name == this.name)
					return 1;
				else
					return -1;
			return -1;
		}

		public override bool GetBindingValue(ref string _value, string _bindingName)
		{
			throw new NotImplementedException();
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.Equals(_searchString);
		}

		internal XElement toXml()
		{
			XElement xmllf = new XElement("LootFilter");
			try { 
				xmllf.SetAttributeValue("name", name);
				xmllf.SetAttributeValue("isActive", isActive);
				xmllf.SetAttributeValue("isFavorite", isFavorite);
				xmllf.SetAttributeValue("useQuality", useQuality.ToString());
				XElement di = new XElement("DropItems");
				foreach(var s in dropItemStacks)
				{
					var Xe = new XElement("DropItem", s.itemValue.ItemClass.Name);
					Xe.SetAttributeValue("index", s.Index);
					di.Add(Xe);
				}					
				XElement si = new XElement("ScrapItems");
				foreach(var s in scrapItemStacks)
					si.Add(new XElement("ScrapItem", s.itemValue.ItemClass.Name, new XAttribute("index", s.Index)));
				XElement ni = new XElement("noneLootContainers");
				foreach(var s in noneLootContainer)
					ni.Add(new XElement("noneLootContainer", s));
				xmllf.Add(di);
				xmllf.Add(si);
				xmllf.Add(ni);
			}
			catch(Exception ex) { Log.Error(ex.ToString()); }
			return xmllf;
		}

		public LootFilterAction getActionForItem(ItemStack itemStack, String lootContainer)
		{
			if(this.noneLootContainer.Contains(lootContainer))
				return LootFilterAction.Leave;
			if(this.dropItemStacks.Where(x => x.itemValue.ItemClass.Name == itemStack.itemValue.ItemClass.Name).FirstOrDefault() != null)
				return LootFilterAction.Drop;
			if(this.dropItemStacks.Where(x => x.itemValue.ItemClass.Name == itemStack.itemValue.ItemClass.Name).FirstOrDefault() != null)
				return LootFilterAction.Scrap;

			return LootFilterAction.Take;
		}
		public Boolean containsDropItem(ItemStack itemStack)
		{
			return this.dropItemStacks.Where(x => x.itemValue.ItemClass.Name == itemStack.itemValue.ItemClass.Name).FirstOrDefault() != null;
		}
		public Boolean containsScrapItem(ItemStack itemStack)
		{
			return this.scrapItemStacks.Where(x => x.itemValue.ItemClass.Name == itemStack.itemValue.ItemClass.Name).FirstOrDefault() != null;
		}
		public void addDropItem(ItemStack itemstack)
		{
			LootFilterItemStack lfitemstack = new LootFilterItemStack(itemstack);
			lfitemstack.isDropItem = true;
			lfitemstack.count = 1;
			lfitemstack.Index = getMaxItemIndex() + 1;
			this.dropItemStacks.Add(lfitemstack);
			LootFilterManager.updateLootFilter(this);
		}
		public void removeDropItem(ItemStack itemstack)
		{
			var toRemove = this.dropItemStacks.Where(x => x.itemValue.ItemClass.Name == itemstack.itemValue.ItemClass.Name).First();
			dropItemStacks.Remove(toRemove);
			LootFilterManager.updateLootFilter(this);
		}

		public void addScrapItem(ItemStack itemstack)
		{
			LootFilterItemStack lfitemstack = new LootFilterItemStack(itemstack);
			lfitemstack.isScrapItem = true;
			lfitemstack.count = 1;
			lfitemstack.Index = getMaxItemIndex() + 1;
			this.scrapItemStacks.Add(lfitemstack);
			LootFilterManager.updateLootFilter(this);
		}
		public void removeScrapItem(ItemStack itemstack)
		{
			var toRemove = this.scrapItemStacks.Where(x => x.itemValue.ItemClass.Name == itemstack.itemValue.ItemClass.Name).First();
			scrapItemStacks.Remove(toRemove);
			LootFilterManager.updateLootFilter(this);
		}
		public void updateIndexOfItem(LootFilterItemStack itemstack)
		{
			LootFilterItemStack toUpdate = null;
			if(itemstack.isDropItem)
			{
				toUpdate = this.dropItemStacks.Where(x => x.itemValue.ItemClass.Name == itemstack.itemValue.ItemClass.Name).First();				
			}
			else
			{
				toUpdate = this.scrapItemStacks.Where(x => x.itemValue.ItemClass.Name == itemstack.itemValue.ItemClass.Name).First();
			}

			toUpdate.Index = itemstack.Index;
			LootFilterManager.updateLootFilterNoUI(this);
		}
		public bool itemIndexExists(int index)
		{
			/*bool b = this.dropItemStacks.Where(x => x.Index == index).Any();
			if(b)
				return b;
			b = this.scrapItemStacks.Where(x => x.Index == index).Any();
			return b;*/
			return this.scrapItemStacks.Where(x => x.Index == index).Any() || this.dropItemStacks.Where(x => x.Index == index).Any();
		}
		public int getMaxItemIndex()
		{
			int i = -1;
			foreach(var item in this.scrapItemStacks)
				if(item.Index > i)
					i = item.Index;
			foreach(var item in this.dropItemStacks)
				if(item.Index > i)
					i = item.Index;
			
			return i;
		}
		public LootFilterItemStack[] getFilteredItems()
		{
			if(this.getMaxItemIndex() == -1)
				return LootFilterItemStack.CreateArray(XUiC_LootFilterContentGrid.PageLength*2);

			int length = Math.Max(XUiC_LootFilterContentGrid.PageLength, (Mathf.CeilToInt((float)this.getMaxItemIndex()/ ((float)XUiC_LootFilterContentGrid.PageLength-1))+1) * XUiC_LootFilterContentGrid.PageLength);
			LootFilterItemStack[] stack =  LootFilterItemStack.CreateArray(length);

			//exclude negative Indices ==> used for DragAndDrop Item
			foreach(var item in this.dropItemStacks)
			{
				if(item.Index > -1)
					stack[item.Index] = item;
			}
			foreach(var item in this.scrapItemStacks)
			{
				if(item.Index > -1)
					stack[item.Index] = item;
			}				
			return stack;
		}
		public LootFilterItemStack getDragAndDropItem()
		{
			foreach(var item in this.dropItemStacks)
			{
				if(item.Index == -1)
					return item;
			}
			foreach(var item in this.scrapItemStacks)
			{
				if(item.Index == -1)
					return item;
			}
			return null;
		}
	}
}
