using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using PortraitBuilder.Engine;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortraitBuilder.Online
{
    public class BlobSprite : ISprite
    {
        public BlobSprite()
        {

        }

        public SKBitmap this[int index] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SKBitmap> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
