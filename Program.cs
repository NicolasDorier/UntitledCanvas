using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Skia.Helpers;
using Avalonia.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Utilities;
using Avalonia.Visuals.Media.Imaging;
using System.Net.Http.Headers;

namespace UntitledCanvas
{
    class Program
    {
        static void Main(string[] args)
        {
            AppBuilder.Configure<UntitledCanvasApp>()
                .UsePlatformDetect()
                .StartWithClassicDesktopLifetime(args);
        }
    }

    class UntitledCanvasApp : Application
    {
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new UntitledCanvasWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
    class UntitledCanvasWindow : Avalonia.Controls.Window
    {
        public UntitledCanvasWindow()
        {
            sketchDraw = new SketchDrawOperation(this);
        }
        protected override void OnInitialized()
        {
            watcher = new FileSystemWatcher("../../..");
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Changed;
            sketchDraw.SetSketch(new Sketch());
            base.OnInitialized();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name == "Sketch.cs")
            {
                var code =  trycatch(() => File.ReadAllText(e.FullPath));
                //code = code.Replace("class Sketch", "class Sketch" + new Random().Next(0, int.MaxValue));
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetRandomFileName(),
                new[] { syntaxTree },
                GetReferenceAssemblies(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );
                using (var ms = new MemoryStream())
                {
                    var compilationResult = compilation.Emit(ms);
                    if (compilationResult.Success)
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        var assembly = Assembly.Load(ms.ToArray());
                        var sketchType = assembly.GetTypes().Where(t => t.BaseType == typeof(SketchBase)).First();
                        var sketch = (SketchBase)Activator.CreateInstance(sketchType);

                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.sketchDraw.SetSketch(sketch);
                        }, DispatcherPriority.Background);
                    }
                }
            }
        }

        private string trycatch(Func<string> p)
        {
            retry:
            try
            {
                return p();
            }
            catch (IOException)
            {
                goto retry;
            }
        }

        private IEnumerable<MetadataReference> GetReferenceAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.IsNullOrEmpty(assembly.Location))
                    continue;
                yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }

        internal class SketchDrawOperation : ICustomDrawOperation
        {
            public SketchDrawOperation(UntitledCanvasWindow window)
            {
                Window = window;
                Bounds = new Rect(0.0, 0.0, 1, 1);
            }
            public SketchBase Sketch { get; private set; }
            bool isSetup;
            public UntitledCanvasWindow Window { get; }

            public Rect Bounds { get; set; }
            public void Dispose()
            {
                
            }

            public bool Equals([AllowNull] ICustomDrawOperation other)
            {
                return this == other;
            }

            public bool HitTest(Point p)
            {
                return false;
            }

            public void Render(IDrawingContextImpl context)
            {
                if (!(Sketch is SketchBase sketch))
                    return;
                var skiaContext = (ISkiaDrawingContextImpl)context;
                var canvas = skiaContext.SkCanvas;
                sketch.Canvas = canvas;
                canvas.Save();
                if (!isSetup)
                {
                    sketch.Setup();
                    isSetup = true;
                }
                sketch.Draw();
                canvas.Restore();
            }

            public void SetSketch(SketchBase sketch)
            {
                Sketch = sketch;
                isSetup = false;
            }
        }
        internal SketchDrawOperation sketchDraw;
        private FileSystemWatcher watcher;
        private RenderTargetBitmap currentRTB;
        public override void Render(DrawingContext context)
        {
            if (sketchDraw.Sketch.HasNewSize || currentRTB is null)
            {
                if (sketchDraw.Sketch.size.Width == 0 ||
                    sketchDraw.Sketch.size.Height == 0)
                {
                    sketchDraw.Sketch.size = new SKRect(0, 0, 1, 1);
                }
                
                var newRTB =
                    new RenderTargetBitmap(new PixelSize((int)sketchDraw.Sketch.size.Width, (int)sketchDraw.Sketch.size.Height));

                if (!(currentRTB is null))
                { 
                    using (var currentRTBContext = currentRTB.CreateDrawingContext(null))
                    using (var newRTBContext = newRTB.CreateDrawingContext(null))
                    {
                        var src = new Rect(new Point(0, 0), currentRTB.Size);
                        var dst = new Rect(new Point(0, 0), newRTB.Size);
                        
                        newRTBContext.DrawBitmap(currentRTB.PlatformImpl, 1, src, dst, BitmapInterpolationMode.Default );
                        
                    }
                    currentRTB?.Dispose();
                }
                
                currentRTB = newRTB;
                sketchDraw.Sketch.HasNewSize = false;
                
                Dispatcher.UIThread.Post(() =>
                {
                    Width = sketchDraw.Sketch.size.Width;
                    Height = sketchDraw.Sketch.size.Height;
                });
            }

            using (var currentRTBContext = currentRTB.CreateDrawingContext(null))
            {
                currentRTBContext.Custom(sketchDraw);
            }
            
            var srcdst = new Rect(new Point(0, 0), currentRTB.Size);
                        
            context.PlatformImpl.DrawBitmap(currentRTB.PlatformImpl, 1, srcdst, srcdst, BitmapInterpolationMode.Default );

            base.Render(context);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
