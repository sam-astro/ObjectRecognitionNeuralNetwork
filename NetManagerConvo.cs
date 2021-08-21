using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public class NetManagerConvo
{
	private bool networksRunning = false;
	public int populationSize = 100;
	private int generationNumber = 1;
	public int[] layers = new int[] { 4096, 120, 120, 120, 60, 1 }; // No. of inputs and No. of outputs
	private List<NeuralNetwork> nets;
	public List<ConvoBot> entityList = null;
	bool startup = true;

	public int amntLeft;

	private float[][][] collectedWeights;
	private float[][][] collectedWeightsCopy;

	NeuralNetwork persistenceNetwork;

	float lastBest = 100000000;
	float lastWorst = 100000000;
	float highestFitness = 0;
	float lowestFitness = 100000;

	bool queuedForUpload = false;

	public void NeuralManager()
	{
		Console.WriteLine("Before this starts, how powerful is your PC?");
		Console.WriteLine("1. Minimum, please go easy :(");
		Console.WriteLine("2. Fair, it can't do much");
		Console.WriteLine("3. Average, it can browse the web and run some games");
		Console.WriteLine("4. Modest, it's good for some things but still a little slow");
		Console.WriteLine("5. Good, everything can run on it but only at medium settings");
		Console.WriteLine("6. Great, everything runs on it at high settings");
		Console.WriteLine("7. Excellent, everything runs on it at extreme settings");
		Console.WriteLine("8. Monstrous, it is basically a supercomputer.\n");

		while (true)
		{
			Console.Write("Answer (1-8) > ");
			string answer = Console.ReadLine();
			if (answer.Contains("1"))
			{
				populationSize = 20;
				break;
			}
			else if (answer.Contains("2"))
			{
				populationSize = 40;
				break;
			}
			else if (answer.Contains("3"))
			{
				populationSize = 80;
				break;
			}
			else if (answer.Contains("4"))
			{
				populationSize = 200;
				break;
			}
			else if (answer.Contains("5"))
			{
				populationSize = 300;
				break;
			}
			else if (answer.Contains("6"))
			{
				populationSize = 400;
				break;
			}
			else if (answer.Contains("7"))
			{
				populationSize = 1000;
				break;
			}
			else if (answer.Contains("8"))
			{
				populationSize = 10000;
				break;
			}
			else
			{
				Console.WriteLine();
				continue;
			}
		}

		StreamReader sr = File.OpenText(".\\dat\\WeightSaveMeta.meta");
		string firstLine = sr.ReadLine().Trim();
		string currentGen = firstLine.Split("#")[0];
		generationNumber = int.Parse(currentGen) + 1;
		float bestLocalFitness = float.Parse(firstLine.Split("#")[1]);
		sr.Close();

		try
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("* Getting highest fitness from  http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
			System.Net.WebClient Client = new System.Net.WebClient();
			string s = Client.DownloadString("http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
			Console.WriteLine("* Got highest fitness, it is: " + s);
			Console.ResetColor();
			if (s != null)
			{
				if (float.Parse(s) > bestLocalFitness)
					Download(s);
				else if (float.Parse(s) < bestLocalFitness)
					Upload(bestLocalFitness);
			}
		}
		catch (Exception)
		{
			if (File.Exists(".\\dat\\temp_WeightSave.dat"))
			{
				if (File.Exists(".\\dat\\WeightSave.dat"))
					File.Delete(".\\dat\\WeightSave.dat");
				File.Move(".\\dat\\temp_WeightSave.dat", ".\\dat\\WeightSave.dat");
			}
			if (File.Exists(".\\dat\\temp_WeightSaveMeta.meta"))
			{
				if (File.Exists(".\\dat\\WeightSaveMeta.meta"))
					File.Delete(".\\dat\\WeightSaveMeta.meta");
				File.Move(".\\dat\\temp_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
			}
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("* ? Could not sync with server. Please try again later. ?");
			Console.ResetColor();
			//throw;
		}

		InitEntityNeuralNetworks();
		while (true)
		{
			amntLeft = populationSize;
			CreateEntityBodies();

			while (true)
			{
				#region Check if nets are training
				amntLeft = populationSize;
				foreach (ConvoBot emt in entityList)
				{
					if (emt.failed)
					{
						amntLeft--;
					}
				}
				if (amntLeft <= 0)
				{
					networksRunning = false;
					amntLeft = populationSize;
					break;
				}
				#endregion
			}

			nets.Sort();

			highestFitness = nets[nets.Count-1].fitness;
			lowestFitness = nets[0].fitness;

			if ((highestFitness / 100) > lastBest || (queuedForUpload == true && generationNumber % 2 == 0))
			{
				StreamWriter persistence = new StreamWriter(".\\dat\\WeightSaveMeta.meta");
				persistence.WriteLine((generationNumber).ToString() + "#" + (highestFitness / 100).ToString());
				
				BinaryFormatter bf = new BinaryFormatter();
				using (FileStream fs = new FileStream(".\\dat\\WeightSave.dat", FileMode.Create))
					bf.Serialize(fs, nets[nets.Count - 1].weights);

				persistence.Close();

				Console.Write("╚═ Generation: " + generationNumber + "  |  Population: " + populationSize);
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write("Best Fitness: " + (highestFitness / 100) + "%");
				if ((lowestFitness / 100) < lastWorst)
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}
				else if ((lowestFitness / 100) > lastWorst)
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}
				else
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}

				Console.WriteLine("* Saving...");
				Console.ResetColor();

				try
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("* Getting highest fitness from  http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
					System.Net.WebClient Client = new System.Net.WebClient();
					string s = Client.DownloadString("http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
					Console.WriteLine("* Got highest fitness, it is: " + s);
					Console.ResetColor();
					if (s != null)
					{
						if (float.Parse(s) > bestLocalFitness)
							Download(s);
						else if (float.Parse(s) < bestLocalFitness)
							Upload(bestLocalFitness);
					}
				}
				catch (Exception)
				{
					if (File.Exists(".\\dat\\temp_WeightSave.dat"))
					{
						if (File.Exists(".\\dat\\WeightSave.dat"))
							File.Delete(".\\dat\\WeightSave.dat");
						File.Move(".\\dat\\temp_WeightSave.dat", ".\\dat\\WeightSave.dat");
					}
					if (File.Exists(".\\dat\\temp_WeightSaveMeta.meta"))
					{
						if (File.Exists(".\\dat\\WeightSaveMeta.meta"))
							File.Delete(".\\dat\\WeightSaveMeta.meta");
						File.Move(".\\dat\\temp_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
					}
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("* ? Could not sync with server. Please try again later. ?");
					Console.ResetColor();
					//throw;
				}
			}
			else
			{
				Console.Write("╚═ Generation: " + generationNumber + "  |  Population: " + populationSize);
				Console.Write("  |  ");
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.Write("Best Fitness: " + (highestFitness / 100) + "%");
				if ((lowestFitness / 100) < lastWorst)
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}
				else if ((lowestFitness / 100) > lastWorst)
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}
				else
				{
					Console.Write("  |  ");
					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write("Worst Fitness: " + (lowestFitness / 100) + "%\n");
					Console.ResetColor();
				}
			}

			Finalizer();
			//new finalizer().FinalizeGeneration(nets, populationSize);
			//nets.Sort();
			//for (int i = 2; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
			//{
			//    nets[i] = new NeuralNetwork(nets[populationSize - 2]);
			//    nets[i].Mutate();                                                    //Mutates new entities

			//    nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			//    nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			//    nets[populationSize - 2].Mutate();
			//}

			//for (int i = 0; i < populationSize; i++)
			//{
			//    nets[i].SetFitness(0f);
			//}

			if (generationNumber % 5 == 0)
			{
				try
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("* Getting highest fitness from  http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
					System.Net.WebClient Client = new System.Net.WebClient();
					string s = Client.DownloadString("http://achillium.us.to/objectrecognitionneuralnetdata/bestuploadedfitness.php");
					Console.WriteLine("* Got highest fitness, it is: " + s);
					Console.ResetColor();
					if (s != null)
					{
						if (float.Parse(s) > highestFitness / 100)
							Download(s);
						else if (float.Parse(s) < highestFitness / 100)
							Upload(highestFitness/100);
					}
				}
				catch (Exception)
				{
					if (File.Exists(".\\dat\\temp_WeightSave.dat"))
					{
						if (File.Exists(".\\dat\\WeightSave.dat"))
							File.Delete(".\\dat\\WeightSave.dat");
						File.Move(".\\dat\\temp_WeightSave.dat", ".\\dat\\WeightSave.dat");
					}
					if (File.Exists(".\\dat\\temp_WeightSaveMeta.meta"))
					{
						if (File.Exists(".\\dat\\WeightSaveMeta.meta"))
							File.Delete(".\\dat\\WeightSaveMeta.meta");
						File.Move(".\\dat\\temp_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
					}
					if (File.Exists(".\\dat\\" + highestFitness / 100 + "_WeightSave.dat"))
					{
						File.Move(".\\dat\\" + highestFitness / 100 + "_WeightSave.dat", ".\\dat\\WeightSave.dat");
					}
					if (File.Exists(".\\dat\\" + highestFitness / 100 + "temp_WeightSaveMeta.meta"))
					{
						File.Move(".\\dat\\" + highestFitness / 100 + "temp_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
					}
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("* ? Could not sync with server. Please try again later. ?");
					Console.ResetColor();
				}
			}

			amntLeft = populationSize;
			networksRunning = true;
			lastBest = (highestFitness / 100);
			lastWorst = (lowestFitness / 100);
			generationNumber++;
		}
	}

	private void CreateEntityBodies()
	{
		entityList = new List<ConvoBot>();

		for (int i = 0; i < populationSize; i++)
		{
			ConvoBot convoBot = new ConvoBot();
			convoBot.Init(nets[i]);
			entityList.Add(convoBot);
		}
	}

	void Finalizer()
	{
		nets.Sort();
		//for (int i = 0; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
		//{
		//	nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half
		//	nets[i].Mutate();
		//	nets[i].Mutate();
		//	nets[i].Mutate();

		//	nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
		//	nets[i + (populationSize - 2) / 2].Mutate();

		//	nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//	nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//}
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.WriteLine("* Copying weights and mutating networks...");
		Parallel.For(0, (populationSize - 2) / 2, i =>
		{
			nets[i] = new NeuralNetwork(nets[populationSize - 1]);     //Copies weight values from top half networks to worst half
			nets[i].Mutate();

			nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
			nets[i + (populationSize - 2) / 2].Mutate();

			nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
			nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		});
		Console.WriteLine("* Done.");
		//if ((highestFitness / 100) > lastBest)
		//{
		//	Parallel.For(0, (populationSize - 2) / 2, i =>
		//	{
		//		Console.WriteLine("* Restarting net " + i + " of " + (populationSize - 2) / 2);
		//		nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half
		//		nets[i].Mutate();

		//		nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
		//		nets[i + (populationSize - 2) / 2].Mutate();

		//		nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//		nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//	});
		//}
		//else
		//{
		//	Parallel.For(0, (populationSize - 2) / 2, i =>
		//	{
		//		Console.WriteLine("* Restarting net " + i + " of " + (populationSize - 2) / 2);
		//		nets[i] = new NeuralNetwork(nets[i]);     //Copies weight values from top half networks to worst half

		//		nets[i].RandomizeWeights();
		//		nets[i].Mutate();

		//		nets[i + (populationSize - 2) / 2] = new NeuralNetwork(nets[populationSize - 1]);
		//		nets[i + (populationSize - 2) / 2].Mutate();

		//		nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//		nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
		//	});
		//}
		Console.ResetColor();

		for (int i = 0; i < populationSize; i++)
		{
			nets[i].SetFitness(0f);
		}

		//CreateEntityBodies(nets, populationSize);
		//return nets;
	}

	void InitEntityNeuralNetworks()
	{
		if (generationNumber > 1 && startup == true)
			GatherPersistence();
		else
			collectedWeights = null;

		if (populationSize % 2 != 0)
		{
			populationSize++;
		}

		nets = new List<NeuralNetwork>();

		Console.ForegroundColor = ConsoleColor.Blue;
		Parallel.For(0, populationSize, i =>
		{
			NeuralNetwork net = new NeuralNetwork(layers, collectedWeights);
			Console.WriteLine("* Creating net: " + i + " of " + populationSize);
			net.Mutate();
			if (persistenceNetwork != null)
				net.weights = persistenceNetwork.weights;
			nets.Add(net);
		});
		Console.ResetColor();

		startup = false;
		Console.Clear();
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("✓ EVERYTHING READY ✓");
		Console.Write("Just let this program process and learn, and only exit if ");
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write("BLUE ");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write("text isn't getting printed to screen. (that is when it is saving or loading data). I have finally implemented networking! Now, as long as you have an internet connection, the weights data will automatically be sent to my server! Hooray!\n");
		Console.ResetColor();
	}

	void GatherPersistence()
	{
		persistenceNetwork = new NeuralNetwork(layers, null);

		// New System
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.WriteLine("* Loading...");
		BinaryFormatter bf = new BinaryFormatter();
		using (FileStream fs = new FileStream(".\\dat\\WeightSave.dat", FileMode.Open))
			persistenceNetwork.weights = (float[][][])bf.Deserialize(fs);
		Console.WriteLine("* Finished Loading.");
		Console.ResetColor();

		// Old System
		//StreamReader sr = File.OpenText(".\\dat\\WeightSave.dat");
		//string[] alllines = sr.ReadToEnd().Split('\n');
		//Console.ForegroundColor = ConsoleColor.Blue;
		//Parallel.For(0, persistenceNetwork.weights.Length, i =>
		//{
		//	Parallel.For(0, persistenceNetwork.weights[i].Length, j =>
		//	{
		//		for (int k = 0; k < persistenceNetwork.weights[i][j].Length; k++)
		//		{
		//			foreach (string line in alllines)
		//			{
		//				if (line.Split("=")[0] != i.ToString())
		//					continue;
		//				if (line.Split("=")[1] != j.ToString())
		//					continue;
		//				if (line.Split("=")[2] != k.ToString())
		//					continue;
		//				Console.WriteLine("Reading: " + line);
		//				persistenceNetwork.weights[i][j][k] = float.Parse(line.Split("=")[3]);
		//			}

		//		}
		//	});
		//});
		//Console.ResetColor();
		//sr.Close();

		collectedWeightsCopy = persistenceNetwork.weights; //convert to 3D array
	}

	static void Upload(float fitness)
	{
		Console.ForegroundColor = ConsoleColor.Magenta;

		File.Move(".\\dat\\WeightSave.dat", ".\\dat\\" + fitness + "_WeightSave.dat");
		Console.WriteLine("* Moved \".\\dat\\WeightSave.dat\" to \".\\dat\\" + fitness + "_WeightSave.dat\"");
		File.Move(".\\dat\\WeightSaveMeta.meta", ".\\dat\\" + fitness + "_WeightSaveMeta.meta");
		Console.WriteLine("* Moved \".\\dat\\WeightSaveMeta.meta\" to \".\\dat\\" + fitness + "_WeightSaveMeta.meta\"");

		// Upload weight save
		Console.WriteLine("* Uploading \".\\dat\\" + fitness + "_WeightSave.dat\" to http://achillium.us.to/objectrecognitionneuralnetdata/");
		System.Net.WebClient Client = new System.Net.WebClient();
		Client.Headers.Add("enctype", "multipart/form-data");
		byte[] result = Client.UploadFile("http://achillium.us.to/objectrecognitionneuralnetdata/uploadweights.php", "POST", ".\\dat\\" + fitness + "_WeightSave.dat");
		string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
		Console.WriteLine("* Uploaded \".\\dat\\" + fitness + "_WeightSave.dat\"");

		// Upload weight save meta
		Console.WriteLine("* Uploading \".\\dat\\" + fitness + "_WeightSaveMeta.meta\" to http://achillium.us.to/objectrecognitionneuralnetdata/");
		System.Net.WebClient ClientTwo = new System.Net.WebClient();
		ClientTwo.Headers.Add("enctype", "multipart/form-data");
		byte[] resultTwo = ClientTwo.UploadFile("http://achillium.us.to/objectrecognitionneuralnetdata/uploadweights.php", "POST", ".\\dat\\" + fitness + "_WeightSaveMeta.meta");
		string sTwo = System.Text.Encoding.UTF8.GetString(resultTwo, 0, resultTwo.Length);
		Console.WriteLine("* Uploaded \".\\dat\\" + fitness + "_WeightSaveMeta.meta\"");

		File.Move(".\\dat\\" + fitness + "_WeightSave.dat", ".\\dat\\WeightSave.dat");
		Console.WriteLine("* Moved \".\\dat\\" + fitness + "_WeightSave.dat\" to \".\\dat\\WeightSave.dat\"");
		File.Move(".\\dat\\" + fitness + "_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
		Console.WriteLine("* Moved \".\\dat\\" + fitness + "_WeightSaveMeta.meta\" to \".\\dat\\WeightSaveMeta.meta\"");

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("* Synced with server");
		Console.ResetColor();
	}

	static void Download(string s)
	{
		Console.ForegroundColor = ConsoleColor.Magenta;

		System.Net.WebClient Client = new System.Net.WebClient();

		Console.WriteLine("* Downloading \"" + s + "_WeightSave.dat\" from http://achillium.us.to/objectrecognitionneuralnetdata/" + s + "_WeightSave.dat");
		Client.DownloadFile(new Uri("http://achillium.us.to/objectrecognitionneuralnetdata/" + s + "_WeightSave.dat"), @".\dat\temp_WeightSave.dat");
		Console.WriteLine("* Downloaded \"" + s + "_WeightSave.dat\"");
		Console.WriteLine("* Downloading \"" + s + "_WeightSaveMeta.meta\" from http://achillium.us.to/objectrecognitionneuralnetdata/" + s + "_WeightSaveMeta.meta");
		Client.DownloadFile(new Uri("http://achillium.us.to/objectrecognitionneuralnetdata/" + s + "_WeightSaveMeta.meta"), @".\dat\temp_WeightSaveMeta.meta");
		Console.WriteLine("* Downloaded \"" + s + "_WeightSaveMeta.meta\"");

		if (File.Exists(".\\dat\\temp_WeightSave.dat"))
		{
			if (File.Exists(".\\dat\\WeightSave.dat"))
				File.Delete(".\\dat\\WeightSave.dat");
			File.Move(".\\dat\\temp_WeightSave.dat", ".\\dat\\WeightSave.dat");
		}
		if (File.Exists(".\\dat\\temp_WeightSaveMeta.meta"))
		{
			if (File.Exists(".\\dat\\WeightSaveMeta.meta"))
				File.Delete(".\\dat\\WeightSaveMeta.meta");
			File.Move(".\\dat\\temp_WeightSaveMeta.meta", ".\\dat\\WeightSaveMeta.meta");
		}

		StreamReader sr = File.OpenText(".\\dat\\WeightSaveMeta.meta");
		string firstLine = sr.ReadLine().Trim();
		string currentGen = firstLine.Split("#")[0];
		int generationNumber = int.Parse(currentGen) + 1;
		sr.Close();
		StreamWriter persistence = new StreamWriter(".\\dat\\WeightSaveMeta.meta");
		persistence.WriteLine((generationNumber).ToString() + "#" + s);
		persistence.Close();

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("* Synced with server");
		Console.ResetColor();
	}
}
