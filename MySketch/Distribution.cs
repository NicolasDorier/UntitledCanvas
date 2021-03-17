using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledCanvas.MySketch
{
	public class Distribution<T>
	{
		Random rand = new Random();
		T[] items;
		float[] probabilities;
		float[] sum;
		float total;
		public Distribution(T[] items, float[] probabilities)
		{
			this.probabilities = probabilities;
			this.items = items;
			sum = new float[probabilities.Length];
			for (int i = 0; i < probabilities.Length; i++)
			{
				float p = probabilities[i];
				total += p;
				sum[i] = total;
			}
		}

		public T Pick()
		{
			float v = random(0, total);
			for (int i = 0; i < probabilities.Length; i++)
			{
				if (v < sum[i])
					return items[i];
			}
			return items[probabilities.Length - 1];
		}

		private float random(float min, float max)
		{
			return min + (float)(rand.NextDouble() * (max - min));
		}
	}
}
