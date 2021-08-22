# ObjectRecognitionNeuralNetwork
> This is a simple neural network built to detect objects in a photo. It is based off of the PricePredictNeuralNetwork project I am also working, with the same base system. My PC isn't very powerful, and even though I run this network most of the time, progress has basically stopped. If you have free CPU, then I would be super grateful if you could help contribute by letting it train on your system.

## How to use:
To run, simply open <code>/Executable/</code>, then run <code>NeuralNetConsoleCS.exe</code>. Then input how powerful your computer is (1-8) so it can optimize to work as best as possible. After that, no further input is required and you can just let it compute new generations to make the network better.

I finally added networking, so you no longer need to send me <code>/Executable/dat/WeightSave.dat</code>! Now, as long as you have an internet connection, the weights file will be automatically uploaded to my server (when it makes progress).

To increase performance, the images it is getting trained on are not shown. If you want to see how much the network knows, I just added a viewer program. You can run it at <code>/Executable/ObjectDetectionWeightPreviewer.exe</code>, then load in any image you want!

## The current best fitness is: <b>57.77%</b>
Your clients' fitness will stay up to date with this, and this everyone elses highest fitness because of the networking I added. I'll keep this up to date with each network evolution.
