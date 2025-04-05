using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LootFilter
{
	public class XUiC_LootFilterContentInfoWindow : XUiC_InfoWindow
	{
		
		public ItemStack itemStack = ItemStack.Empty.Clone();		
		public ItemClass itemClass;

		public LootFilter lootFilter = null;

		//the new type of Stack (a square in the LootFilterContentGrid)
		public XUiC_LootFilterContentItemStack selectedLootFilterItemStack;
		public XUiC_LootFilterEntry selectedLootFilterEntry;

		public XUiController itemPreview;

		public XUiC_LootFilterItemActionList mainActionItemList;
		public XUiC_PartList partList;
		public XUiC_Counter BuySellCounter;		
		public XUiController statButton;		
		public XUiController descriptionButton;		
		public XUiC_InfoWindow emptyInfoWindow;		
		public bool isBuying;		
		public bool useCustomMarkup;
		public bool SetMaxCountOnDirty;		
		public ItemDisplayEntry itemDisplayEntry;		
		public XUiC_SelectableEntry hoverEntry;			
		public ItemStack compareStack = ItemStack.Empty;		
		public bool showStats = true;		
		public readonly CachedStringFormatterInt itemcostFormatter = new CachedStringFormatterInt();		
		public readonly CachedStringFormatter<int> markupFormatter = new CachedStringFormatter<int>((int _i) => (_i <= 0) ? ((_i >= 0) ? "" : $" ({_i}%)") : $" (+{_i}%)");
		public readonly CachedStringFormatterXuiRgbaColor itemicontintcolorFormatter = new CachedStringFormatterXuiRgbaColor();		
		public readonly CachedStringFormatterXuiRgbaColor durabilitycolorFormatter = new CachedStringFormatterXuiRgbaColor();		
		public readonly CachedStringFormatterFloat durabilityfillFormatter = new CachedStringFormatterFloat();		
		public readonly CachedStringFormatterInt durabilitytextFormatter = new CachedStringFormatterInt();		
		public readonly CachedStringFormatterXuiRgbaColor altitemtypeiconcolorFormatter = new CachedStringFormatterXuiRgbaColor();		
		public static readonly Dictionary<string, string> itemGroupToIcon = new CaseInsensitiveStringDictionary<string>
	{
		{ "basics", "ui_game_symbol_campfire" },
		{ "building", "ui_game_symbol_map_house" },
		{ "resources", "ui_game_symbol_resource" },
		{ "ammo/weapons", "ui_game_symbol_knife" },
		{ "tools/traps", "ui_game_symbol_tool" },
		{ "food/cooking", "ui_game_symbol_fork" },
		{ "medicine", "ui_game_symbol_medical" },
		{ "clothing", "ui_game_symbol_shirt" },
		{ "decor/miscellaneous", "ui_game_symbol_chair" },
		{ "books", "ui_game_symbol_book" },
		{ "chemicals", "ui_game_symbol_water" },
		{ "mods", "ui_game_symbol_assemble" }
	};
		
		public static readonly string defaultItemGroupIcon = "ui_game_symbol_campfire";				
		public ItemStack CompareStack
		{
			get
			{
				return compareStack;
			}
			set
			{
				if(compareStack != value)
				{
					compareStack = value;
					RefreshBindings();
				}
			}
		}


		public override void Init()
		{
			base.Init();
			itemPreview = GetChildById("itemPreview");
			mainActionItemList = (XUiC_LootFilterItemActionList)GetChildById("itemActions");
			partList = (XUiC_PartList)GetChildById("parts");
			BuySellCounter = GetChildByType<XUiC_Counter>();
			if(BuySellCounter != null)
			{
				BuySellCounter.OnCountChanged += Counter_OnCountChanged;
				BuySellCounter.Count = 1;
			}

			statButton = GetChildById("statButton");
			statButton.OnPress += StatButton_OnPress;
			descriptionButton = GetChildById("descriptionButton");
			descriptionButton.OnPress += DescriptionButton_OnPress;
		}

		
		public void DescriptionButton_OnPress(XUiController _sender, int _mouseButton)
		{
			((XUiV_Button)statButton.ViewComponent).Selected = false;
			((XUiV_Button)descriptionButton.ViewComponent).Selected = true;
			showStats = false;
			IsDirty = true;
		}

		
		public void StatButton_OnPress(XUiController _sender, int _mouseButton)
		{
			((XUiV_Button)statButton.ViewComponent).Selected = true;
			((XUiV_Button)descriptionButton.ViewComponent).Selected = false;
			showStats = true;
			IsDirty = true;
		}

		
		public void Counter_OnCountChanged(XUiController _sender, OnCountChangedEventArgs _e)
		{
			RefreshBindings();
		}

		public override void Deselect()
		{
		}

		public override bool GetBindingValue(ref string value, string bindingName)
		{
			switch(bindingName)
			{
				case "itemname":
					if(selectedLootFilterEntry != null)
					{
						value = selectedLootFilterEntry.entryData.getName();
						return true;
					}
					value = ((this.itemClass != null) ? this.itemClass.GetLocalizedItemName() : "");
					return true;
				case "itemammoname":
					value = "";
					if(this.itemClass != null)
					{
						if(this.itemClass.Actions[0] is ItemActionRanged itemActionRanged)
						{
							if(itemActionRanged.MagazineItemNames.Length > 1)
							{
								ItemClass itemClass = ItemClass.GetItemClass(itemActionRanged.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
								value = itemClass.GetLocalizedItemName();
							}
						}
						else if(this.itemClass.Actions[0] is ItemActionLauncher itemActionLauncher && itemActionLauncher.MagazineItemNames.Length > 1)
						{
							ItemClass itemClass2 = ItemClass.GetItemClass(itemActionLauncher.MagazineItemNames[itemStack.itemValue.SelectedAmmoTypeIndex]);
							value = itemClass2.GetLocalizedItemName();
						}
					}

					return true;
				case "itemicon":
					if(itemStack != null)
					{
						value = itemStack.itemValue.GetPropertyOverride("CustomIcon", (this.itemClass != null) ? this.itemClass.GetIconName() : "");
					}
					else
					{
						value = "";
					}

					return true;
				case "itemcost":
					value = "";
					if(this.itemClass != null)
					{
						bool flag = ((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader);
						if(!flag && !isBuying)
						{
							value = Localization.Get("xuiNoSellPrice");
							return true;
						}

						int count = itemStack.count;					
						int sellPrice = XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count, this.itemClass);
						value = ((sellPrice > 0) ? itemcostFormatter.Format(sellPrice) : Localization.Get("xuiNoSellPrice"));
					}

					return true;
				case "pricelabel":
					value = "";
					if(this.itemClass != null)
					{
						if(!((!this.itemClass.IsBlock()) ? this.itemClass.SellableToTrader : Block.list[itemStack.itemValue.type].SellableToTrader))
						{
							return true;
						}

						int count2 = itemStack.count;

						if(isBuying)
						{
							value = ((XUiM_Trader.GetBuyPrice(base.xui, itemStack.itemValue, count2, this.itemClass) > 0) ? Localization.Get("xuiBuyPrice") : "");
						}
						else
						{
							value = ((XUiM_Trader.GetSellPrice(base.xui, itemStack.itemValue, count2, this.itemClass) > 0) ? Localization.Get("xuiSellPrice") : "");
						}
					}

					return true;
				case "markup":
					value = "";
					return true;
				case "itemicontint":
					{
						Color32 v = Color.white;
						if(this.itemClass != null)
						{
							v = this.itemClass.GetIconTint(itemStack.itemValue);
						}

						value = itemicontintcolorFormatter.Format(v);
						return true;
					}
				case "itemdescription":
					value = "";
					if(this.itemClass != null)
					{
						if(this.itemClass.IsBlock())
						{
							string descriptionKey = Block.list[this.itemClass.Id].DescriptionKey;
							if(Localization.Exists(descriptionKey))
							{
								value = Localization.Get(descriptionKey);
							}
						}
						else
						{
							string descriptionKey2 = this.itemClass.DescriptionKey;
							if(Localization.Exists(descriptionKey2))
							{
								value = Localization.Get(descriptionKey2);
							}

							if(this.itemClass.Unlocks != "")
							{
								ItemClass itemClass3 = ItemClass.GetItemClass(this.itemClass.Unlocks);
								if(itemClass3 != null)
								{
									value = value + "\n\n" + Localization.Get(itemClass3.DescriptionKey);
								}
							}
						}
					}

					return true;
				case "itemgroupicon":
					value = "";
					if(this.itemClass != null && this.itemClass.Groups.Length != 0)
					{
						string key = this.itemClass.Groups[0];
						if(!itemGroupToIcon.TryGetValue(key, out value))
						{
							value = defaultItemGroupIcon;
						}
					}

					return true;
				case "hasdurability":
					value = (!itemStack.IsEmpty() && this.itemClass != null && this.itemClass.ShowQualityBar).ToString();
					return true;
				case "durabilitycolor":
					{
						Color32 v2 = Color.white;
						if(!itemStack.IsEmpty())
						{
							v2 = QualityInfo.GetTierColor(itemStack.itemValue.Quality);
						}

						value = durabilitycolorFormatter.Format(v2);
						return true;
					}
				case "durabilityfill":
					value = (itemStack.IsEmpty() ? "0" : ((itemStack.itemValue.MaxUseTimes == 0) ? "1" : durabilityfillFormatter.Format(((float)itemStack.itemValue.MaxUseTimes - itemStack.itemValue.UseTimes) / (float)itemStack.itemValue.MaxUseTimes)));
					return true;
				case "durabilityjustify":
					value = "center";
					if(!itemStack.IsEmpty() && this.itemClass != null && !this.itemClass.ShowQualityBar)
					{
						value = "right";
					}

					return true;
				case "durabilitytext":
					value = "";
					if(!itemStack.IsEmpty() && this.itemClass != null)
					{
						if(this.itemClass.ShowQualityBar)
						{
							value = ((itemStack.itemValue.Quality > 0) ? durabilitytextFormatter.Format(itemStack.itemValue.Quality) : "-");
						}
						else
						{
							value = ((this.itemClass.Stacknumber == 1) ? "" : durabilitytextFormatter.Format(itemStack.count));
						}
					}

					return true;
				case "itemtypeicon":
					value = "";
					if(!itemStack.IsEmpty() && this.itemClass != null)
					{
						if(this.itemClass.IsBlock())
						{
							value = Block.list[itemStack.itemValue.type].ItemTypeIcon;
						}
						else
						{
							if(this.itemClass.AltItemTypeIcon != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, itemStack.itemValue))
							{
								value = this.itemClass.AltItemTypeIcon;
								return true;
							}

							value = this.itemClass.ItemTypeIcon;
						}
					}

					return true;
				case "hasitemtypeicon":
					value = "false";
					if(!itemStack.IsEmpty() && this.itemClass != null)
					{
						if(this.itemClass.IsBlock())
						{
							value = (Block.list[itemStack.itemValue.type].ItemTypeIcon != "").ToString();
						}
						else
						{
							value = (this.itemClass.ItemTypeIcon != "").ToString();
						}
					}

					return true;
				case "itemtypeicontint":
					value = "255,255,255,255";
					if(!itemStack.IsEmpty() && this.itemClass != null && this.itemClass.Unlocks != "" && XUiM_ItemStack.CheckKnown(base.xui.playerUI.entityPlayer, this.itemClass, itemStack.itemValue))
					{
						value = altitemtypeiconcolorFormatter.Format(this.itemClass.AltItemTypeIconColor);
					}

					return true;
				case "shownormaloptions":
					value = (true).ToString();
					return true;
				case "showstats":
					value = showStats.ToString();
					return true;
				case "showdescription":
					value = (!showStats).ToString();
					return true;
				case "iscomparing":
					value = (!CompareStack.IsEmpty()).ToString();
					return true;
				case "isnotcomparing":
					value = CompareStack.IsEmpty().ToString();
					return true;
				case "showstatoptions":
					value = "false";
					return true;
				case "showonlydescription":
					if(selectedLootFilterEntry != null)
					{
						value = false.ToString();
					}
					else
						value = (!XUiM_ItemStack.HasItemStats(itemStack)).ToString();
					return true;
				case "showstatanddescription":
					if(selectedLootFilterEntry != null)
					{
						value = true.ToString();
					} else
					value = XUiM_ItemStack.HasItemStats(itemStack).ToString();
					return true;
				case "itemstattitle1":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(0) : "");
					return true;
				case "itemstat1":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(0) : "");
					return true;
				case "itemstattitle2":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(1) : "");
					return true;
				case "itemstat2":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(1) : "");
					return true;
				case "itemstattitle3":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(2) : "");
					return true;
				case "itemstat3":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(2) : "");
					return true;
				case "itemstattitle4":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(3) : "");
					return true;
				case "itemstat4":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(3) : "");
					return true;
				case "itemstattitle5":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(4) : "");
					return true;
				case "itemstat5":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(4) : "");
					return true;
				case "itemstattitle6":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(5) : "");
					return true;
				case "itemstat6":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(5) : "");
					return true;
				case "itemstattitle7":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatTitle(6) : "");
					return true;
				case "itemstat7":
					value = ((this.itemClass != null || this.selectedLootFilterEntry != null) ? GetStatValue(6) : "");
					return true;
				default:
					return false;
			}
		}

		
		public string GetStatTitle(int index)
		{
			if(this.selectedLootFilterEntry != null)
			{
				switch(index)
				{
					case 0:
						{
							return "DropItemCount";
						}
					default:
						return "something";
				}
			}
			if(itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
			{
				return "";
			}

			if(itemDisplayEntry.DisplayStats[index].TitleOverride != null)
			{
				return itemDisplayEntry.DisplayStats[index].TitleOverride;
			}

			return UIDisplayInfoManager.Current.GetLocalizedName(itemDisplayEntry.DisplayStats[index].StatType);
		}

		
		public string GetStatValue(int index)
		{
			if(this.selectedLootFilterItemStack != null)
			{
				if(itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
				{
					return "";
				}

				DisplayInfoEntry infoEntry = itemDisplayEntry.DisplayStats[index];
				if(!CompareStack.IsEmpty())
				{
					return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack, CompareStack, base.xui.playerUI.entityPlayer, infoEntry, flipCompare: false, useMods: false);
				}
				return XUiM_ItemStack.GetStatItemValueTextWithCompareInfo(itemStack, CompareStack, base.xui.playerUI.entityPlayer, infoEntry);
			}
			else if(selectedLootFilterEntry != null)
			{
				/*if(itemDisplayEntry == null || itemDisplayEntry.DisplayStats.Count <= index)
				{
					return "";
				}
				DisplayInfoEntry infoEntry = itemDisplayEntry.DisplayStats[index];
				switch(infoEntry.CustomName)
				{
					case "DropItemCount":
					{
						Log.Warning("DropItemCount");
							return selectedLootFilterEntry.entryData.dropItemStacks.ToArray().Length.ToString();
					}
				}*/
				switch(index)
				{
					case 0:
					{
						return selectedLootFilterEntry.entryData.dropItemStacks.ToArray().Length.ToString();
					}
				}
			}
			return "";
		}

		
		public void makeVisible(bool _makeVisible)
		{
			if(_makeVisible && windowGroup.isShowing)
			{
				base.ViewComponent.IsVisible = true;
				((XUiV_Window)viewComponent).ForceVisible(1f);
			}
		}
		public void SetInfo(ItemStack stack, XUiController controller, XUiC_LootFilterItemActionList.LootFilterItemActionListTypes actionListType)
		{
			bool flag = stack.itemValue.type == itemStack.itemValue.type && stack.count == itemStack.count;
			itemStack = stack.Clone();
			bool flag2 = itemStack != null && !itemStack.IsEmpty();
			if(itemPreview == null)
			{
				return;
			}

			if(!flag || !stack.itemValue.Equals(itemStack.itemValue))
			{
				compareStack = ItemStack.Empty.Clone();
			}

			itemClass = null;
			int num = 1;
			if(flag2)
			{
				itemClass = itemStack.itemValue.ItemClassOrMissing;
				if(itemClass != null)
				{
					if(itemClass is ItemClassQuest)
					{
						itemClass = ItemClassQuest.GetItemQuestById(itemStack.itemValue.Seed);
					}

					num = (itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicBundleSize : itemClass.EconomicBundleSize);
				}
			}

			if(itemClass != null)
			{
				itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag(itemClass.IsBlock() ? Block.list[itemStack.itemValue.type].DisplayType : itemClass.DisplayType);
			}
			mainActionItemList.SetCraftingActionList(actionListType, controller);
			isBuying = false;
			useCustomMarkup = false;

			if(flag2 && itemStack.itemValue.Modifications != null)
			{
				partList.SetMainItem(itemStack);
				if(itemStack.itemValue.CosmeticMods != null && itemStack.itemValue.CosmeticMods.Length != 0 && itemStack.itemValue.CosmeticMods[0] != null && !itemStack.itemValue.CosmeticMods[0].IsEmpty())
				{
					partList.SetSlot(itemStack.itemValue.CosmeticMods[0], 0);
					partList.SetSlots(itemStack.itemValue.Modifications, 1);
				}
				else
				{
					partList.SetSlots(itemStack.itemValue.Modifications);
				}

				partList.ViewComponent.IsVisible = true;
			}
			else
			{
				partList.ViewComponent.IsVisible = false;
			}

			RefreshBindings();
		}
		public void SetInfo(LootFilter stack, XUiController controller, XUiC_LootFilterItemActionList.LootFilterItemActionListTypes actionListType)
		{
			itemDisplayEntry = UIDisplayInfoManager.Current.GetDisplayStatsForTag("lootFilter");
			mainActionItemList.SetCraftingActionList(actionListType, controller);
			partList.ViewComponent.IsVisible = false;
			this.itemClass = null;
			RefreshBindings();
		}

		public XUiC_SelectableEntry HoverEntry
		{
			get
			{
				return hoverEntry;
			}
			set
			{
				if(hoverEntry == value)
				{
					return;
				}

				hoverEntry = value;
				if(hoverEntry != null && !hoverEntry.Selected && !itemStack.IsEmpty())
				{
					ItemStack hoverControllerItemStack = GetHoverControllerItemStack();
					if(!hoverControllerItemStack.IsEmpty() && XUiM_ItemStack.CanCompare(hoverControllerItemStack.itemValue.ItemClass, itemClass))
					{
						CompareStack = hoverControllerItemStack;
					}
					else
					{
						CompareStack = ItemStack.Empty;
					}
				}
				else
				{
					CompareStack = ItemStack.Empty;
				}
			}
		}
		public void ShowEmptyInfo()
		{
			if(emptyInfoWindow == null)
			{
				emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("lootfilter").GetChildById("emptyInfoPanel");
			}

			emptyInfoWindow.ViewComponent.IsVisible = true;
		}
		public override void Update(float _dt)
		{
			base.Update(_dt);
			if(IsDirty && base.ViewComponent.IsVisible)
			{
				if(emptyInfoWindow == null)
				{
					emptyInfoWindow = (XUiC_InfoWindow)base.xui.FindWindowGroupByName("lootfilter").GetChildById("emptyInfoPanel");
				}
				if(selectedLootFilterItemStack != null)
				{
					SetItemStack(selectedLootFilterItemStack);
				}
				if(selectedLootFilterEntry != null)
				{
					SetLootFilter(selectedLootFilterEntry);
				}

				IsDirty = false;
			}
		}
		public void SetItemStack(XUiC_LootFilterContentItemStack stack, bool _makeVisible = true)
		{
			if(stack == null || stack.LFItemStack == null || stack.LFItemStack.IsEmpty())
			{
				ShowEmptyInfo();
				return;
			}

			makeVisible(_makeVisible);
			selectedLootFilterItemStack = stack;
			selectedLootFilterEntry = null;
			SetInfo(stack.LFItemStack, stack, XUiC_LootFilterItemActionList.LootFilterItemActionListTypes.LootFilterItemStack);
		}
		public ItemStack GetHoverControllerItemStack()
		{
			if(hoverEntry is XUiC_LootFilterContentItemStack xUiC_LootFilterItemStack)
			{
				return xUiC_LootFilterItemStack.LFItemStack;
			}

			return null;
		}

		internal void SetLootFilter(XUiC_LootFilterEntry lootFilterEntry, bool _makeVisible = true)
		{
			if(lootFilterEntry == null || lootFilterEntry.entryData == null)
			{
				ShowEmptyInfo();
				return;
			}

			makeVisible(_makeVisible);
			selectedLootFilterItemStack = null;
			selectedLootFilterEntry = lootFilterEntry;
			SetInfo(lootFilterEntry.entryData, lootFilterEntry, XUiC_LootFilterItemActionList.LootFilterItemActionListTypes.LootFilter);
		}
	}
}
