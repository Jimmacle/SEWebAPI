using Newtonsoft.Json;
using Sandbox.Game.Entities;
using VRageMath;

namespace WebAPIPlugin
{
    public struct WebGrid
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("blocks")]
        public long BlockCount;
        [JsonProperty("pos")]
        public WebVector3D Position;
        [JsonProperty("rot")]
        public WebQuatD Rotation;

        public WebGrid(MyCubeGrid grid)
        {
            this.Name = grid.DisplayName;
            this.BlockCount = grid.BlocksCount;
            this.Position = new WebVector3D(grid.WorldMatrix.Translation);
            this.Rotation = new WebQuatD(QuaternionD.CreateFromRotationMatrix(grid.WorldMatrix.GetOrientation()));
        }
    }
}
