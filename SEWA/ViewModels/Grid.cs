using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace SEWA.ViewModels
{
    public class Grid
    {
        public string Name { get; }
        public Vector3D Position { get; }
        public Quaternion Orientation { get; }
        public List<TerminalBlock> Blocks { get; } = new List<TerminalBlock>();

        public Grid(IMyCubeGrid grid)
        {
            Name = grid.DisplayName;
            Position = grid.PositionComp.GetPosition();
            Orientation = Quaternion.CreateFromRotationMatrix(grid.PositionComp.WorldMatrix.GetOrientation());

            grid.GetBlocks(new List<IMySlimBlock>(), b =>
            {
                if (b.FatBlock is IMyTerminalBlock block)
                {
                    Blocks.Add(new TerminalBlock(block));
                }
                return false;
            });
        }

        /*
        public Grid(MyObjectBuilder_CubeGrid gridBuilder)
        {
            Name = gridBuilder.DisplayName;
            Position = gridBuilder.PositionAndOrientation.Value.Position;
            Orientation = gridBuilder.PositionAndOrientation.Value.Orientation;

            foreach (var block in gridBuilder.CubeBlocks)
            {
                block
            }
        }*/
    }
}
