using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Engine
{
    public interface ISpriteCache
    {
        Sprite Get(SpriteDef def);
    }
}