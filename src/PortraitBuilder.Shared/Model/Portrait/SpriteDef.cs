namespace PortraitBuilder.Model.Portrait
{
    //TODO: make immutable
    public sealed class SpriteDef
    {
        public string Name { get; set; }
        public string TextureFilePath { get; set; }
        public int FrameCount { get; set; }
        public bool NoRefCount { get; set; }

        public override string ToString()
            => $"Name: {Name}, Texture: {TextureFilePath} ({FrameCount} frames), NoRefCount: {NoRefCount}";
    }
}
