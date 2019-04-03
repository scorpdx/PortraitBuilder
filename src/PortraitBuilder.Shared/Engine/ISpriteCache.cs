using PortraitBuilder.Model.Portrait;
using System;

namespace PortraitBuilder.Engine
{
    public interface ISpriteCache : IDisposable
    {
        Sprite Get(SpriteDef def);
    }
}