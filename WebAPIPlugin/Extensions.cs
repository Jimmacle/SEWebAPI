using Newtonsoft.Json;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Net;
using System.Text;
using VRage.Game.ModAPI.Ingame;

namespace WebAPIPlugin
{
    public static class Extensions
    {
        public static void Respond(this HttpListenerContext context, object data, int statusCode)
        {
            try
            {
                var response = context.Response;
                response.StatusCode = statusCode;

                string serialized = JsonConvert.SerializeObject(data, Formatting.Indented);
                byte[] bytes = Encoding.ASCII.GetBytes(serialized);
                response.ContentLength64 = bytes.Length;

                response.OutputStream.Write(bytes, 0, bytes.Length);
                response.OutputStream.Close();
            }
            catch
            {
                MySandboxGame.Log.WriteLineAndConsole("SEWA: Error writing stream");
            }
        }

        public static List<IMyTerminalBlock> GetTerminalBlocks(this IMyCubeGrid grid)
        {
            var result = new List<IMyTerminalBlock>();
            foreach (var block in (grid as MyCubeGrid).CubeBlocks)
            {
                if (block?.FatBlock is IMyTerminalBlock)
                {
                    result.Add(block.FatBlock as IMyTerminalBlock);
                }
            }
            return result;
        }
    }
}
