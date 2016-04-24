using Newtonsoft.Json;
using VRageMath;

namespace WebAPIPlugin
{
    public struct WebVector3D
    {
        [JsonProperty("x")]
        public double X;

        [JsonProperty("y")]
        public double Y;

        [JsonProperty("z")]
        public double Z;

        public WebVector3D(Vector3D vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }
    }
}
