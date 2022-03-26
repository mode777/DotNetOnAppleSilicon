using System.Xml.Linq;
using XPlat.Engine.Serialization;

namespace XPlat.Engine
{
    [SceneElement("scene")]
    public class Scene : ISceneElement
    {
        public Scene()
        {
            RootNode = new Node();
        }

        public Node FindNode(string name) => RootNode.Find(name);

        public Scene(Node rootNode) 
        {
            this.RootNode = rootNode;
               
        }
        public Node RootNode { get; private set; }

        public void Init() => RootNode.Init();
        public void Update() => RootNode.Update();

        public void Parse(XElement el, SceneReader reader)
        {
            foreach(var i in el.Elements("import")){
                reader.ReadElement(i);
            }
            var rootEl = el.Element("node") ?? throw new InvalidDataException("A scene needs a root note element");
            RootNode = (Node)reader.ReadElement(rootEl);
        }
    }
}