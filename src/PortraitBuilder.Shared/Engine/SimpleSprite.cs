using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PortraitBuilder.Engine
{
    public class SimpleSprite : ISprite
    {
        private IReadOnlyList<SKBitmap> _bitmaps;
        public SimpleSprite(IReadOnlyList<SKBitmap> bitmaps)
        {
            if (bitmaps == null)
                throw new ArgumentNullException(nameof(bitmaps));

            _bitmaps = bitmaps;
        }

        public SKBitmap this[int index] => _bitmaps[index];

        public int Count => _bitmaps.Count;

        public IEnumerator<SKBitmap> GetEnumerator() => _bitmaps.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach(var bitmap in _bitmaps)
                    {
                        bitmap?.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
