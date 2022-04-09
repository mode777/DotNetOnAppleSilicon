using System.Xml.Linq;
using XPlat.Engine.Serialization;
using XPlat.LuaScripting;

namespace XPlat.Engine.Components
{
    [SceneElement("lua")]
    public class LuaScriptComponent : Behaviour
    {
        public ScriptResource Resource { 
            get => _resource; 
            private set { 
                _resource = value;
                _resource.Changed += (s,a) => reload = true;
            } 
        }
        public LuaScriptInstance Instance { get; private set; }
        private bool reload;
        private ScriptResource _resource;

        public override void Init()
        {
            LoadScript();
        }

        private void LoadScript()
        {
            if(Resource == null) return;
            Instance = Resource.Script.Instantiate(Node);
            if(Instance == null) return;
            Instance.OnError += (s,e) => System.Console.WriteLine(e.Message);
            reload = false;
            Instance.Init();
        }

        public override void Parse(XElement el, SceneReader reader)
        {
            if (el.TryGetAttribute("res", out var res)) Resource = (ScriptResource)reader.Scene.Resources.Load(res);
            else throw new InvalidDataException("script resource needs 'ref' attribute");
            if (el.TryGetAttribute("src", out var src)) throw new NotImplementedException("Src attribute is no longer supported for scripts");
            base.Parse(el, reader);
        }

        public override void Update()
        {
            if (reload) LoadScript();
            Instance?.Update();
        }
    }
}

