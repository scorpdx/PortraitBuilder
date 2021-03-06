﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        internal static readonly Lazy<Task<PortraitData>> _portraitPack = new Lazy<Task<PortraitData>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("portraits.json");
            var json = await blob.DownloadTextAsync();

            var options = ContentPacks.JsonHelper.GetDefaultOptions();
            return JsonSerializer.Deserialize<PortraitData>(json, options);
        });

        internal static readonly Lazy<Task<IReadOnlyDictionary<string, IEnumerable<string>>>> _cultureLookup = new Lazy<Task<IReadOnlyDictionary<string, IEnumerable<string>>>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("culture_lookup.json");
            var json = await blob.DownloadTextAsync();

            var options = ContentPacks.JsonHelper.GetDefaultOptions();
            return JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(json, options);
        });

        internal static readonly Lazy<Task<IReadOnlyDictionary<string, ValueTuple<int, int>>>> _religiousClothingLookup = new Lazy<Task<IReadOnlyDictionary<string, ValueTuple<int, int>>>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("religious_clothing_lookup.json");
            var json = await blob.DownloadTextAsync();

            using var jd = JsonDocument.Parse(json);
            return jd.RootElement
                .EnumerateObject()
                .ToDictionary(
                    jp => jp.Name,
                    jp => (
                        jp.Value.GetProperty("religious_clothing_head").GetInt32(),
                        jp.Value.GetProperty("religious_clothing_priest").GetInt32()
                    )
                );
        });

        internal static readonly ConcurrentDictionary<string, Task<SKBitmap>> _blobTileCache = new ConcurrentDictionary<string, Task<SKBitmap>>();

    }
}
