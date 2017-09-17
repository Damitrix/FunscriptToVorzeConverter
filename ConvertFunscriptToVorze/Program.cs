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
            int files = 0;
            int error = 0;

            if (args.Length == 0)
            {
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                {
                    files++;
                    if (Path.GetExtension(file) == ".funscript")
                    {
                        try
                        {
                            Convert(file, Path.GetFullPath(file) + ".csv");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            error++;
                        }
                    }
                }
                Console.WriteLine("You can also convert a single file by using 2 arguments, like \"convert.exe script.funscript script.csv\".");
            }
            else if(args.Length == 2)
            {
                string input = args[0];
                string output = args[1];

                if (Path.GetDirectoryName(input) == "")
                {
                    input = Path.Combine(Directory.GetCurrentDirectory() + "\\" + input);
                }

                if (Path.GetDirectoryName(output) == "")
                {
                    output = Path.Combine(Directory.GetCurrentDirectory() + "\\" + output);
                }

                if (File.Exists(input) && File.Exists(output))
                {
                    try
                    {
                        Convert(input, output);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        error++;
                    }
                }
                else
                {
                    error++;
                    Console.WriteLine("Error: One or more files could not be found");
                }
                files = 1;
            }
            

            if (files == 0)
            {
                Console.WriteLine("Could not find any compatible file. Please make sure, that your scripts are ending with \".funscript\". Then try again");
            }
            else if(error == 0)
            {
                Console.WriteLine("Everything is finished :)");
                Console.WriteLine("Please note, that this process is incredibly inaccurate.");
            }
            else
            {
                Console.WriteLine("Welp, it appears something went wrong. You can inform me of any bugs, i'll try to patch them asap :)");
            }

            Console.WriteLine("Get more info at https://github.com/Damitrix/FunscriptToVorzeConverter");
            Console.WriteLine("Press any key to Close this.");
            Console.ReadKey();
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
                    Console.WriteLine("Starting conversion of " + Path.GetFileName(inputFile));

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
            Console.WriteLine("Finished conversion of " + Path.GetFileNameWithoutExtension(inputFile));
        }
    }
}
