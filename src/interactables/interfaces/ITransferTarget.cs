using System.Collections.Generic;
using Godot;

namespace Necromation;

public interface ITransferTarget
{
    public bool CanAcceptItems(string item, int count = 1, Vector2 position = default);
    public void Insert(string item, int count = 1, Vector2 position = default);
    public bool Remove(string item, int count = 1);
    public string GetFirstItem();
    public List<Inventory> GetInventories();
}