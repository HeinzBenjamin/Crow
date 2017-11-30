using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

using Rhino.Geometry;

namespace Crow
{
    class Utils
    {
        public static double[][] GHTreeToMultidimensionalArray(GH_Structure<GH_Number> tree)
        {
            List<double[]> arr = new List<double[]>();
            for (int i = 0; i < tree.Branches.Count; i++)
            {
                List<double> branch = new List<double>();
                for (int j = 0; j < tree.Branches[i].Count; j++)
                {
                    try { branch.Add(tree[i][j].Value); }
                    catch { throw new Exception("There's an invalid item in your data at branch: " + i + ", item: " + j); }
                }
                arr.Add(branch.ToArray());
            }
            return arr.ToArray();
        }

        public static List<Point3d> GHTreeToPointList(GH_Structure<GH_Point> pointTree)
        {
            int listLengthes = pointTree.Branches[0].Count;
            for(int i = 0; i < pointTree.Branches.Count; i++) if (listLengthes != pointTree.Branches[i].Count) return new List<Point3d>();
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < pointTree.Branches.Count; i++)
            {
                for (int j = 0; j < pointTree.Branches[i].Count; j++)
                {
                    points.Add(pointTree[i][j].Value);
                }
            }
            return points;
        }

        public static GH_Structure<GH_Number> MultidimensionalArrayToGHTree(double[,] array)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            if (array.Length != 0)
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        tree.Append(new GH_Number(array[i, j]), new GH_Path(i));
                    }
                }
            }
            return tree;
        }

        public static GH_Structure<GH_Number> MultidimensionalArrayToGHTree(int[][] array)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            if (array.Length != 0)
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    int h = array[0].Length;
                    for (int j = 0; j < array[0].Length; j++)
                    {
                        tree.Append(new GH_Number(array[i][j]), new GH_Path(i));
                    }
                }
            }
            return tree;
        }

        public static GH_Structure<GH_Number> MultidimensionalArrayToGHTree(double[][] array)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            if (array.Length != 0)
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    int h = array[0].Length;
                    for (int j = 0; j < array[0].Length; j++)
                    {
                        tree.Append(new GH_Number(array[i][j]), new GH_Path(i));
                    }
                }
            }
            return tree;
        }

        public static double[] FlattenedInput(List<Point3d> points)
        {
            List<double> numbers = new List<double>();
            for (int i = 0; i < points.Count; i++)
            {
                numbers.Add(points[i].X);
                numbers.Add(points[i].Y);
                numbers.Add(points[i].Z);
            }
            return numbers.ToArray();
        }

        public static int AdressToIndex(int[][] adressBook, int[] adress)
        {
            for (int i = 0; i < adressBook.GetLength(0); i++)
            {
                int compliance = 0;
                for (int j = 0; j < adressBook[i].Length; j++)
                {                    
                    if (adressBook[i][j] == adress[j]) compliance++;
                    if (compliance == adress.Length) return i;
                }
            }
            return -1;
        }

        public static int[] IndexToAdress(int[] size, int index, int counter)
        {
            int[] currentAdress = new int[size.Length];

            if (index == counter) return currentAdress;
            else return IndexToAdress(size, index, counter++);
        }

        public static int[] ReverseArray(int[] array)
        {
            int[] newArray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[array.Length - i - 1];
            }
            return newArray;
        }

        public static double[,] ReverseArrayInFirstDimension(double[,] array)
        {
            double[,] newArray = new double[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < newArray.GetLength(0); i++)
            {
                for(int j = 0; j < array.GetLength(1); j++)
                {
                    newArray[i,j] = array[array.GetLength(0) - i - 1, j];
                }
                
            }
            return newArray;
        }

        public static int[][] ReverseArrayInSecondDimension(int[][] array)
        {
            int[][] newArray = new int[array.GetLength(0)][];
            for (int i = 0; i < newArray.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    newArray[i][j] = array[i][array.GetLength(1) - j - 1];
                }

            }
            return newArray;
        }

        public static double[][] SoftMax(double[][] inputVector)
        {
            for(int i = 0; i < inputVector.Length; i++)
            {
                inputVector[i] = Softmax(inputVector[i]);
            }
            return inputVector;
        }

        public static double[] Softmax(double[] inputVector)
        {
            double max = double.MinValue;
            foreach (double d in inputVector)
                if (d > max)
                    max = d;

            double sum = 0.0;
            for (int i = 0; i < inputVector.Length; i++)
            {
                inputVector[i] = Math.Exp(inputVector[i] - max);
                sum += inputVector[i];
            }

            for (int i = 0; i < inputVector.Length; i++)
                inputVector[i] /= sum;

            return inputVector;
        }

        public class ConnectivityMap
        {
            public List<List<int>>[] Lists { get; private set; }
            public int[] EdgeIndices { get; private set; }
            public int NumVertices { get; private set; }
            int VertexIndex = -1;

            public ConnectivityMap(int numVertices, int[] edgeIndices)
            {
                EdgeIndices = edgeIndices;
                NumVertices = numVertices;
                Lists = new List<List<int>>[NumVertices];

                //iterating through the lines
                for (int i = 0; i < edgeIndices.Length; i++)
                {
                    int ind = edgeIndices[i];

                    if (Lists[ind] == null)
                    {
                        List<int> exceptions = new List<int>();
                        Lists[ind] = new List<List<int>>();
                        VertexIndex = ind;
                        FindNextNeighbors(new List<int>() { ind }, exceptions);
                    }
                }
            }

            void FindNextNeighbors(List<int> vertexIndices, List<int> exceptions)
            {
                exceptions.AddRange(vertexIndices);
                exceptions = exceptions.Distinct().ToList();
                if (exceptions.Count >= NumVertices)
                    return;
                List<int> firstDeg = new List<int>();
                foreach (int i in vertexIndices)
                    firstDeg.AddRange(FirstDegreeNeighbors(i, exceptions));

                if (firstDeg.Count > 0)
                {
                    firstDeg = firstDeg.Distinct().ToList();
                    exceptions.AddRange(firstDeg);
                    Lists[VertexIndex].Add(firstDeg);
                    FindNextNeighbors(firstDeg, exceptions);
                }
            }

            public List<int> FirstDegreeNeighbors(int vertexInd, List<int> exceptions)
            {
                List<int> fdn = new List<int>();
                for (int i = 0; i < EdgeIndices.Length; i += 2)
                {
                    int indA = EdgeIndices[i];
                    int indB = EdgeIndices[i + 1];
                    if (vertexInd == indA) fdn.Add(indB);
                    else if (vertexInd == indB) fdn.Add(indA);
                }

                foreach (int e in exceptions)
                    fdn.RemoveAll(i => i == e);

                return fdn;
            }
        }

    }
}
