using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace WebAPIMod
{
    /// <summary>
    /// Client-side component so the player can configure API blocks.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Client : MySessionComponentBase
    {
        private bool init = false;

        private IMyTextPanel selectedBlock;

        public override void UpdateBeforeSimulation()
        {
            //Uncomment this in production.
            //if (!MyAPIGateway.Multiplayer.IsServer)
            {
                //Initialize stuff.
                if (!init)
                {
                    MyAPIGateway.Utilities.MessageEntered += MessageEntered;
                    init = true;
                }

                //Handle player mouse input.
                if (MyAPIGateway.Input.IsNewPrimaryButtonPressed())
                {
                    //Find the API block the player clicked on.
                    var targetGrid = MyCubeGrid.GetTargetGrid();
                    if (targetGrid != null)
                    {
                        var cameraMatrix = MatrixD.Invert(MyAPIGateway.Session.CameraController.GetViewMatrix());

                        var blockPosition = targetGrid.RayCastBlocks(cameraMatrix.Translation, cameraMatrix.Translation + 15 * cameraMatrix.Forward);
                        if (blockPosition != null)
                        {
                            var targetBlock = (targetGrid.GetCubeBlock((Vector3I)blockPosition) as IMySlimBlock);
                            if (targetBlock != null && targetBlock.FatBlock != null)
                            {
                                var objectBuilder = targetBlock.GetObjectBuilder();
                                if (objectBuilder.SubtypeName == "WebAPI")
                                {
                                    IMyTextPanel apiBlock = targetBlock.FatBlock as IMyTextPanel;

                                    if (apiBlock != selectedBlock)
                                    {
                                        selectedBlock = apiBlock;
                                        MyAPIGateway.Utilities.ShowMessage("WebAPI", "Selected " + targetGrid.DisplayName + " - " + selectedBlock.CustomName.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                else if (MyAPIGateway.Input.IsNewSecondaryButtonPressed() && selectedBlock != null)
                {
                    selectedBlock = null;
                    MyAPIGateway.Utilities.ShowMessage("WebAPI", "Cleared Selection");
                }
            }
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        private void MessageEntered(string message, ref bool sendToOthers)
        {
            if (message.StartsWith("/web"))
            {
                sendToOthers = false;

                //TODO: Add commands here.
            }
        }
    }
}
