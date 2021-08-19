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
				pixels[y][x] = (byte)avgBrightness;
			}
		}
		//resized.Save(TrainingFiles[l] + "out.jpg");

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
