﻿// StructuralSystem.cs

/*using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static matrix.StructuralSystem;
using static System.Net.Mime.MediaTypeNames;*/

using System.Reflection.Metadata.Ecma335;

namespace matrix
{
    public partial class StructuralSystem
    {
        public system type { get; set; }
        public units Units { get; set; }

        public int nodes { get; set; }
        public int members { get; set; }
        public int degreesOfFreedom { get; set; }

        public List<int> RowsColsToRemove { get; set; } = new List<int>();
        public int[] RowsColsToUse { get; set; }

        public List<Node> nodeData { get; set; } = new List<Node>();
        public List<Member> MemberData { get; set; } = new List<Member>();

        public enum system
        {
            Trusses, // 0
            Beams,   // 1
            Frames   // 2
        }

        public enum units
        {
            US,     // 0
            Metric  // 1
        }

        public StructuralSystem(
            int system, int[,] nxy, int[,] nf, int[,] nd, int[,] conn, int unit)
        {
            type = (system)Enum.ToObject(typeof(system), system);
            Units = (units)Enum.ToObject(typeof(units), unit);

            nodes = nxy.Length / 2;
            members = conn.Length / 2;

            // Calculate global degrees of freedom depending on structural system
            if (type == StructuralSystem.system.Trusses || type == matrix.StructuralSystem.system.Beams)
                degreesOfFreedom = 2 * nodes;
            else
                degreesOfFreedom = 3 * nodes;

            // Create list of nodes
            for (int i = 0; i < nodes; i++)
            {
                int x = nxy[i, 0];
                int y = nxy[i, 1];

                int f1 = nf[i, 0];
                int f2 = nf[i, 1];

                int d1 = nd[i, 0];
                int d2 = nd[i, 1];

                if (d1 == 0)
                    RowsColsToRemove.Add(2 * i);

                if (d2 == 0)
                    RowsColsToRemove.Add(2 * i + 1);

                nodeData.Add(new Node(
                    i, x, y, f1, f2, d1, d2));
            }

            int[] numbers = new int[degreesOfFreedom];

            for (int i = 0; i < degreesOfFreedom; i++)
            {
                numbers[i] = i;
            }

            List<int> rowsAndCols = numbers.Except(RowsColsToRemove).ToList();
            RowsColsToUse = new int[rowsAndCols.Count];

            rowsAndCols.ForEach(delegate (int use) { RowsColsToUse[rowsAndCols.IndexOf(use)] = use; });

            // Create list of members
            for (int i = 0; i < members; i++)
            {
                int n1 = conn[i, 0];
                int n2 = conn[i, 1];

                int x1 = nxy[n1, 0];
                int y1 = nxy[n1, 1];
                int x2 = nxy[n2, 0];
                int y2 = nxy[n2, 1];

                MemberData.Add(new Member(
                    i, 2, 29000000, n1, n2, x1, y1, x2, y2));
            }
        }

        public double[,] GetGlobalK()
        {
            double[,] globalK = new double[degreesOfFreedom, degreesOfFreedom];

            foreach (Member member in MemberData)
            {
                foreach (Member.Mapping member_k in member.RowColumnValue)
                {
                    globalK[member_k.I, member_k.J] = globalK[member_k.I, member_k.J] + member_k.P;
                }
            }
            return globalK;
        }

        public double[,] ReduceGlobalK(double[,] unreducedK)
        {
            int nRows = degreesOfFreedom - RowsColsToRemove.Count;
            int nCols = degreesOfFreedom - RowsColsToRemove.Count;

            double[,] reducedK = new double[nRows, nCols];
            string[,] stringReduced = new string[nRows, nCols];

            for (int i = 0; i < nRows; i++)
            {
                for (int j = 0; j < nCols; j++)
                {
                    double k = unreducedK[RowsColsToUse[i], RowsColsToUse[j]];

                    reducedK[i, j] = k < 1 * Math.Pow(1, -10) ? 0 : k;
                    stringReduced[i, j] = $"k{RowsColsToUse[i]}{RowsColsToUse[j]}";
                }
            }

            Console.WriteLine("");
            for (int i = 0; i < RowsColsToUse.Length; i++)
            {
                //Console.WriteLine($"\t[ {reducedK[row, 0]:0.00}, {reducedK[row, 1]:0.00}, {reducedK[row, 2]:0.00}, {reducedK[row, 3]:0.00} ]");
                Console.WriteLine($"\t[ {stringReduced[i, 0]}, {stringReduced[i, 1]}, {stringReduced[i, 2]}, {stringReduced[i, 3]} ]");
            }

            return reducedK;
        }

