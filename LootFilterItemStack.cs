using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LootFilter
{
	public class LootFilterItemStack : ItemStack
	{
		public Boolean isDropItem = false;
		public Boolean isScrapItem = false;
		public int Index = 0;
		public static new LootFilterItemStack Empty = new LootFilterItemStack(ItemValue.None, 0,false, false);
		public LootFilterItemStack(ItemValue _itemValue, int _count, Boolean isDropItem = false, Boolean isScrapItem = false)
		{
			itemValue = _itemValue;
			count = _count;
			this.isDropItem = isDropItem;
			this.isScrapItem = isScrapItem;
		}
		public LootFilterItemStack(ItemStack itemStack)
		{
			itemValue = itemStack.itemValue.Clone();
			count = itemStack.count;
		}

		public new LootFilterItemStack Clone()
		{
			if(itemValue != null)
			{
				return new LootFilterItemStack(itemValue.Clone(), count, isDropItem, isScrapItem);
			}

			return new LootFilterItemStack(ItemValue.None.Clone(), count, isDropItem, isScrapItem);
		}
		public static new LootFilterItemStack[] CreateArray(int _size)
		{
			LootFilterItemStack[] array = new LootFilterItemStack[_size];
			for(int i = 0; i < array.Length; i++)
			{
				array[i] = Empty.Clone();
			}

			return array;
		}
	}
}
