using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConvertFunscriptToVorze
{
    class Program
    {
        private static float multi = 2.4f; //Multiplicator for final calculation

        static void Main(string[] args)
        {
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (Path.GetExtension(file) == ".funscript")
                {
                    Convert(file, Path.GetFullPath(file) + ".csv");
                }
            }
        }

        static void Convert(string inputFile, string outputFile)
        {
            string inputScript = File.ReadAllText(inputFile);
            StreamWriter outputStream = File.CreateText(outputFile);  

            dynamic jsonObj = JsonConvert.DeserializeObject(inputScript);
            foreach (var childToken in jsonObj)
            {
                if (childToken.Name == "actions")
                {
                    Console.WriteLine("Starting conversion");

                    foreach (var actionParent in childToken)
                    {
                        foreach (var action in actionParent)
                        {
                            long thisAt = 0;
                            long thisPos = 0;

                            long nextAt = 0;
                            long nextPos = 0;

                            if (action.Next == null)
                            {
                                //Last Action, Send 0 Command
                                outputStream.WriteLine(thisAt / 100 + ",0,0");
                                continue;
                            }
                            else
                            {
                                foreach (var childAction in action)
                                {
                                    if (childAction.Name == "at")
                                    {
                                        thisAt = childAction.Value.Value;
                                    }
                                    if (childAction.Name == "pos")
                                    {
                                        thisPos = childAction.Value.Value;
                                    }
                                }
                                foreach (var childAction in action.Next)
                                {
                                    if (childAction.Name == "at")
                                    {
                                        nextAt = childAction.Value.Value;
                                    }
                                    if (childAction.Name == "pos")
                                    {
                                        nextPos = childAction.Value.Value;
                                    }
                                }
                            }

                            long posDif = 0;
                            int direction = 0;

                            if (thisPos - nextPos < 0)
                            {
                                posDif = -(thisPos - nextPos);
                                direction = 1;
                            }
                            else
                            {
                                posDif = thisPos - nextPos;
                                direction = 0;
                            }

                            //Really bad calculation done at 5 in the morning
                            double force = Math.Round((((posDif * 10) / ((nextAt - thisAt) / 10)) * multi));

                            outputStream.WriteLine(thisAt / 100 + "," + direction.ToString() + "," + force);
                        }
                    }
                }
            }
            outputStream.Close();
        }
    }
}
