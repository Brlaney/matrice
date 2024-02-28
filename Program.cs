// Program.cs

namespace consoleApp
{
    using Newtonsoft.Json;
    using System;
    using static consoleApp.InputData;
    using static consoleApp.StructuralSystem;
    /*    using System.Net.Http.Headers;
        using System.Runtime.CompilerServices;
        using System.Transactions;
        using System.Xml.Linq;
        using System.Linq;*/

    internal class Program
    {
        static void Main()
        {
            //For the future, make more dynamic
            string fileName = "truss1.json";

            JSONStructure? jsonData = JsonConvert.DeserializeObject<JSONStructure?>(
                File.ReadAllText($"C:\\Users\\Brlan\\Documents\\Coding\\latest-matrix\\matrix\\data\\{fileName}"));

            InputData? inputData;
            if (jsonData == null)
            {
                Console.WriteLine("An error occurred trying to read the json file. Program will stop.");
                return;
            }
            else
            {
                inputData = new(jsonData);
            }

            StructuralSystem example_1 = new(inputData.type, inputData.NXY,
                                             inputData.NF, inputData.ND,
                                             inputData.CONN, inputData.units);

            /*            foreach (Member member in test.MemberData)
                        {
                            Console.WriteLine("");
                            Console.WriteLine($"Member {member.Index + 1}:   k = {Math.Round(member._k, 2)}");
                            Console.WriteLine($"Theta (degrees): {Math.Round(member.ThetaDegrees, 2)}");
                            Console.WriteLine($"Theta (radians): {Math.Round(member.ThetaRadians, 2)}");

                            for (int i = 0; i < 4; i++)
                            {
                                Console.WriteLine($"\tRow {i + 1}:   [ {member.KK[i, 0]:0.00}, {member.KK[i, 1]:0.00}, {member.KK[i, 2]:0.00}, {member.KK[i, 3]:0.00} ]");
                            }

                            Console.WriteLine("");
                        }*/

            foreach (Member member in example_1.MemberData)
            {
                Console.WriteLine($"Member {member.Index + 1}:");

                for (int i = 0; i < 4;  i++)
                {
                    Console.WriteLine($"k_{member.LocalToGlobal[i]}{member.LocalToGlobal[0]}  k_{member.LocalToGlobal[i]}{member.LocalToGlobal[1]}, k_{member.LocalToGlobal[i]}{member.LocalToGlobal[2]}, k_{member.LocalToGlobal[i]}{member.LocalToGlobal[3]}");
                }

                Console.WriteLine("");
            }

            double[,] globalK = example_1.GetGlobalK();
            Console.WriteLine("");

            for (int i = 0; i < 8; i++)
            {
                Console.WriteLine($"{globalK[i, 0]}, {globalK[i, 1]}, {globalK[i, 2]}, {globalK[i, 3]}, {globalK[i, 4]}, {globalK[i, 5]}, {globalK[i, 6]}, {globalK[i, 7]}");
            }

            //Console.WriteLine(globalK);

            double[,] reducedK = example_1.ReduceGlobalK(globalK);

            Console.WriteLine(reducedK);

            double theDeterminate = example_1.CalculateDeterminate(reducedK);

        }
    }
}
