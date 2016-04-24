using Newtonsoft.Json;
using VRageMath;

namespace WebAPIPlugin
{
    public struct WebQuatD
    {
        [JsonProperty("x")]
        public double X;

        [JsonProperty("y")]
        public double Y;

        [JsonProperty("z")]
        public double Z;

        [JsonProperty("w")]
        public double W;

        public WebQuatD(QuaternionD quat)
        {
            this.X = quat.X;
            this.Y = quat.Y;
            this.Z = quat.Z;
            this.W = quat.W;
        }
    }
}
