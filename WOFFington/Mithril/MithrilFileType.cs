namespace WOFFington.Mithril
{
    /// <summary>
    ///     File Types found in Mithril compressed files
    /// </summary>
    public enum MithrilFileType
    {
        /// <summary>
        ///     ?
        /// </summary>
        Package = 0x00,

        /// <summary>
        ///     Effect data.
        /// </summary>
        Effect = 0x01,

        /// <summary>
        ///     Speculation: Video?
        /// </summary>
        VideoInfo = 0x02,

        /// <summary>
        ///     Speculation: Map data?
        /// </summary>
        Map = 0x04,

        /// <summary>
        ///     Speculation: Camera path, Animation curves.
        /// </summary>
        Curve = 0x06,

        /// <summary>
        ///     HLSL compiled Vertex Shader
        /// </summary>
        VertexShader = 0x07,

        /// <summary>
        ///     HLSL compiled Pixel Shader
        /// </summary>
        PixelShader = 0x09,

        /// <summary>
        ///     Speculation: Model material.
        /// </summary>
        ModelMaterial = 0x0C,

        /// <summary>
        ///     Hashed/Cell-based CSV binary file.
        /// </summary>
        Csh = 0x0D,

        /// <summary>
        ///     Geometry data.
        /// </summary>
        Model = 0x0E,

        /// <summary>
        ///     Speculation: Spk?
        /// </summary>
        ShaderPack = 0x14,

        /// <summary>
        ///     HUD Data
        /// </summary>
        HUD = 0x18,

        /// <summary>
        ///     Texture. Speculation: Win64 only?
        /// </summary>
        Texture = 0x23
    }
}
