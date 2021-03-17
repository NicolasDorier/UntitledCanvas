using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledCanvas.MySketch
{
	public class Grid
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
}
