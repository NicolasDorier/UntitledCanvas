using Avalonia;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace UntitledCanvas
{
	public class Sketch : SketchBase
	{
		static Random rand = new Random();
		static float random(float min, float max)
		{
			return min + (float)(rand.NextDouble() * (max - min));
		}
		public Sketch()
		{

		}
		static Grid grid = null;
		static Distrib dist = new Distrib();
		static List<Particle> particules = new List<Particle>();
		static List<Particle> deadParticules = new List<Particle>();
		static List<Particle> newParticules = new List<Particle>();



		public override void Setup()
		{
			int w = 800;
			int h = 600;
			Size(w, h);
			Background(255);
			Stroke(255,0,0);
			
			grid = new Grid(w, h, 80 * 3, 60 * 3);
			//imgGrid = new Grid(w, h, 80 * 3, 60 * 3);
			//img = loadImage("Bitcoin_logo.png");
			//imgGrid.Mark(img);
			particules.Add(new Particle(50, 50, grid, this));
		}

		int x = 5;
		byte back = 255;
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

		class Particle
		{
			public Grid grid;
			int row;
			int column;
			Sketch sketch;
			public Particle(int row,
							int column,
							Grid grid,
							Sketch sketch)
			{
				this.grid = grid;
				this.row = row;
				this.column = column;
				this.sketch = sketch;
			}

			public void Move()
			{
				int tries = 0;
				int newRow, newCol;
				do
				{
					Vector direction = dist.Pick();
					tries++;
					if (tries > 100)
					{
						Vector pos = grid.ToPosition(row, column);
						sketch.Stroke(255, 0, 0);
						//ellipse(pos.x,pos.y, 2, 2);
						sketch.Stroke(0);
						deadParticules.Add(this);
						return;
					}
					newRow = (int)(row + direction.X);
					newCol = (int)(column + direction.Y);
				} while (grid.IsMarked(newRow, newCol));

				grid.Mark(newRow, newCol);

				int prevRow = row;
				int prevCol = column;
				row = newRow;
				column = newCol;


				Vector from = grid.ToPosition(prevRow, prevCol);
				Vector to = grid.ToPosition(row, column);

				//if (imgGrid.IsMarked(row, column))
				//	stroke(0xf7, 0x93, 0x1a);
				sketch.Line((float)from.X, (float)from.Y, (float)to.X, (float)to.Y);
				sketch.Stroke(0);
				if (random(0.0f, 100.0f) < 5.0)
				{
					newParticules.Add(new Particle(row, column, grid, sketch));
				}
			}
		}
	}

	class Grid
	{
		float wPerRows;
		float hPerColumns;
		int w;
		int h;
		int maxRow;
		int maxCol;
		HashSet<long> marked = new HashSet<long>();
		public Grid(int width, int height,
			  int rows, int columns)
		{
			wPerRows = width / rows;
			hPerColumns = height / columns;
			maxRow = (int)(width / wPerRows);
			maxCol = (int)(height / hPerColumns);
			w = width;
			h = height;
		}
		public Vector ToPosition(int row, int column)
		{
			return new Vector(wPerRows * row, hPerColumns * column);
		}

		public void Clear()
		{
			marked.Clear();
		}
		public void Mark(int row, int column)
		{
			long l = (long)row | ((long)column << (8 * 4));
			marked.Add(l);
		}

		public bool IsMarked(int row, int column)
		{
			if (row < 0 || row > maxRow)
				return true;
			if (column < 0 || column > maxCol)
				return true;
			long l = (long)row | ((long)column << (8 * 4));
			return marked.Contains(l);
		}
	}

	

	class Distrib
	{
		Random rand = new Random();
		Vector[] directions =
		{
	new Vector(-1.0, -1.0), new Vector(0.0, -1.0), new Vector(1.0, -1.0),
	new Vector(-1.0, 0.0),                        new Vector(1.0, 0.0),
	new Vector(-1.0, 1.0), new Vector(0.0, 1.0), new Vector(1.0, 1.0),
  };
		float[] probabilities =
		{
	  3.0f, 2.0f, 1.0f,
	  1.0f,       1.0f,
	  1.0f, 1.0f , 2.0f,
  };
		float[] sum;
		float total;
		public Distrib()
		{
			sum = new float[probabilities.Length];
			for (int i = 0; i < probabilities.Length; i++)
			{
				float p = probabilities[i];
				total += p;
				sum[i] = total;
			}
		}

		public Vector Pick()
		{
			float v = random(0, total);
			for (int i = 0; i < probabilities.Length; i++)
			{
				if (v < sum[i])
					return directions[i];
			}
			return directions[probabilities.Length - 1];
		}

		private float random(float min, float max)
		{
			return min + (float)(rand.NextDouble() * (max - min));
		}
	}
}
