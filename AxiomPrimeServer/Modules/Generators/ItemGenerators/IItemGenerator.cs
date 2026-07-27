using System;
using System.Collections.Generic;

public interface IItemGenerator
{
    Item_Database GenerateItem(int playerLvl);
    Item_Database GenerateItem(int playerLvl, ItemType itemType);

    void Initialize(
        List<ItemData> itemData,
        float itemValueModifier,
        float itemValueRandomRange);
}