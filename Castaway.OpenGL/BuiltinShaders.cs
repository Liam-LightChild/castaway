using System.IO;
using System.Reflection;
using Castaway.Base;
using Castaway.Rendering;
using Serilog;

namespace Castaway.OpenGL
{
    [ProvidesShadersFor(typeof(OpenGLImpl)),
     ProvidesShadersFor(typeof(OpenGL33)),
     ProvidesShadersFor(typeof(OpenGL40)),
     ProvidesShadersFor(typeof(OpenGL41)),
     ProvidesShadersFor(typeof(OpenGL42))]
    internal class BuiltinShaders : IShaderProvider
    {
        private static readonly ILogger Logger = CastawayGlobal.GetLogger();

        public virtual ShaderObject CreateDefault(Graphics g) => ReadShader("default.normal.shdr");
        public virtual ShaderObject CreateDefaultTextured(Graphics g) => ReadShader("default.textured.shdr");
        public virtual ShaderObject CreateDirect(Graphics g) => ReadShader("direct.normal.shdr");
        public virtual ShaderObject CreateDirectTextured(Graphics g) => ReadShader("direct.textured.shdr");
        public virtual ShaderObject CreateUIScaled(Graphics g) => ReadShader("ui.scaled.normal.shdr");
        public virtual ShaderObject CreateUIScaledTextured(Graphics g) => ReadShader("ui.scaled.textured.shdr");
        public virtual ShaderObject CreateUIUnscaled(Graphics g) => ReadShader("ui.unscaled.normal.shdr");
        public virtual ShaderObject CreateUIUnscaledTextured(Graphics g) => ReadShader("ui.unscaled.textured.shdr");

        protected static ShaderObject ReadShader(string path)
        {
            Logger.Verbose("Searching manifest for {Path}", path);
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream($"Castaway.OpenGL._shaders.{path}");
            var reader = new StreamReader(stream!);
            return ShaderAssetType.LoadOpenGL(reader.ReadToEnd(),
                $"manifest:Castaway.OpenGL:Castaway.OpenGL._shaders.{path}");
        }
    }
}