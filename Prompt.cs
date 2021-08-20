using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Drawing;

public class Prompt
{
	public byte[][] prompt;
	public float correctAnswer;
	public int amountOfPrompts;

	public bool CheckPrompt(float answer)
	{
		//ReadList();
		if (answer == correctAnswer)
		{
			return true;
		}
		return false;
	}


	public static byte[][] pixels = new byte[28][];
	public byte[][] GetPrompt(int l)
	{
		string[] TrainingFiles = Directory.GetFiles(".\\humansdataset\\");
		
		for (int i = 0; i < pixels.Length; ++i)
			pixels[i] = new byte[28];

		Bitmap original = (Bitmap)Image.FromFile(TrainingFiles[l]);
		Bitmap resized = new Bitmap(original, new Size(28, 28));

		for (int x = 0; x < resized.Width; x++)
		{
			for (int y = 0; y < resized.Height; y++)
			{
				Color pixelColor = resized.GetPixel(x, y);
				int avgBrightness = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
				resized.SetPixel(x, y, Color.FromArgb(avgBrightness, avgBrightness, avgBrightness));
				//pixels[y][x] = (byte)avgBrightness;
			}
		}
		Bitmap edgeDetected = EdgeDetection(resized);
		for (int x = 0; x < resized.Width; x++)
		{
			for (int y = 0; y < resized.Height; y++)
			{
				pixels[y][x] = (byte)edgeDetected.GetPixel(x, y).R;
			}
		}

		prompt = pixels;
		if (TrainingFiles[l].Split("humansdataset")[1].Contains("human") == true)
		{
			correctAnswer = 1;
			//Console.WriteLine(TrainingFiles[l]);
		}
		else
		{
			correctAnswer = 0;
			//Console.WriteLine("FOUND NON-HUMAN!!!!!");
		}

		return prompt;
	}

	public static Bitmap EdgeDetection(Bitmap inputImg)
	{
		Bitmap result = inputImg;
		int newIntensity;

		for (var i = 0; i < inputImg.Height; i++)
		{
			for (var j = 0; j < inputImg.Width; j++)
			{
				if (i == inputImg.Height - 1 || j == inputImg.Width - 1)
					newIntensity = 0;
				else
				{
					var Gx = (int)Math.Pow(inputImg.GetPixel(i, j).R - inputImg.GetPixel(i + 1, j + 1).R, 2);
					var Gy = (int)Math.Pow(inputImg.GetPixel(i, j + 1).R - inputImg.GetPixel(i + 1, j).R, 2);

					newIntensity = (int)Math.Clamp(Math.Sqrt(Gx + Gy) * 2, 0, 255);

					if (newIntensity < 100)
						newIntensity = 0;
				}

				result.SetPixel(i, j, Color.FromArgb(newIntensity, newIntensity, newIntensity));
			}
		}

		return result;
	}

	//void ReadList()
	//{
	//	StreamReader sr = File.OpenText(".\\dat\\promptlist.dat");
	//	string[] fullFile = sr.ReadToEnd().Split('\n');

	//	int randomPromptNumber = new Random().Next(0, fullFile.Length);

	//	prompt = fullFile[randomPromptNumber].Split(" # ")[0];
	//	correctAnswer = float.Parse(fullFile[randomPromptNumber].Split(" # ")[1]);
	//}

	public string PromptType()
	{
		if (correctAnswer == 1)
			return "HUMAN";
		else
			return "NON-HUMAN";
	}

	public int AmountOfPrompts()
	{
		amountOfPrompts = Directory.GetFiles(".\\humansdataset\\").Length;
		return amountOfPrompts;
	}

	public float CheckCorrectness(float guess)
	{
		//return (Math.Abs(correctAnswer - s) / correctAnswer) * 100;
		if (guess > 0.5 && correctAnswer == 1)
			return 100;
		else if (guess < 0.5 && correctAnswer == 0.01f)
			return 100;
		else
			return 0;
	}
}
