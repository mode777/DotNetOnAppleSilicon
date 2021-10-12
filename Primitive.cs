namespace net6test
{
    public class Primitive 
    {
        private readonly VertexAttribute[] attributes;
        private readonly VertexIndices indices;

        public Material? Material { get; set; }

        public Primitive(VertexAttribute[] attributes, VertexIndices indices)
        {
            this.attributes = attributes;
            this.indices = indices;
        }

        public void DrawWithShader(Shader shader){
            Material?.ApplyToShader(shader);
            foreach (var attr in attributes)
            {
                attr.EnableOnShader(shader);
            }
            indices.DrawWithShader(shader);
        }
    }
}