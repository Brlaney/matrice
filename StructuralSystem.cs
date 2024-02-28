// StructuralSystem.cs

using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static matrix.StructuralSystem;
using static System.Net.Mime.MediaTypeNames;

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

        public List<Node> nodeData { get; set;} = new List<Node>();
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
            {
                degreesOfFreedom = 2 * nodes;
            }
            else  // Frame System
            {
                degreesOfFreedom = 3 * nodes;
            }

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
                {
                    RowsColsToRemove.Add(2 * i);
                }

                if (d2 == 0)
                {
                    RowsColsToRemove.Add(2 * i + 1);
                }

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

            rowsAndCols.ForEach(delegate (int use) {RowsColsToUse[rowsAndCols.IndexOf(use)] = use;});

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
            for (int i = 0; i < RowsColsToUse.Length; i ++)
            {
                //Console.WriteLine($"\t[ {reducedK[row, 0]:0.00}, {reducedK[row, 1]:0.00}, {reducedK[row, 2]:0.00}, {reducedK[row, 3]:0.00} ]");
                Console.WriteLine($"\t[ {stringReduced[i, 0]}, {stringReduced[i, 1]}, {stringReduced[i, 2]}, {stringReduced[i, 3]} ]");
            }

            return reducedK;
        }

        public class DeterminateCalc
        {
            public double[,] Matrix { get; set; }
            public int NumberOfRecursians { get; set; }
            public double Determinate { get; set; }
            public double[] Coefficients { get; set; } 
            public List<DeterminateCalc> determinateCalcs { get; set; }

            public DeterminateCalc(double[,] matrix)
            {
                NumberOfRecursians = matrix.GetLength(0) - 1;

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    Coefficients[i] = matrix[0, i];
                }
            }
        }

        public double CalculateDeterminate(double[,] matrix)
        {
            DeterminateCalc determinate = new (matrix);

            return determinate.Determinate;
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

        public class Member
        {
            public int Index { get; set; }
            public int Area { get; set; }
            public int E { get; set; }

            public double Length { get; set; }

            public double ThetaDegrees { get; set; }
            public double ThetaRadians { get; set; }

            public int N1 { get; set; }
            public int N2 { get; set; }

            public int X1 { get; set; }
            public int Y1 { get; set; }

            public int X2 { get; set; }
            public int Y2 { get; set; }

            public double DX { get; set; }
            public double DY { get; set; }

            public double C { get; set; }
            public double S { get; set; }

            public double[,] KK { get; set; } = new double[4, 4];
            public double[,] K { get; set; } = new double[4, 4];
            public double _k { get; set; }
            public int[] LocalToGlobal { get; set; } = new int[4];
            public List<Mapping> RowColumnValue { get; set; }

            public Member(int index, int area, int e,
                          int n1, int n2, 
                          int x1, int y1, 
                          int x2, int y2)
            {
                Index = index;

                LocalToGlobal[0] = 2 * n1;
                LocalToGlobal[1] = 2 * n1 + 1;
                LocalToGlobal[2] = 2 * n2;
                LocalToGlobal[3] = 2 * n2 + 1;

                Area = area;
                E = e;

                X1 = x1;
                X2 = x2;
                Y1 = y1;
                Y2 = y2;

                DX = x2 - x1;
                DY = y2 - y1;

                N1 = n1;
                N2 = n2;

                Length = Math.Sqrt(Math.Pow(DX, 2) + Math.Pow(DY, 2));

                // Calculate thetas in both degrees and radians
                double[,] thetas = GetThetas(DX, DY);
                ThetaDegrees = thetas[0, 0];
                ThetaRadians = thetas[0, 1];

                C = Math.Cos(ThetaRadians);
                S = Math.Sin(ThetaRadians);

                _k = (E * Area) / (Length * 12);

                K = AssembleK(C, S, _k);
                KK = AssembleKK(C, S, _k);
                RowColumnValue = GetMemberK(KK, LocalToGlobal);
            }

            /// <summary>
            /// Returns Member Stiffness Matrix in global coordinates including coefficient _k
            /// </summary>
            public static double[,] AssembleKK(double c, double s, double _k)
            {
                double[,] K = {
                    { _k * c * c, _k * c * s, -1 * _k * c * c, -1 * _k * c * s } ,
                    { _k * c * s, _k * s * s, -1 * _k * c * s, -1 * _k * s * s } ,
                    { -1 * _k * c * c, -1 * _k * c * s, _k * c * c, _k * c * s } ,
                    { -1 * _k * c * s, -1 * _k * s * s, _k * c * s, _k * s * s } ,
                };

                return K;
            }

            /// <summary>
            /// Returns Member Stiffness Matrix in global coordinates excluding coefficient _k
            /// </summary>
            public static double[,] AssembleK(double c, double s, double _k)
            {
                double[,] K = {
                    { c * c, c * s, -1 * c * c, -1 * c * s } ,
                    { c * s, s * s, -1 * c * s, -1 * s * s } ,
                    { -1 * c * c, -1 * c * s, c * c, c * s } ,
                    { -1 * c * s, -1 * s * s, c * s, s * s } ,
                };

                return K;
            }

            /// <summary>
            /// Gets theta in both degrees and radians
            /// </summary>
            public static double[,] GetThetas(double dx, double dy)
            {
                double[,] ret = new double[1, 2];

                // Theta = 0 degrees
                if (dx > 0 && dy == 0)
                {
                    ret[0, 0] = ret[0, 1] = 0;
                }

                // Theta = 90 degrees
                if (dx == 0 && dy > 0)
                {
                    ret[0, 0] = 90;
                    ret[0, 1] = 90 * (Math.PI / 180);
                }

                // Theta = 180 degrees
                if (dx < 0 && dy == 0)
                {
                    ret[0, 0] = 180;
                    ret[0, 1] = 180 * (Math.PI / 180);
                }

                // Theta = 270 degrees
                if (dx == 0 && dy < 0)
                {
                    ret[0, 0] = 270;
                    ret[0, 1] = 270 * (Math.PI / 180);
                }

                // Quadrant I
                if (dx > 0 && dy > 0)
                {
                    ret[0, 0] = Math.Atan(dy / dx) * (180 / Math.PI);
                    ret[0, 1] = ret[0, 0] * (Math.PI / 180);
                }

                // Quadrant II
                if (dx < 0 && dy > 0)
                {
                    ret[0, 0] = 180 - (Math.Atan(dy / dx) * (180 / Math.PI));
                    ret[0, 1] = ret[0, 0] * (Math.PI / 180);
                }

                // Quadrant III
                if (dx < 0 && dy < 0)
                {
                    ret[0, 0] = 270 - (Math.Atan(dy / dx) * (180 / Math.PI));
                    ret[0, 1] = ret[0, 0] * (Math.PI / 180);
                }

                // Quadrant IV
                if (dx > 0 && dy < 0)
                {
                    ret[0, 0] = 360 + (Math.Atan(dy / dx) * (180 / Math.PI));
                    ret[0, 1] = ret[0, 0] * (Math.PI / 180);
                }

                return ret;
            }

            public List<Mapping> GetMemberK(double[,] KK, int[] localToGlobal)
            {
                List<Mapping> ret = new();

                for (int i = 0; i < localToGlobal.Length; i++)
                {
                    for (int j = 0; j < localToGlobal.Length; j++)
                    {
                        ret.Add(new Mapping(localToGlobal[i], 
                                            localToGlobal[j], 
                                            KK[i, j]));
                    }
                }

                return ret;
            }

            public class Mapping
            {
                public int I { get; set; }
                public int J { get; set; }
                public double P { get; set; }

                public Mapping(int i, int j, double p)
                {
                    I = i;
                    J = j;
                    P = p;
                }

                public override string ToString()
                {
                    return $"({I}, {J}, {P})";
                }
            }

            public override string ToString()
            {
                string length = Length != 0 ? $"{Math.Round(Length, 2):00.00}" : "0.00";
                string theta = ThetaDegrees != 0 ? $"{Math.Round(ThetaDegrees, 2):0.00} {ThetaDegrees:0.00}" : $"0.00";

                return $"Member {Index + 1}  -  Length: {length}  - Theta: {theta}";
            }
        }
    }
}
