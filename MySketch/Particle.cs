using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledCanvas.MySketch
{
	public class Particle
	{
		Grid grid;
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
				Vector direction = sketch.dist.Pick();
				tries++;
				if (tries > 100)
				{
					Vector pos = grid.ToPosition(row, column);
					sketch.Stroke(255, 0, 0);
					//ellipse(pos.x,pos.y, 2, 2);
					sketch.Stroke(0);
					sketch.deadParticules.Add(this);
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
			if (sketch.Random(0.0, 100.0) < 5.0)
			{
				sketch.newParticules.Add(new Particle(row, column, grid, sketch));
			}
		}
	}
}
