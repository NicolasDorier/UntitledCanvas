using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledCanvas
{
	public class SketchBase
	{
		internal UntitledCanvasWindow Window { get; set; }
		public SKCanvas Canvas { get; set; }
		public virtual void Setup()
		{

		}
		
		public virtual void Draw()
		{

		}
		
		public void Background(byte gray)
		{
			var color = new SKColor(gray, gray, gray);
			if (background != null)
			{
				if (background.Color == color)
				{
					Canvas.DrawRect(size, background);
					return;
				}
				background.Dispose();
			}
			background = new SKPaint() { Color = color };
			Canvas.DrawRect(size, background);
		}
		SKPaint background;
		SKPaint stroke;
		public void Stroke(byte gray)
		{
			if (stroke != null)
			{
				stroke.Dispose();
			}
			stroke = new SKPaint() { Color = new SKColor(gray, gray, gray) };
		}

		SKRect size;
		public void Size(int width, int height)
		{
			if (size.Width == width && size.Height == height)
				return;
			Dispatcher.UIThread.InvokeAsync(() =>
			{
				//Window.sketchDraw.Bounds = new Rect(0.0, 0.0, width, height);
				Window.Width = width;
				Window.Height = height;
			}, DispatcherPriority.Normal);
			size = new SKRect(0.0f, 0.0f, width, height);
		}
	}
}
