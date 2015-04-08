
using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace Emotions
{
    enum EmotionType
    {
        Neutral,
        Joy,
        Surprise,
        Anger,
        Sadness,
        Fear
    }

    class EmotionRecognizer
    {
        private readonly Dictionary<EmotionType,ActivationNetwork> _networks = new Dictionary<EmotionType, ActivationNetwork>();
        private readonly TrainingDataSet _trainingData;

        public EmotionRecognizer(TrainingDataSet trainingData)
        {
            _trainingData = trainingData;

            Train(EmotionType.Neutral);
            Train(EmotionType.Joy);
            Train(EmotionType.Surprise);
            Train(EmotionType.Anger);
            Train(EmotionType.Sadness);
            Train(EmotionType.Fear);
        }

        private void Train(EmotionType emotionType)
        {
            var network = new ActivationNetwork(new SigmoidFunction(), 6, 3, 3, 1);
            var inputs = new List<double[]>();
            var outputs = new List<double[]>();

            foreach (var row in _trainingData.Rows)
            {
                inputs.Add(row.AUs.Select(au => (double)au).ToArray());
                var output = row.EmotionType == emotionType ? 1.0 : 0.0;
                outputs.Add(new []{ output });
            }
            TrainNetwork(network, inputs.ToArray(), outputs.ToArray());
            _networks.Add(emotionType, network);
        }

        private static void TrainNetwork(ActivationNetwork network, double[][] inputs, double[][] otputs)
        {
            var trainer = new BackPropagationLearning(network);

            // Previous step error
            double prErr = 10000000;
            // Network error
            double error = 100;
            // Initial learning rate
            trainer.LearningRate = 1;
            // While error of network is big enough
            while (error > 0.001)
            {
                // Get network error
                error = trainer.RunEpoch(inputs, otputs);
                // If error changed slightly
                if (Math.Abs(error - prErr) < 0.000000001)
                {
                    // Lower the learning rate
                    trainer.LearningRate /= 2;
                    if (trainer.LearningRate < 0.001)
                        trainer.LearningRate = 0.001;
                }

                prErr = error;
            }
        }

        public double Compute(EmotionType key, double[] inputs)
        {
            var result = _networks[key].Compute(inputs);
            return result[0];
        }
    }
}
