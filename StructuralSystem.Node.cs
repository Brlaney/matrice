using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matrix
{
    public partial class StructuralSystem
    {
        public class Node
        {
            public int Index { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int F1 { get; set; }
            public int F2 { get; set; }
            public int D1 { get; set; }
            public int D2 { get; set; }
            public State R1 { get; set; }
            public State R2 { get; set; }

            public Node(int index,
                        int x, int y,
                        int f1, int f2,
                        int d1, int d2)
            {
                Index = index;
                X = x;
                Y = y;
                F1 = f1;
                F2 = f2;
                D1 = d1;
                D2 = d2;

                if (D1 == 0) 
                    R1 = State.Restrained;
                else
                    R1 = State.Unrestrained;

                if (D2 == 0)
                    R2 = State.Restrained;
                else
                    R2 = State.Unrestrained;
            }

            public override string ToString()
            {
                return $"Node {Index + 1}";
            }

            public enum State
            {
                Unrestrained,
                Restrained
            }
        }
    }
}
