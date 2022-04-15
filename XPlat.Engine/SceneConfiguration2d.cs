using XPlat.Core;

namespace XPlat.Engine
{
    public class SceneConfiguration2d : SceneConfiguration {
        public SceneConfiguration2d(IPlatform platform)
        {
            var bs = new BehaviourSubsystem();
            AddInitSystem(bs);
            AddUpdateSystem(bs);
            AddUpdateSystem(new Collision2dSubsystem());
            AddRenderPass(new RenderPass2d(platform));
            AddRenderPass(new DebugRenderPass(platform));
        }
    }
}