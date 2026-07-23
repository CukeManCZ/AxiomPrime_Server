using System;
using System.Collections.Generic;
using Utilities.DataStructures;

[Serializable]
public class BoolRow
{
    public List<bool> row = new List<bool>();
}

public class ItemData
{
    public required string subType;
    public required string itemName;
    public required string itemDescription;

    public List<BoolRow> space = new List<BoolRow>();

    public CustomGrid<bool> GetGrid()
    {
        if (space == null || space.Count == 0 || space[0].row.Count == 0)
            return new CustomGrid<bool>(1, 1, true);

        int width = space.Count;
        int height = space[0].row.Count;

        CustomGrid<bool> grid = new CustomGrid<bool>(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid.SetValue(x, y, space[x].row[y]);
            }
        }

        return grid;
    }

    public static List<string> GetItemNamesWithSubType(string subType, List<ItemData> items)
    {
        if (items == null || string.IsNullOrEmpty(subType))
            return new List<string>();

        List<string> names = new List<string>();

        foreach (ItemData item in items)
        {
            if (item.subType == subType)
                names.Add(item.itemName);
        }

        return names;
    }

    public static ItemData? GetItemDataWithName(string name, List<ItemData> items)
    {
        if (items == null || string.IsNullOrEmpty(name))
            return null;

        foreach (ItemData item in items)
            if (item.itemName == name)
                return item;

        return null;
    }
}
