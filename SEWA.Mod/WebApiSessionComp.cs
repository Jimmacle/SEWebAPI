using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.ModAPI;
using VRage.Utils;

namespace SEWA.Mod
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class WebApiSessionComp : MySessionComponentBase
    {
        private IMyTerminalControlCheckbox _showQrControl;
        private IMyTerminalControlTextbox _keyControl;
        private IMyTerminalControlButton _genControl;
        private bool _init;

        public override void UpdateAfterSimulation()
        {
            if (_init || MyAPIGateway.TerminalControls == null || MyAPIGateway.Multiplayer == null)
                return;

            _init = true;

            //Don't do anything in this class on the server
            if (MyAPIGateway.Session.IsServer)
                return;

            MyAPIGateway.TerminalControls.CustomControlGetter += TerminalControlsOnCustomControlGetter;
            MyAPIGateway.Multiplayer.RegisterMessageHandler(7331, UpdateLocalKey);
            InitControls();
        }

        private void UpdateLocalKey(byte[] bytes)
        {
            var id = BitConverter.ToInt64(bytes, 0);
            var key = BitConverter.ToString(bytes, 8);

            if (!MyAPIGateway.Entities.EntityExists(id))
                return;

            var logic = MyAPIGateway.Entities.GetEntityById(id)?.GameLogic?.GetAs<WebApiBlockComp>();
            if (logic == null)
                return;
            logic.LocalKey = key;
            _keyControl.UpdateVisual();
        }

        private static void RequestKey(IMyEntity entity, bool generateNew = false)
        {
            var data = new byte[9];
            data[8] = generateNew ? (byte)1 : (byte)0;
            var id = BitConverter.GetBytes(entity.EntityId);
            Array.Copy(id, data, 8);

            MyAPIGateway.Multiplayer.SendMessageToServer(7331, data);
        }

        private void InitControls()
        {
            _showQrControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyTextPanel>("");
            _showQrControl.Title = MyStringId.GetOrCompute("Show QR Code");
            _showQrControl.Getter = b => ((IMyTextPanel)b).ShowOnScreen == ShowTextOnScreenFlag.PUBLIC;
            _showQrControl.Setter = (b, v) =>
            {
                ((IMyTextPanel)b).SetShowOnScreen(v ? ShowTextOnScreenFlag.PUBLIC : ShowTextOnScreenFlag.NONE);
            };

            _keyControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTextPanel>("");
            _keyControl.Title = MyStringId.GetOrCompute("API Key");
            _keyControl.Getter = b =>
            {
                var logic = b.GameLogic.GetAs<WebApiBlockComp>();
                if (string.IsNullOrEmpty(logic?.LocalKey))
                    RequestKey(b);

                return new StringBuilder(logic?.LocalKey ?? "");
            };
            _keyControl.Setter = (b, v) => { };

            _genControl = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTextPanel>("");
            _genControl.Title = MyStringId.GetOrCompute("Generate New Key");
            _genControl.Action = b => RequestKey(b, true);
        }

        private void TerminalControlsOnCustomControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (!(block is IMyTextPanel) || !block.BlockDefinition.SubtypeId.Contains("WebAPI"))
                return;

            controls.RemoveAll(x => !(x is IMyTerminalControlOnOffSwitch));
            controls.Add(_keyControl);
            controls.Add(_genControl);
            controls.Add(_showQrControl);
        }

        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= TerminalControlsOnCustomControlGetter;
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(7331, UpdateLocalKey);
        }
    }
}