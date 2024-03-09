using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Necromation;

public interface ITransferTarget
{
    public bool CanAcceptItems(string item, int count = 1) => GetMaxTransferAmount(item) >= count;
    public void Insert(string item, int count = 1);
    public bool Remove(string item, int count = 1);
    public List<string> GetItems();
    public string GetFirstItem();
    public List<Inventory> GetInventories();

    public int GetMaxTransferAmount(string itemType)
    {
        return int.MaxValue;
    }

    public Inventory GetInputInventory()
    {
        return GetInventories().First();
    }
    
    public Inventory GetOutputInventory()
    {
        return GetInventories().Count > 1 ? GetInventories().Last() : GetInventories().First();
    }
}