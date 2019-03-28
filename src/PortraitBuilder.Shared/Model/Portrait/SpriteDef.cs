namespace PortraitBuilder.Model.Portrait
{
    //TODO: make immutable
    public sealed class SpriteDef
    {
        public string Name { get; set; }
        public string TextureFilePath { get; set; }
        public int FrameCount { get; set; }
        public bool NoRefCount { get; set; }
    }
}
