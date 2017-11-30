using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] edgeIndices = { 0, 1, 0, 2, 1, 2, 1, 7, 2, 4, 2, 7, 2, 8, 7, 8, 3, 4, 3, 5, 3, 6, 5, 6, 4, 6, 6, 9, 6, 8, 4, 8, 8, 9 };

            ConnectivityMap cl = new ConnectivityMap(10, edgeIndices);

            int i = 0;

            foreach (List<List<int>> clx in cl.cLists)
            {
                Console.WriteLine(i + ":\n");
                i++;
                foreach (List<int> cly in clx)
                {
                    foreach (int clz in cly)
                        Console.Write(clz.ToString() + ", ");
                    Console.WriteLine("\n -");
                    Console.ReadKey();
                }
                Console.WriteLine("\n ---");
            }

            Console.ReadKey();

        }
    }

    public class ConnectivityMap
    {
        public List<List<int>>[] cLists { get; private set; }
        public int[] EdgeIndices { get; private set; }
        public int NumVertices { get; private set; }

        public ConnectivityMap(int numVertices, int[] edgeIndices)
        {
            EdgeIndices = edgeIndices;
            NumVertices = numVertices;
            cLists = new List<List<int>>[NumVertices];

            //iterating through the lines
            for (int i = 0; i < edgeIndices.Length; i++)
            {
                int ind = edgeIndices[i];

                if (cLists[ind] == null)
                {
                    List<int> exceptions = new List<int>();
                    cLists[ind] = new List<List<int>>();
                    VertexIndex = ind;
                    FindNextNeighbors(new List<int>() { ind }, exceptions);
                }
            }
        }

        int VertexIndex = -1;

        void FindNextNeighbors(List<int> vertexIndices, List<int> exceptions)
        {
            exceptions.AddRange(vertexIndices);
            exceptions = exceptions.Distinct().ToList();
            if (exceptions.Count >= NumVertices)
                return;
            List<int> firstDeg = new List<int>();
            foreach(int i in vertexIndices)
                firstDeg.AddRange(FirstDegreeNeighbors(i, exceptions));
            
            if (firstDeg.Count > 0)
            {
                firstDeg = firstDeg.Distinct().ToList();
                exceptions.AddRange(firstDeg);
                cLists[VertexIndex].Add(firstDeg);
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
