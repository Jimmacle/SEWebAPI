using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Utils;

namespace WebAPIMod
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class SEWATerminal : MySessionComponentBase
    {
        private bool init = false;

        public override void UpdateBeforeSimulation()
        {
            if (!init && MyAPIGateway.TerminalControls != null)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter += ControlGetter;
                init = true;
            }
        }

        private void ControlGetter(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block.BlockDefinition.SubtypeId == "WebAPI" || block.BlockDefinition.SubtypeId == "SmallWebAPI" )
            {
                //Remove default LCD controls except for the on/off toggles.
                controls.RemoveAll(x => !(x is IMyTerminalControlOnOffSwitch));

                //Only show the controls to the owner of the block.
                if (MyAPIGateway.Session.Player.IdentityId == block.OwnerId)
                {
                    var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyTextPanel>("Separator");

                    //Show QR Code (On/Off)
                    var qrToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTextPanel>("Display");
                    qrToggle.Title = MyStringId.GetOrCompute("Show QR Code");
                    qrToggle.Getter = x => (x as IMyTextPanel).ShowOnScreen == ShowTextOnScreenFlag.PUBLIC;
                    qrToggle.Setter = (x, y) =>
                    {
                        (x as IMyTextPanel).SetShowOnScreen(y ? ShowTextOnScreenFlag.PUBLIC : ShowTextOnScreenFlag.NONE);
                    };
                    qrToggle.OffText = MyStringId.GetOrCompute("Off");
                    qrToggle.OnText = MyStringId.GetOrCompute("On");

                    //API Key (Textbox)
                    var keyTextbox = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyTextPanel>("Key");
                    keyTextbox.Title = MyStringId.GetOrCompute("API Key");
                    keyTextbox.Getter = x => new StringBuilder((x as IMyTextPanel).GetPrivateText());
                    keyTextbox.Setter = (x, y) => { };

                    //Generate New Key (Button)
                    var genButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyTextPanel>("Generate");
                    genButton.Title = MyStringId.GetOrCompute("Generate New Key");
                    genButton.Action = x => MyAPIGateway.Multiplayer.SendMessageToServer(7331, BitConverter.GetBytes(x.EntityId));

                    //Add controls
                    controls.Add(separator);
                    controls.Add(qrToggle);
                    controls.Add(keyTextbox);
                    controls.Add(genButton);
                }
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= ControlGetter;
        }
    }
}
