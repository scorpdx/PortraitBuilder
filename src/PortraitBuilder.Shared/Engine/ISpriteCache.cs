using PortraitBuilder.Model.Portrait;
using System;

namespace PortraitBuilder.Engine
{
    public interface ISpriteCache : IDisposable
    {
        ISprite Get(SpriteDef def);
    }
}