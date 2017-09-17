using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConvertFunscriptToVorze
{
    internal class Program
    {
        private static float multi = 2.4f; //Multiplicator for final calculation

        private static void Main(string[] args)
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
            else if (args.Length == 2)
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
            else if (error == 0)
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

        private static void Convert(string inputFile, string outputFile)
        {
            string inputScript = File.ReadAllText(inputFile);
            StreamWriter outputStream = File.CreateText(outputFile);

            Actions actions = JsonConvert.DeserializeObject<Actions>(inputScript);
            
            Console.WriteLine("Starting conversion of " + Path.GetFileName(inputFile));

            int i = 0;
            foreach (var action in actions.actions)
            {
                if (i == actions.actions.Count - 1)
                {
                    outputStream.WriteLine(action.at + ",0,0");
                    break;
                }
                
                var thisAt = action.at;
                var thisPos = action.pos;

                var nextAt = actions.actions[i + 1].at;
                var nextPos = actions.actions[i + 1].pos;

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
                if (force > 100)
                {
                    force = 100;
                }
                outputStream.WriteLine(thisAt / 100 + "," + direction.ToString() + "," + force);

                i++;
            }
            outputStream.Close();
            Console.WriteLine("Finished conversion of " + Path.GetFileNameWithoutExtension(inputFile));
        }

        private class Actions
        {
            public List<action> actions;

            public class action
            {
                public long at;
                public long pos;
            }
        }
    }
}