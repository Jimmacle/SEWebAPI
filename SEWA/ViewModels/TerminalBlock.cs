using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Definitions;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.ComponentSystem;
using static Sandbox.ModAPI.Ingame.TerminalBlockExtentions;

namespace SEWA.ViewModels
{
    public class TerminalBlock
    {
        public string Type { get; }
        public string Name { get; }
        public long Id { get; }
        public List<Inventory> Inventories { get; } = new List<Inventory>();
        public List<BlockProperty> Properties { get; } = new List<BlockProperty>();
        public List<string> Actions { get; } = new List<string>();

        public TerminalBlock(IMyTerminalBlock block, bool light = true)
        {
            Name = block.CustomName;
            var def = block.BlockDefinition;
            Type = $"{def.TypeId}/{def.SubtypeId}";
            Id = block.EntityId;

            if (light)
                return;

            if (block.HasInventory())
            {
                var count = block.GetInventoryCount();
                for (var i = 0; i < count; i++)
                {
                    Inventories.Add(new Inventory((IMyInventory)block.GetInventory(i)));
                }
            }

            block.GetProperties(new List<ITerminalProperty>(), p =>
            {
                Properties.Add(new BlockProperty(p, block));
                return false;
            });

            foreach (var action in MyTerminalControlFactory.GetActions(block.GetType()))
            {
                Actions.Add(action.Id);
            }
        }

        /*
        public TerminalBlock(MyObjectBuilder_TerminalBlock blockBuilder, bool light = true)
        {
            Name = blockBuilder.CustomName;
            Type = $"{blockBuilder.TypeId}/{blockBuilder.SubtypeId}";
            Id = blockBuilder.EntityId;

            if (light)
                return;

            foreach (var comp in blockBuilder.ComponentContainer.Components)
            {
                if (comp.Component is MyObjectBuilder_InventoryBase inventory)
                {
                    Inventories.Add(new Inventory(inventory));
                }

                if (comp.Component is myobjectbuilder_proper)
            }
        }*/
    }
}
