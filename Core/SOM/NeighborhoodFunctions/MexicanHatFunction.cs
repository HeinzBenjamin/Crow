using System;
using System.Runtime.Serialization;

namespace Crow.Core.SOM.NeighborhoodFunctions
{
    /// <summary>
    /// Mexican Hat Neighborhood Function is the normalized second derivative of a Gaussian function.
    /// It is a continuous function with neighborhood value decreasing from unity at the winner to
    /// a negative value at a certain point (forming an inhibitory influence) and then gradually
    /// increasing to zero.
    /// </summary>
    [Serializable]
    public sealed class MexicanHatFunction : INeighborhoodFunction
    {
        /* 
         *  Mexican Hat Function = a * (1 - ((x-b)/c)square) * Exp( - 1/2 * ((x-b)/c)square)
         *
         *  The parameter 'a' is the height of the curve's peak, 'b' is the position of the center of
         *  the peak, and 'c' controls the width of the bell shape.
         *
         *  For a Mexican Hat Neighborhood function,
         *  a = unity (the neighborhood at the winner)
         *  b = winner position
         *  c = depends on training progress.
         *
         *  Initial value of c is obtained from the user (as learning radius)
         *  Note that, (x-b)square denotes the euclidean distance between winner neuron 'b' and neuron 'x' 
         *
         *  (Mexican hat function) vs (Hamming distance)
         *                         _
         *                        / \
         *              _____    |   |    _____
         *                   \__/     \__/
         *                         .
         *                       Winner
         */

        private readonly double sigma = 0d;

        /// <summary>
        /// Gets the value of sigma
        /// </summary>
        /// <value>
        /// Initial value of sigma
        /// </value>
        public double Sigma
        {
            get { return sigma; }
        }

        /// <summary>
        /// Creates a new Mexican Hat Neighborhood Function
        /// </summary>
        /// <param name="learningRadius">
        /// Initial Learning Radius
        /// </param>
        public MexicanHatFunction(int learningRadius)
        {
            // Full Width at Half Maximum for a Mexican Hat curve 
            //        = 1.2518753 * sigma
            // Full Width at Half Maximum (FWHM) is nothing but learning diameter
            // so, learning radius = 0.62593765 * sigma

            //this.sigma = learningRadius / 0.6259d;
            this.sigma = learningRadius / 0.5259d; ///works better somehow
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
        public MexicanHatFunction(SerializationInfo info, StreamingContext context)
        {
            Helper.ValidateNotNull(info, "info");
            this.sigma = info.GetDouble("sigma");
        }

        /// <summary>
        /// Populates the serialization info with the data needed to serialize the neighborhood function
        /// </summary>
        /// <param name="info">
        /// The serialization info to populate the data with
        /// </param>
        /// <param name="context">
        /// The serialization context to use
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>info</c> is <c>null</c>
        /// </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Helper.ValidateNotNull(info, "info");
            info.AddValue("sigma", sigma);
        }

        /// <summary>
        /// Determines the neighborhood of every neuron in the given Kohonen layer with respect to
        /// winner neuron using Mexican Hat function
        /// </summary>
        /// <param name="layer">
        /// The Kohonen Layer containing neurons
        /// </param>
        /// <param name="currentIteration">
        /// Current training iteration
        /// </param>
        /// <param name="trainingEpochs">
        /// Total number of training epochs
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <c>layer</c> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <c>trainingEpochs</c> is zero or negative
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <c>currentIteration</c> is negative or, if it is not less than <c>trainingEpochs</c>
        /// </exception>
        public void EvaluateNeighborhood(KohonenLayer layer, int currentIteration, int trainingEpochs)
        {
            Helper.ValidateNotNull(layer, "layer");
            Helper.ValidatePositive(trainingEpochs, "trainingEpochs");
            Helper.ValidateWithinRange(currentIteration, 0, trainingEpochs - 1, "currentIteration");

            // Winner co-ordinates
            int winnerX = layer.Winner.Coordinate.X;
            int winnerY = layer.Winner.Coordinate.Y;

            // Layer width and height
            int layerWidth = layer.Size.Width;
            int layerHeight = layer.Size.Height;

            // Optimization: Pre-calculated 2-Sigma-Square (1e-9 to make sure it is non-zero)
            double sigmaSquare = sigma * sigma + 1e-9;

            // Evaluate and update neighborhood value of each neuron
            foreach (PositionNeuron neuron in layer.Neurons)
            {
                int dx = Math.Abs(winnerX - neuron.Coordinate.X);
                int dy = Math.Abs(winnerY - neuron.Coordinate.Y);

                if (layer.IsRowCircular)
                {
                    dx = Math.Min(dx, layerWidth - dx);
                }
                if (layer.IsColumnCircular)
                {
                    dy = Math.Min(dy, layerHeight - dy);
                }

                double dxSquare = dx * dx;
                double dySquare = dy * dy;
                if (layer.Topology == LatticeTopology.Hexagonal)
                {
                    if (dy % 2 == 1)
                    {
                        dxSquare += 0.25 + (((neuron.Coordinate.X > winnerX) == (winnerY % 2 == 0)) ? dx : -dx);
                    }
                    dySquare *= 0.75;
                }
                double distanceBySigmaSquare = (dxSquare + dySquare) / sigmaSquare;
                neuron.neighborhoodValue = (1 - distanceBySigmaSquare) * Math.Exp(-distanceBySigmaSquare / 2);
            }
        }

        public void EvaluateNeighborhood(KohonenLayerND layer, int currentIteration, int trainingEpochs)
        {
            Helper.ValidateNotNull(layer, "layer");
            Helper.ValidatePositive(trainingEpochs, "trainingEpochs");
            Helper.ValidateWithinRange(currentIteration, 0, trainingEpochs - 1, "currentIteration");

            // Winner co-ordinates
            int[] winner = layer.WinnerND.CoordinateND;

            // Layer width and height
            int[] size = layer.Size;

            // Optimization: Pre-calculated 2-Sigma-Square (1e-9 to make sure it is non-zero)
            double sigmaSquare = sigma * sigma + 1e-9;

            // Evaluate and update neighborhood value of each neuron
            foreach (PositionNeuron neuron in layer.Neurons)
            {
                int[] dD = new int[winner.Length];
                for (int i = 0; i < winner.Length; i++)
                {
                    dD[i] = Math.Abs(winner[i] - neuron.CoordinateND[i]);
                }

                /// TO DO!!!
                
                /*if (layer.IsDimensionCircular[0])
                {
                    dx = Math.Min(dx, layerWidth - dx);
                }
                if (layer.IsDimensionCircular[1])
                {
                    dy = Math.Min(dy, layerHeight - dy);
                }*/

                double[] dDSquares = new double[winner.Length];
                for (int i = 0; i < dD.Length; i++) dDSquares[i] = dD[i] * dD[i];
                /*
                if (layer.Topology == LatticeTopology.Hexagonal)
                {
                    if (dy % 2 == 1)
                    {
                        dxSquare += 0.25 + (((neuron.Coordinate.X > winnerX) == (winnerY % 2 == 0)) ? dx : -dx);
                    }
                    dySquare *= 0.75;
                }*/

                double dSum = 0.0;
                for (int i = 0; i < dDSquares.Length; i++) dSum += dDSquares[i];
                double distanceBySigmaSquare = dSum / sigmaSquare;
                // double distanceBySigmaSquare = (dxSquare + dySquare) / sigmaSquare;
                neuron.neighborhoodValue = (1 - distanceBySigmaSquare) * Math.Exp(-distanceBySigmaSquare / 2);
            }
        }
    }
}