using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media.Imaging;

namespace UntitledCanvas
{
	public class SketchBase
	{
		public const double TAU = Math.PI * 2.0;
		Random rand = new Random();
		public double Random(double min, double max)
		{
			return min + (rand.NextDouble() * (max - min));
		}
		public SKCanvas Canvas { get; set; }
 		
		public virtual void Setup()
		{

		}
		
		public virtual void Draw()
		{

		}
		
		public void Background(byte gray)
		{
			Background(gray, gray, gray);
		}

		SKPaint background = null;
		public void Background(byte r, byte g, byte b)
		{
			if (background is null)
				background = new SKPaint();
			background.Color = new SKColor(r, g, b);
			background = new SKPaint() { Color = new SKColor(r, g, b) };
			Canvas.DrawRect(size, background);
		}
		public void Rotate(float radians)
		{
			Canvas.RotateRadians(radians);
		}

		public void Line(float x)
		{
			Canvas.DrawLine(0f, 0f, x, 0f, stroke);
			Canvas.Translate(x, 0f);
		}
		public void Line(double x1, double y1, double x2, double y2)
		{
			Canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, stroke);

		}

		public void Translate(double x, double y)
		{
			Canvas.Translate((float)x, (float)y);
		}

		SKPaint stroke = new SKPaint();
		public void Stroke(byte gray)
		{
			Stroke(gray, gray, gray);
		}
		public void Stroke(byte r, byte g, byte b)
		{
			stroke.Color = new SKColor(r, g, b);
		}

		private RenderTargetBitmap _targetBitmap;
		internal SKRect size;
		public void Size(int width, int height)
		{
			if (size.Width == width && size.Height == height)
				return;
			
			
			// Dispatcher.UIThread.InvokeAsync(() =>
			// {
			// 	//Window.sketchDraw.Bounds = new Rect(0.0, 0.0, width, height);
			// 	Window.Width = width;
			// 	Window.Height = height;
			// }, DispatcherPriority.Normal);
			HasNewSize = true;
			
			size = new SKRect(0.0f, 0.0f, width, height);
		}

		internal volatile bool HasNewSize;
		
	}
}
