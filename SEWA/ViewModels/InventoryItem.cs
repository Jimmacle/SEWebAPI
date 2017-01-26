using Sandbox.Game.Entities.Inventory;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;

namespace SEWA.ViewModels
{
    public class InventoryItem
    {
        public string Type { get; }
        public float Amount { get; }
        public float Mass { get; }
        public float Volume { get; }

        public InventoryItem(IMyInventoryItem item)
        {
            var content = item.Content;
            var adapter = MyInventoryItemAdapter.Static;
            adapter.Adapt(content.GetId());

            Amount = (float)item.Amount;
            Mass = adapter.Mass * Amount;
            Volume = adapter.Volume * Amount;
            Type = $"{content.TypeId}/{content.SubtypeName}";
        }
    }
}