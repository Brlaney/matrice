// InputData.cs

namespace consoleApp
{
    public class InputData
    {
        public int type { get; set; }
        public int units { get; set; }
        public int nodes { get; set; }
        public int members { get; set; }
        public int[,] NXY { get; set; }
        public int[,] NF { get; set; }
        public int[,] ND { get; set; }
        public int[,] CONN { get; set; }

        public InputData(JSONStructure jsonData)
        {
            type = jsonData.system;
            units = jsonData.units;

            nodes = jsonData.nodes;
            members = jsonData.members;

            int[,] nxy = new int[jsonData.nodes, 2];
            int[,] nf = new int[jsonData.nodes, 2];
            int[,] nd = new int[jsonData.nodes, 2];
            int[,] conn = new int[jsonData.members, 2];

            for (int i = 0; i < jsonData.nodes; i++)
            {
                nxy[i, 0] = jsonData.nxy[i][0];
                nxy[i, 1] = jsonData.nxy[i][1];

                nf[i, 0] = jsonData.nf[i][0];
                nf[i, 1] = jsonData.nf[i][1];

                nd[i, 0] = jsonData.nd[i][0];
                nd[i, 1] = jsonData.nd[i][1];
            }

            for (int i = 0; i < jsonData.members; i++)
            {
                conn[i, 0] = jsonData.conn[i][0];
                conn[i, 1] = jsonData.conn[i][1];
            }

            NXY = nxy;
            NF = nf;
            ND = nd;
            CONN = conn;
        }

        public class JSONStructure
        {
            public int system { get; set; }
            public int units { get; set; }
            public int nodes { get; set; }
            public int members { get; set; }
            public List<List<int>>? nxy { get; set; }
            public List<List<int>>? nf { get; set; }
            public List<List<int>>? nd { get; set; }
            public List<List<int>>? conn { get; set; }
        }
    }
}
