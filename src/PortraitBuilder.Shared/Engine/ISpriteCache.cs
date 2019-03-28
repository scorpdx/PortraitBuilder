using System.Collections.Generic;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;

namespace PortraitBuilder.Engine
{
    public interface ISpriteCache
    {
        IReadOnlyList<Content> ActiveContent { get; }

        Sprite Get(SpriteDef def);
    }
}