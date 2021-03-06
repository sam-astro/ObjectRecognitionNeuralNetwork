using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ConvoBot
{
	public NeuralNetwork net;

	public bool failed;

	byte[][] prompt;
	float answer;
	int amountCorrect = 0;
	float confidence = 0;

	public void StartNetwork()
	{
		failed = false;
		prompt = null;
		answer = 0;
		amountCorrect = 0;
		confidence = 0;
		net.SetFitness(0);

		Prompt promptObject = new Prompt();
		for (int l = 0; l < promptObject.AmountOfPrompts(); l++)
		{
			answer = 0;

			prompt = promptObject.GetPrompt(l);

			float[] inputs = new float[1024];

			Parallel.For(0, prompt.Length, x =>
			{
				Parallel.For(0, prompt[x].Length, y =>
				{
					if (y == 0)
						inputs[x] = prompt[x][y]/255.0f;
					else
						inputs[(32 * (y - 1)) + x] = prompt[x][y] / 255.0f;
				});
			});
			//for (int x = 0; x < prompt.Length; x++)
			//{
			//	for (int y = 0; y < prompt[x].Length; y++)
			//	{
			//		if (y == 0)
			//			inputs[x] = prompt[x][y];
			//		else
			//			inputs[(64 * (y - 1)) + x] = prompt[x][y];
			//	}
			//}
			float[] outputs = net.FeedForward(inputs);
			answer = Math.Abs(outputs[0]);
			//for (int i = 0; i < 10; i++)
			//{
			//	answer += Math.Abs(outputs[i]/10);
			//}

			//float guessScore = (float)(Math.Round((promptObject.CheckCorrectness(answer)) * 100) / 100);
			if (answer > 0.5f && answer < 1f && promptObject.PromptType() == "HUMAN")
			{
				amountCorrect++;
				//net.AddFitness(Math.Clamp(answer, 0, 1000) * 100);
				net.AddFitness(((float)answer*100 / (float)promptObject.AmountOfPrompts()) * (float)100);
				confidence += answer;
			}
			else if (answer < 0.5f && answer > 0f && promptObject.PromptType() == "NON-HUMAN")
			{
				amountCorrect++;
				//net.AddFitness(Math.Abs(Math.Clamp(1 - answer, -1000, 1)) * 100);
				net.AddFitness(((float)(1f- answer)*100 / (float)promptObject.AmountOfPrompts()) * (float)100);
				confidence += (1 - answer);
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
		net.AddFitness(confidence / 10);
		Console.WriteLine("Got " + amountCorrect + " of " + promptObject.AmountOfPrompts() + " correct, " + Math.Round(confidence * 10) / 10 + "% confidence.");
		promptObject = null;
		failed = true;
	}

	public void Init(NeuralNetwork net)
	{
		this.net = net;
		StartNetwork();
	}

}

