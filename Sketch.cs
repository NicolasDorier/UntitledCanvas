using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledCanvas
{
	public class Sketch : SketchBase
	{
		public override void Setup()
		{
			Size(800, 600);
			//Background(200);
			//Stroke(255); 
		}

		byte gray = 125;
		public override void Draw()
		{
			gray++;
			Background(gray);
		}
	}
}
