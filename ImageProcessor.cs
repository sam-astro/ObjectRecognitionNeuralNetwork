using System;
using System.IO;
using System.Drawing;

namespace ProcessImageCSharp
{
	class Program
	{
		public static byte[][] pixels = new byte[28][];

		//static void Main(string[] args)
		//{
		//	string[] TrainingFiles = Directory.GetFiles(".\\humansdataset\\");

		//	foreach (var imagePath in TrainingFiles)
		//	{
		//		if (!imagePath.Contains("out"))
		//		{
		//			for (int i = 0; i < pixels.Length; ++i)
		//				pixels[i] = new byte[28];

		//			Bitmap original = (Bitmap)Image.FromFile(imagePath);
		//			Bitmap resized = new Bitmap(original, new Size(28, 28));

		//			for (int x = 0; x < resized.Width; x++)
		//			{
		//				for (int y = 0; y < resized.Height; y++)
		//				{
		//					Color pixelColor = resized.GetPixel(x, y);
		//					int avgBrightness = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
		//					resized.SetPixel(x, y, Color.FromArgb(avgBrightness, avgBrightness, avgBrightness));
		//					pixels[y][x] = (byte)avgBrightness;
		//				}
		//			}
		//			Console.Write(ImgToString());
		//			resized.Save(imagePath + "out.jpg");

		//			//var bitmap = new Bitmap(@"dat\in.jpg");
		//			//bitmap.SetResolution(32, 32);
		//			////bitmap.
		//			//bitmap.Save(@"dat\out.jpg");
		//		}
		//	}
		//}

        public static string ImgToString()
        {
            string s = "";
            for (int i = 0; i < 28; ++i)
            {
                for (int j = 0; j < 28; ++j)
                {
                    if (pixels[i][j] >= 245)
                        s += "##";
                    else if (pixels[i][j] >= 230)
                        s += "BB";
                    else if (pixels[i][j] >= 225)
                        s += "XX";
                    else if (pixels[i][j] >= 220)
                        s += "OO";
                    else if (pixels[i][j] >= 210)
                        s += "II";
					else if (pixels[i][j] >= 180)
						s += "++";
					else if (pixels[i][j] >= 150)
						s += "::";
					else if (pixels[i][j] >= 100)
						s += "..";
					else
						s += "  ";
				}
                s += "\n";
            }
			
            return s;
        } // ToString
    }
}
