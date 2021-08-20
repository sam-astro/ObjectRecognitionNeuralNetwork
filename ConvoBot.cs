using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

public class ConvoBot
{
	public NeuralNetwork net;

	public bool failed;

	byte[][] prompt;
	float answer;
	int amountCorrect = 0;

	public void StartNetwork()
	{
		Prompt promptObject = new Prompt();
		failed = false;
		for (int l = 0; l < promptObject.AmountOfPrompts(); l++)
		{
			answer = 0;

			prompt = promptObject.GetPrompt(l);

			float[] inputs = new float[784];
			for (int x = 0; x < prompt.Length; x++)
			{
				for (int y = 0; y < prompt[x].Length; y++)
				{
					if (y == 0)
						inputs[x] = prompt[x][y];
					else
						inputs[(28 * (y - 1)) + x] = prompt[x][y];
				}
			}
			float[] outputs = net.FeedForward(inputs);
			answer = outputs[0] * 3;
			//for (int i = 0; i < 10; i++)
			//{
			//	answer += Math.Abs(outputs[i]/10);
			//}

			//float guessScore = (float)(Math.Round((promptObject.CheckCorrectness(answer)) * 100) / 100);
			if (answer > 0.5f && answer < 1f && promptObject.PromptType() == "HUMAN")
			{
				amountCorrect++;
				//net.AddFitness(Math.Clamp(answer, 0, 1000) * 100);
				net.AddFitness(((float)100 / (float)promptObject.AmountOfPrompts()) * (float)100);
			}
			else if (answer < 0.5f && answer > 0f && promptObject.PromptType() == "NON-HUMAN")
			{
				amountCorrect++;
				//net.AddFitness(Math.Abs(Math.Clamp(1 - answer, -1000, 1)) * 100);
				net.AddFitness(((float)100 / (float)promptObject.AmountOfPrompts()) * (float)100);
			}
			//if (guessScore > 95)
			//{
			//Console.Write("Guessed::  " + answer + " : " + promptObject.correctAnswer + " : ");

			//if (answer >= 0.5f)
			//	Console.Write("Guessed::  HUMAN    Answer::  " + promptObject.PromptType() + " : ");
			//else
			//	Console.Write("Guessed::  NON-HUMAN    Answer::  " + promptObject.PromptType() + " : ");


			//if (guessScore < 94)
			//{
			//	Console.ForegroundColor = ConsoleColor.Red;
			//	Console.Write(guessScore + "%" + '\n');
			//	Console.ResetColor();
			//}
			//else if (guessScore < 97)
			//{
			//	Console.ForegroundColor = ConsoleColor.DarkYellow;
			//	Console.Write(guessScore + "%" + '\n');
			//	Console.ResetColor();
			//}
			//else
			//{
			//	Console.ForegroundColor = ConsoleColor.Green;
			//	Console.Write(guessScore + "%" + '\n');
			//	Console.ResetColor();
			//}
			//}
			//Console.WriteLine(score);

			//net.AddFitness(((float)amountCorrect / (float)promptObject.amountOfPrompts)*100);
		}
		//if(net.GetFitness() < ((float)amountCorrect) * 100)
		//	net.SetFitness((((float)amountCorrect / (float)promptObject.amountOfPrompts) * 100) + (int)(new Random().Next(-50, 50) / 1000.0f));
		//net.SetFitness(((float)amountCorrect / (float)promptObject.amountOfPrompts) * 100);
		//net.AddFitness(((float)amountCorrect) * 100);
		//if (amountCorrect > promptObject.AmountOfPrompts() - 25)
		Console.WriteLine("Got " + amountCorrect + " of " + promptObject.AmountOfPrompts() + " correct.");
		failed = true;
	}

	public void Init(NeuralNetwork net)
	{
		this.net = net;
		StartNetwork();
	}

}

