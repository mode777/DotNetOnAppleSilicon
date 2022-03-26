using GLES2;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace net6test
{
    public static class GltfLoader
    {
        public static Scene LoadScene(SharpGLTF.Schema2.ModelRoot root)
        {
            var scene = new Scene();
            foreach (var n in root.DefaultScene.VisualChildren)
            {
                scene.RootNode.AddChild(LoadNode(n));
            }
            return scene;
        }

        public static Node LoadNode(SharpGLTF.Schema2.Node node)
        {
            var mynode = new Node(); 
            var gltfTrans = node.LocalTransform;
            mynode.Transform.Rotation = gltfTrans.Rotation;
            mynode.Transform.Translation = gltfTrans.Translation;
            mynode.Transform.Scale = gltfTrans.Scale;
            mynode.Name = node.Name;
            if(node.Mesh != null)
            {
                var prims = node.Mesh.Primitives.Select(x => LoadPrimitive(x)).ToArray();
                var mesh = new Mesh(prims);
                var renderer = new RendererComponent();
                renderer.Mesh = mesh;
                mynode.AddComponent(renderer);
            }
            return mynode;
        }

        public static Primitive LoadPrimitive(SharpGLTF.Schema2.MeshPrimitive prim)
        {
            var indices = prim.GetIndexAccessor().AsIndicesArray().Select(x => (ushort)x).ToArray();
            var vis = new VertexIndices(indices);

            var p = new Primitive(LoadAttributes(prim).ToArray(), vis); 
            var c = prim.Material?.FindChannel("BaseColor")?.Parameter;
            var mr = prim.Material?.FindChannel("MetallicRoughness")?.Parameter;

            var bc = prim.Material?.FindChannel("BaseColor");
            if(bc != null){
                var img = bc.Value.Texture.PrimaryImage.Content;
                if(img.IsPng){
                    var decode = Image.Load<Rgba32>(img.Content.Span);
                    var tex = GlUtil.CreateTexture2d(decode);
                    p.Material = new LightmapMaterial {
                        Texture = tex
                    };
                }    
            } else {
                p.Material = new PbrMaterial 
                {
                    BaseColor = c.HasValue ? new Vector3(c.Value.X, c.Value.Y, c.Value.Z) : new Vector3(1,0,1),
                    RoughnessFactor = mr.HasValue ? mr.Value.X : 0,
                    MetallicFactor = mr.HasValue ? mr.Value.Y : 0
                };
            }



            return p;
        }

        private static readonly Dictionary<string, StandardAttribute> mappings = new()
        {
            ["POSITION"] = StandardAttribute.Position,
            ["NORMAL"] = StandardAttribute.Normal,
            ["TANGENT"] = StandardAttribute.Tangent,
            ["TEXCOORD_0"] = StandardAttribute.Uv_0,
            ["TEXCOORD_1"] = StandardAttribute.Uv_1,
        };

        private static IEnumerable<VertexAttribute> LoadAttributes(SharpGLTF.Schema2.MeshPrimitive prim)
        {
            foreach (var kv in prim.VertexAccessors)
            {
                if (mappings.ContainsKey(kv.Key))
                {
                    var acc = kv.Value;
                    var type = mappings[kv.Key];
                    var dimension = 0;

                    switch (acc.Dimensions)
                    {
                        case SharpGLTF.Schema2.DimensionType.VEC2:
                            dimension = 2;
                            break;
                        case SharpGLTF.Schema2.DimensionType.VEC3:
                            dimension = 3;
                            break;
                        default:
                            break;
                    }
                    
                    if (dimension == 3 && acc.Encoding == SharpGLTF.Schema2.EncodingType.FLOAT)
                    {
                        var desc = new VertexAttributeDescriptor(dimension, GL.FLOAT, 0, 0, acc.Normalized);
                        yield return new VertexAttribute<Vector3>(type, acc.AsVector3Array().ToArray(), desc);
                    }
                    else if(dimension == 2 && acc.Encoding == SharpGLTF.Schema2.EncodingType.FLOAT)
                    {
                        var desc = new VertexAttributeDescriptor(dimension, GL.FLOAT, 0, 0, acc.Normalized);
                        yield return new VertexAttribute<Vector2>(type, acc.AsVector2Array().ToArray(), desc);
                    }
                }
            }
            yield break;
        }

    }
}