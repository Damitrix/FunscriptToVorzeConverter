using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ConvertFunscriptToVorze
{
    internal class Program
    {
        private float Multi = 2.4f; //Multiplicator for final calculation
        private int MinChange = 300;
        private int Error;

        private static void Main(string[] args)
        {
            new Program().MainTask(args);
        }

        private void MainTask(string[] args)
        {
            int files = 0;
            int error = 0;

            if (args.Length == 0)
            {
                if (!ConvertAll())
                {
                    error++;
                }
                else
                {
                    files++;
                }
            }
            else
            {
                for (int arg = 0; arg < args.Length; arg++)
                {
                    switch (args[arg])
                    {
                        case "-m":
                            Multi = System.Convert.ToSingle(args[arg + 1]);
                            break;
                        case "-c":
                            MinChange = System.Convert.ToInt32(args[arg + 1]);
                            break;
                        default:
                            if (args.Length == 2 && !(args[0] == "-m" || args[0] == "-c"))
                            {
                                if (!ConvertSpecific(args))
                                {
                                    error++;
                                }
                                else
                                {
                                    files++;
                                }
                            }
                            break;
                    }
                }
                if (!ConvertAll())
                {
                    error++;
                }
                else
                {
                    files++;
                }
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

        private bool ConvertAll()
        {
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (Path.GetExtension(file) == ".funscript")
                {
                    try
                    {
                        Convert(file, Path.GetFullPath(file) + ".csv");
                        Console.WriteLine("You can also convert a single file by using 2 arguments, like \"convert.exe script.funscript script.csv\".");
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return false;
                    }
                }
            }
            return false;
        }

        private bool ConvertSpecific(string[] args)
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
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Error: One or more files could not be found");
                return false;
            }
            return true;
        }

        private void Convert(string inputFile, string outputFile)
        {
            string inputScript = File.ReadAllText(inputFile);
            StreamWriter outputStream = File.CreateText(outputFile);

            //Writing all Actions from Json to Array
            Actions actions = JsonConvert.DeserializeObject<Actions>(inputScript);
            
            Console.WriteLine("Starting conversion of " + Path.GetFileName(inputFile));

            Direction lastDirection = Direction.Left;
            int lastDirectionChange = 0;

            int i = 0;
            foreach (var action in actions.actions)
            {
                if (i == actions.actions.Count - 1)
                {
                    //No more Commands, setting to 0 and stopping
                    outputStream.WriteLine(action.at + ",0,0");
                    break;
                }
                
                var thisAt = action.at;
                var thisPos = action.pos;

                var nextAt = actions.actions[i + 1].at;
                var nextPos = actions.actions[i + 1].pos;

                long posDif = 0;
                Direction direction = 0;
                
                if (thisPos - nextPos < 0)
                {
                    posDif = -(thisPos - nextPos);

                    //Make sure, the unit will not be damaged by applying a minimum Delay between direction changes
                    if (lastDirection != Direction.Right && thisAt - lastDirectionChange > MinChange)
                    {
                        direction = Direction.Right;
                    }
                }
                else
                {
                    posDif = thisPos - nextPos;

                    //Make sure, the unit will not be damaged by applying a minimum Delay between direction changes
                    if (lastDirection != Direction.Left && thisAt - lastDirectionChange > MinChange)
                    {
                        direction = Direction.Left;
                    }
                }

                lastDirection = direction;
                lastDirectionChange = System.Convert.ToInt32(thisAt);

                //Really bad calculation done at 5 in the morning
                double force = Math.Round(((posDif * 10) / ((nextAt - thisAt) / 10)) * Multi);
                if (force > 100)
                {
                    force = 100;
                }
                outputStream.WriteLine(thisAt / 100 + "," + (int)direction + "," + force);

                i++;
            }
            outputStream.Close();
            Console.WriteLine("Finished conversion of " + Path.GetFileNameWithoutExtension(inputFile));
        }

        private enum Direction
        {
            Left,
            Right
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