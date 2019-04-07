using System;
using System.Collections.Generic;
using SkiaSharp;

namespace PortraitBuilder.Engine
{
    public interface ISprite : IReadOnlyList<SKBitmap>, IDisposable
    {
    }
}