using System.Reflection.Emit;
using System.Text;
using NLog;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI;
using VRageMath;

namespace SEWA.ViewModels
{ 
    public class BlockProperty
    {
        private static readonly Logger Log = LogManager.GetLogger(nameof(BlockProperty));

        public string Name { get; }
        public string Type { get; }
        public object Value { get; }

        public BlockProperty(ITerminalProperty property, IMyTerminalBlock block)
        {
            Name = property.Id;
            Type = property.TypeName;

            if (property is ITerminalProperty<bool> pBool)
                Value = pBool.GetValue(block);
            else if (property is ITerminalProperty<float> pFloat)
                Value = pFloat.GetValue(block);
            else if (property is ITerminalProperty<Color> pColor)
                Value = pColor.GetValue(block);
            else if (property is ITerminalProperty<StringBuilder> pSb)
                Value = pSb.GetValue(block).ToString();
            else
            {
#if DEBUG
                Log.Warn($"Falling back to reflection for type `{Type}`");
                var propType = property.GetType();
                if (propType.IsGenericType)
                {
                    var getValueMethod = propType.GetMethod("GetValue", new[] { typeof(IMyCubeBlock) });
                    Value = getValueMethod.Invoke(property, new object[] { block });
                }
#else
                Log.Warn($"Property {Type} not supported");
#endif
            }
        }
    }
}
