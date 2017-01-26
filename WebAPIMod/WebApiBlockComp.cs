using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.ObjectBuilders;

namespace WebAPIMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel))]
    public class WebApiBlockComp : MyGameLogicComponent
    {
        private MyObjectBuilder_EntityBase _objectBuilder;
        public string LocalKey { get; set; }

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;
            base.Init(objectBuilder);
        }

        public override MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return copy ? (MyObjectBuilder_EntityBase)_objectBuilder.Clone() : _objectBuilder;
        }
    }
}
