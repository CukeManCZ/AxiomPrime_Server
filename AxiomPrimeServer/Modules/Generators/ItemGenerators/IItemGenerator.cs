using System;
using System.Collections.Generic;

public interface IItemGenerator
{
    Item GenerateItem(int playerLvl);
    Item GenerateItem(int playerLvl, ItemType itemType);

    void Initialize(
        List<ItemData> itemData,
        float itemValueModifier,
        float itemValueRandomRange);
}