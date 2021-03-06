﻿using System;
using System.Runtime.Serialization;

namespace Crow.Core.SOM
{
    /// <summary>
    /// This class extends a <see cref="Network"/> and represents a Kohonen Self-Organizing Map.
    /// </summary>
    [Serializable]
    public class KohonenNetworkND : Network
    {
        /// <summary>
        /// Gets the winner neuron of the network
        /// </summary>
        /// <value>
        /// Winner Neuron
        /// </value>
        public PositionNeuron Winner
        {
            get { return (outputLayer as KohonenLayerND).WinnerND; }
        }

        /// <summary>
        /// Creates a new Kohonen SOM, with the specified input and output layers. (You are required
        /// to connect all layers using appropriate synapses, before using the constructor. Any changes
        /// made to the structure of the network here-after, may lead to complete malfunctioning of the
        /// network)
        /// </summary>
        /// <param name="inputLayer">
        /// The input layer
        /// </param>
        /// <param name="outputLayer">
        /// The output layer
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>inputLayer</c> or <c>outputLayer</c> is <c>null</c>
        /// </exception>
        public KohonenNetworkND(ILayer inputLayer, KohonenLayerND outputLayer)
            : base(inputLayer, outputLayer, TrainingMethod.Unsupervised)
        {
        }

        /// <summary>
        /// Deserialization Constructor
        /// </summary>
        /// <param name="info">
        /// Serialization information to deserialize and obtain the data
        /// </param>
        /// <param name="context">
        /// Serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public KohonenNetworkND(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// A protected helper function used to train single learning sample
        /// </summary>
        /// <param name="trainingSample">
        /// Training sample to use
        /// </param>
        /// <param name="currentIteration">
        /// Current training epoch (Assumed to be positive and less than <c>trainingEpochs</c>)
        /// </param>
        /// <param name="trainingEpochs">
        /// Number of training epochs (Assumed to be positive)
        /// </param>
        protected override void LearnSample(TrainingSample trainingSample, int currentIteration, int trainingEpochs)
        {
            // No validation here
            inputLayer.SetInput(trainingSample.InputVector);
            foreach (ILayer layer in layers)
            {
                layer.Run();
                layer.Learn(currentIteration, trainingEpochs);
            }
        }
    }
}