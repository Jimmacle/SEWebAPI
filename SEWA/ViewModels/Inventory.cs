using System.Collections.Generic;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace SEWA.ViewModels
{
    public class Inventory
    {
        public float CurrentVolume { get; }
        public float MaxVolume { get; }
        public List<InventoryItem> Items { get; } = new List<InventoryItem>();

        public Inventory(IMyInventory inventory)
        {
            CurrentVolume = (float)inventory.CurrentVolume;
            MaxVolume = (float)inventory.MaxVolume;

            foreach (var item in inventory.GetItems())
            {
                Items.Add(new InventoryItem((IMyInventoryItem)item));
            }
        }
    }
}
