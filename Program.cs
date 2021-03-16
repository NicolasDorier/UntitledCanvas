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
                var code = File.ReadAllText(e.FullPath);
                code = code.Replace("class Sketch", "class Sketch" + new Random().Next(0, int.MaxValue));
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
                    sketch.Window = Window;
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

        public override void Render(DrawingContext context)
        {
            context.Custom(sketchDraw);
            base.Render(context);
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
}
