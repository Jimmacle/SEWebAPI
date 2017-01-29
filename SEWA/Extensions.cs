using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI;
using VRageMath;

namespace SEWA
{
    public static class Extensions
    {
        public static Type GetValueType(this ITerminalProperty prop)
        {
            if (!(prop is ITerminalProperty<object>))
                return null;

            var type = prop.GetType();
            return type.GenericTypeArguments[0];
        }

        public static bool TrySetValue(this ITerminalProperty prop, IMyTerminalBlock block, string value)
        {
            if (prop is ITerminalProperty<bool> pBool)
            {
                if (!bool.TryParse(value, out bool bVal))
                    return false;
                pBool.SetValue(block, bVal);
            }
            else if (prop is ITerminalProperty<float> pFloat)
            {
                if (!float.TryParse(value, out float fVal))
                    return false;
                pFloat.SetValue(block, fVal);
            }
            else if (prop is ITerminalProperty<Color> pColor)
            {
                var bytes = StringToByteArray(value);
                if (bytes.Length != 4)
                    return false;
                var c = new Color {R = bytes[0], G = bytes[1], B = bytes[2], A = bytes[3]};
                pColor.SetValue(block, c);
            }
            else if (prop is ITerminalProperty<StringBuilder> pSb)
            {
                pSb.SetValue(block, new StringBuilder(value));
            }

            return true;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
