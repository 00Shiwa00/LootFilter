using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootFilter
{
	public class XUiC_LootFilterItemActionList : XUiC_ItemActionList
	{

		public enum LootFilterItemActionListTypes
		{
			None,
			LootFilter,
			LootFilterItemStack
		}

		public void SetCraftingActionList(LootFilterItemActionListTypes _actionListType, XUiController itemController)
		{
			for(int i = 0; i < itemActionEntries.Count; i++)
			{
				itemActionEntries[i].DisableEvents();
			}

			itemActionEntries.Clear();
			switch(_actionListType)
			{
				/*
				case ItemActionListTypes.Crafting:
					{
						XUiC_RecipeEntry xUiC_RecipeEntry = (XUiC_RecipeEntry)itemController;
						List<XUiC_CraftingWindowGroup> childrenByType = base.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
						XUiC_CraftingWindowGroup xUiC_CraftingWindowGroup = null;
						for(int j = 0; j < childrenByType.Count; j++)
						{
							if(childrenByType[j].WindowGroup != null && childrenByType[j].WindowGroup.isShowing)
							{
								xUiC_CraftingWindowGroup = childrenByType[j];
								break;
							}
						}

						bool flag = xUiC_CraftingWindowGroup == null || (xUiC_CraftingWindowGroup.Workstation == "" && xUiC_RecipeEntry.Recipe.craftingArea == "");
						int craftingTier = -1;
						if(xUiC_CraftingWindowGroup != null)
						{
							craftingTier = xUiC_CraftingWindowGroup.GetChildByType<XUiC_CraftingInfoWindow>().SelectedCraftingTier;
							Block blockByName = Block.GetBlockByName(xUiC_CraftingWindowGroup.Workstation);
							if(blockByName != null && blockByName.Properties.Values.ContainsKey("Workstation.CraftingAreaRecipes"))
							{
								string text = blockByName.Properties.Values["Workstation.CraftingAreaRecipes"];
								string[] array = new string[1] { text };
								if(text.Contains(","))
								{
									array = text.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",")
										.Split(',', StringSplitOptions.None);
								}

								for(int k = 0; k < array.Length; k++)
								{
									flag = ((!array[k].EqualsCaseInsensitive("player")) ? (flag | array[k].EqualsCaseInsensitive(xUiC_RecipeEntry.Recipe.craftingArea)) : (flag | string.IsNullOrEmpty(xUiC_RecipeEntry.Recipe.craftingArea)));
								}
							}
							else
							{
								flag |= xUiC_CraftingWindowGroup.Workstation.EqualsCaseInsensitive(xUiC_RecipeEntry.Recipe.craftingArea);
							}
						}

						if(flag)
						{
							if(XUiM_Recipes.GetRecipeIsUnlocked(base.xui, xUiC_RecipeEntry.Recipe))
							{
								AddActionListEntry(new ItemActionEntryCraft(itemController, craftCountControl, craftingTier));
								AddActionListEntry(new ItemActionEntryFavorite(itemController, xUiC_RecipeEntry.Recipe));
							}
							else
							{
								HandleUnlockedBy(itemController, xUiC_RecipeEntry);
							}
						}
						else if(!XUiM_Recipes.GetRecipeIsUnlocked(base.xui, xUiC_RecipeEntry.Recipe))
						{
							HandleUnlockedBy(itemController, xUiC_RecipeEntry);
						}

						if(xUiC_RecipeEntry.Recipe.IsTrackable)
						{
							AddActionListEntry(new ItemActionEntryTrackRecipe(itemController, craftCountControl, craftingTier));
						}

						break;
					}
				case ItemActionListTypes.Equipment:
				case ItemActionListTypes.Part:
					{
						if(itemController is XUiC_VehiclePartStack xUiC_VehiclePartStack)
						{
							if(xUiC_VehiclePartStack.SlotType != "chassis")
							{
								AddActionListEntry(new ItemActionEntryTake(itemController));
							}

							break;
						}

						if(itemController is XUiC_ItemPartStack xUiC_ItemPartStack)
						{
							ItemStack itemStack2 = xUiC_ItemPartStack.ItemStack;
							if(itemStack2 != null && (itemStack2.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment)
							{
								AddActionListEntry(new ItemActionEntryTake(itemController));
							}

							break;
						}

						if(itemController is XUiC_ItemCosmeticStack)
						{
							AddActionListEntry(new ItemActionEntryTake(itemController));
							break;
						}

						XUiC_EquipmentStack xUiC_EquipmentStack = (XUiC_EquipmentStack)itemController;
						if(xUiC_EquipmentStack == base.xui.AssembleItem.CurrentEquipmentStackController)
						{
							AddActionListEntry(new ItemActionEntryAssemble(itemController));
							break;
						}

						AddActionListEntry(new ItemActionEntryTake(itemController));
						ItemStack itemStack3 = xUiC_EquipmentStack.ItemStack;
						if(!itemStack3.IsEmpty())
						{
							ItemValue itemValue2 = itemStack3.itemValue;
							if(itemValue2.Modifications.Length != 0 || itemValue2.CosmeticMods.Length != 0)
							{
								AddActionListEntry(new ItemActionEntryAssemble(itemController));
							}
						}

						break;
					}
				case ItemActionListTypes.Item:
					{
						XUiC_ItemStack xUiC_ItemStack = (XUiC_ItemStack)itemController;
						ItemStack itemStack = xUiC_ItemStack.ItemStack;
						ItemValue itemValue = itemStack.itemValue;
						ItemClass itemClass = itemStack.itemValue.ItemClass;
						if(itemClass == null)
						{
							break;
						}

						if(xUiC_ItemStack is XUiC_Creative2Stack)
						{
							AddActionListEntry(new ItemActionEntryTake(itemController));
							bool equipFound = true;
							AddActionActions(itemValue, itemClass, xUiC_ItemStack, ref equipFound);
							if(XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
							{
								AddActionListEntry(new ItemActionEntryRecipes(itemController));
							}

							AddActionListEntry(new CreativeActionEntryFavorite(itemController, itemStack.itemValue.type));
						}
						else
						{
							if(itemStack.IsEmpty())
							{
								break;
							}

							if(xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Vehicle)
							{
								AddActionListEntry(new ItemActionEntryTake(itemController));
							}

							if(xUiC_ItemStack is XUiC_RequiredItemStack)
							{
								break;
							}

							if((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && xUiC_ItemStack == base.xui.AssembleItem.CurrentItemStackController)
							{
								AddActionListEntry(new ItemActionEntryAssemble(itemController));
								break;
							}

							if(base.xui.Trader.Trader != null)
							{
								if(base.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine2)
								{
									if(base.xui.Trader.Trader.TraderInfo.AllowSell || (!base.xui.Trader.Trader.TraderInfo.AllowSell && tileEntityVendingMachine2.LocalPlayerIsOwner()) || tileEntityVendingMachine2.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
									{
										AddActionListEntry(new ItemActionEntrySell(itemController));
									}
								}
								else if(base.xui.Trader.Trader.TraderInfo.AllowSell)
								{
									AddActionListEntry(new ItemActionEntrySell(itemController));
								}

								break;
							}

							if(itemClass.IsEquipment && base.xui.AssembleItem.CurrentItem == null)
							{
								AddActionListEntry(new ItemActionEntryWear(itemController));
							}

							bool equipFound2 = false;
							if(itemClass is ItemClassBlock && xUiC_ItemStack.StackLocation != XUiC_ItemStack.StackLocationTypes.ToolBelt && !xUiC_ItemStack.AssembleLock)
							{
								ItemActionEntryEquip actionEntry = new ItemActionEntryEquip(itemController);
								AddActionListEntry(actionEntry);
								equipFound2 = true;
							}

							AddActionActions(itemValue, itemClass, xUiC_ItemStack, ref equipFound2);
							if(itemValue.MaxUseTimes > 0 && itemValue.UseTimes > 0f && itemClass.RepairTools != null && itemClass.RepairTools.Length > 0 && itemClass.RepairTools[0].Value.Length > 0)
							{
								AddActionListEntry(new ItemActionEntryRepair(itemController));
							}

							if((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && (itemValue.Modifications.Length != 0 || itemValue.CosmeticMods.Length != 0))
							{
								AddActionListEntry(new ItemActionEntryAssemble(itemController));
							}

							Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
							if(scrapableRecipe != null && scrapableRecipe.CanCraft(new ItemStack[1] { itemStack }))
							{
								AddActionListEntry(new ItemActionEntryScrap(itemController));
							}

							if((xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xUiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && base.xui.AssembleItem.CurrentItemStackController == null && XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
							{
								AddActionListEntry(new ItemActionEntryRecipes(itemController));
							}

							AddActionListEntry(new ItemActionEntryDrop(itemController));
						}

						break;
					}
				case ItemActionListTypes.Trader:
					if(base.xui.Trader.Trader.TraderInfo.AllowBuy)
					{
						AddActionListEntry(new ItemActionEntryPurchase(itemController));
					}

					if(base.xui.Trader.TraderTileEntity is TileEntityVendingMachine tileEntityVendingMachine && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
					{
						AddActionListEntry(new ItemActionEntryMarkup(itemController));
						AddActionListEntry(new ItemActionEntryMarkdown(itemController));
						AddActionListEntry(new ItemActionEntryResetPrice(itemController));
					}

					break;
				*/
				case LootFilterItemActionListTypes.None:
					break;
				case LootFilterItemActionListTypes.LootFilter:
					break;
				case LootFilterItemActionListTypes.LootFilterItemStack:
					break;
			}

			isDirty = true;
		}


	}
}
