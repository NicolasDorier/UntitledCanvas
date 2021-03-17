using Avalonia;
using Avalonia.Controls;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UntitledCanvas.MySketch
{
	public class Sketch : SketchBase
	{
		public Sketch()
		{

		}
		public Grid grid = null;
		public Distribution<Vector> dist = new Distribution<Vector>(new[]
		{
			new Vector(-1.0, -1.0), new Vector(0.0, -1.0), new Vector(1.0, -1.0),
			new Vector(-1.0, 0.0),                        new Vector(1.0, 0.0),
			new Vector(-1.0, 1.0), new Vector(0.0, 1.0), new Vector(1.0, 1.0),
		},
		new[]
		{
			  3.0f, 2.0f, 1.0f,
			  1.0f,       1.0f,
			  1.0f, 1.0f , 2.0f,
		});
		public List<Particle> particules = new List<Particle>();
		public List<Particle> deadParticules = new List<Particle>();
		public List<Particle> newParticules = new List<Particle>();



		public override void Setup()
		{
			int w = 800;
			int h = 600;
			Size(w, h);
			Background(255);
			Stroke(61, 0, 0);

			grid = new Grid(w, h, 80 * 1, 60 * 1);
			//imgGrid = new Grid(w, h, 80 * 3, 60 * 3);
			//img = loadImage("Bitcoin_logo.png");
			//imgGrid.Mark(img);	
			particules.Add(new Particle(50, 50, grid, this));
		}
		public override void Draw()
		{
			foreach (Particle p in particules)
			{
				p.Move();
			}

			foreach (Particle p in deadParticules)
			{
				particules.Remove(p);
			}
			deadParticules.Clear();

			foreach (Particle p in newParticules)
			{
				particules.Add(p);
			}
			newParticules.Clear();
		}

		
	}

	




}
