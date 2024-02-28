using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matrix
{
    public partial class StructuralSystem
    {
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
