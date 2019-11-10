using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortraitBuilder.Online
{
    internal static class BlobLoader
    {
        internal static readonly Lazy<CloudBlobClient> _storageClient = new Lazy<CloudBlobClient>(() =>
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("PortraitPackStorage");
            Debug.Assert(!string.IsNullOrEmpty(storageConnectionString));

            var storage = CloudStorageAccount.Parse(storageConnectionString);
            return storage.CreateCloudBlobClient();
        });

        internal static readonly Lazy<ValueTask<PortraitData>> _portraitPack = new Lazy<ValueTask<PortraitData>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("portraits.json");
            var json = await blob.DownloadTextAsync();

            var options = ContentPacks.JsonHelper.GetDefaultOptions();
            return JsonSerializer.Deserialize<PortraitData>(json, options);
        });

        internal static readonly Lazy<ValueTask<IReadOnlyDictionary<string, IEnumerable<string>>>> _cultureLookup = new Lazy<ValueTask<IReadOnlyDictionary<string, IEnumerable<string>>>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("culture_lookup.json");
            var json = await blob.DownloadTextAsync();

            var options = ContentPacks.JsonHelper.GetDefaultOptions();
            return JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(json, options);
        });

        internal static readonly ConcurrentDictionary<string, Task<SKBitmap>> _blobTileCache = new ConcurrentDictionary<string, Task<SKBitmap>>();

    }
}