        public class Determinant
        {
            //public double[,] Matrix { get; set; }
            public int Index { get; set; } = 0;
            public double Determinate { get; set; }
            public double[] Coefficients { get; set; }
            public List<Determinant> SubDeterminants { get; set; } = new List<Determinant>();

            public int[,,] GetAdjugate(int rank, int iteration)
            {
                int[,,] ret = new int[rank - 1, rank - 1, 2];

                if (iteration == 0)
                {
                    ret = new int[,,]
                    {
                        { { 1, 1 }, { 1, 2 }, { 1, 3 } },
                        { { 2, 1 }, { 2, 2 }, { 2, 3 } },
                        { { 3, 1 }, { 3, 2 }, { 3, 3 } }
                    };
                }
                else if (iteration == 1)
                {
                    ret = new int[,,]
                    {
                        { { 1, 0 }, { 1, 2 }, { 1, 3 } },
                        { { 2, 0 }, { 2, 2 }, { 2, 3 } },
                        { { 3, 0 }, { 3, 2 }, { 3, 3 } }
                    };
                }
                else if (iteration == 2)
                {
                    ret = new int[,,]
                    {
                        { { 1, 0 }, { 1, 1 }, { 1, 3 } },
                        { { 2, 0 }, { 2, 1 }, { 2, 3 } },
                        { { 3, 0 }, { 3, 1 }, { 3, 3 } }
                    };
                }
                else if (iteration == 3)
                {
                    ret = new int[,,]
                    {
                        { { 1, 0 }, { 1, 1 }, { 1, 2 } },
                        { { 2, 0 }, { 2, 1 }, { 2, 2 } },
                        { { 3, 0 }, { 3, 1 }, { 3, 2 } }
                    };
                }

                return ret;
            }

            public Determinant(double[,] matrix)
            {
                int rank = matrix.GetLength(0);

                // Not a 2 X 2 matrix
                if (rank > 2)
                {
                    Coefficients = new double[rank];
                    double[,,] subDets = new double[rank, rank - 1, rank - 1];

                    for (int i = 0; i < rank; i++)
                    {
                        Coefficients[i] = i % 2 == 0 ? matrix[0, i] : -1 * matrix[0, i];

                        int[,,] AdjugateIndexes = GetAdjugate(rank, i);

                        for (int j = 0; j < rank - 1; j++)
                        {
                            for (int k = 0; k < rank - 1; k++)
                            {
                                int _i = AdjugateIndexes[j, k, 0];
                                int _j = AdjugateIndexes[j, k, 1];

                                subDets[i, j, k] = matrix[_i, _j];
                            }
                        }
                    }

/*                    for (int i = 0; i < rank; i ++)
                    {
                        double[,] passIn = subDets[0,,];

                        SubDeterminants.Add(new Determinant())
                    }
*/
                    Console.WriteLine(subDets);
                }
                else
                {
                    double finalDet = Calc2x2(matrix);
                }
            }

            public double Calc2x2(double[,] matrix)
            {
                return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
            }
        }

        /// <summary>
        /// Takes in a 2D matrix and displays in the console
        /// </summary>
        public static void Display2DMatrix(int[,] mtrx, string meta)
        {
            Console.WriteLine("");

            for (int i = 0; i < mtrx.Length / 2; i++)
            {
                Console.WriteLine($"\tn{i + 1}_{meta}:   {mtrx[i, 0]}, {mtrx[i, 1]} ");
            }

            Console.WriteLine("");
        }

        /// <summary>
        /// Takes in a 1D matrix and displays in the console along with provided prefix
        /// </summary>
        public static void Display1DMatrix(double[] mtrx, string prefix)
        {
            Console.WriteLine("");

            for (int i = 0; i < mtrx.Length; i++)
            {
                double thisEntry = mtrx[i];
                string displayTxt = thisEntry != 0 ? $"{Math.Round(thisEntry, 2)}" : "0.00";

                Console.WriteLine($"\t{prefix}{i + 1}:   {displayTxt} ");
            }

            Console.WriteLine("");
        }

    }
}
