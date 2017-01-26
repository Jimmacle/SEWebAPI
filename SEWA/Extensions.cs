using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ModAPI;

namespace SEWA
{
    public static class Extensions
    {
        public static T GetValue<T>(this ITerminalProperty prop, IMyCubeBlock block)
        {
            if (prop is ITerminalProperty<T> tProp)
                return tProp.GetValue(block);

            return default(T);
        }

        public static object GetValue(this ITerminalProperty prop, IMyCubeBlock block)
        {
            if (prop is ITerminalProperty<object> objProp)
                return objProp.GetValue(block);

            return null;
        }

        public static Type GetValueType(this ITerminalProperty prop)
        {
            if (!(prop is ITerminalProperty<object>))
                return null;

            var type = prop.GetType();
            return type.GenericTypeArguments[0];
        }
    }
}
