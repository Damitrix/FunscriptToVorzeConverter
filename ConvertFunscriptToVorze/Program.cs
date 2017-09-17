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
        private static float multi = 2.4f;

        static void Main(string[] args)
        {
            string inputScript = File.ReadAllText(Directory.GetCurrentDirectory() + "/Script.funscript");

            StreamWriter outputScript = File.CreateText(Directory.GetCurrentDirectory() + "/output.csv");

            dynamic jsonObj = JsonConvert.DeserializeObject(inputScript);
            foreach (var childToken in jsonObj)
            {
                if (childToken.Name == "actions")
                {
                    foreach (var actionParent in childToken)
                    {
                        foreach (var action in actionParent)
                        {
                            long thisAt = 0;
                            long thisPos = 0;

                            long nextAt = 0;
                            long nextPos = 0;

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
                            try
                            {
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
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                //throw;
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

                            double force = Math.Round((((posDif * 10) / ((nextAt - thisAt) / 10)) * multi));

                            outputScript.WriteLine(thisAt / 100 + "," + direction.ToString() + "," + force);
                        }
                    }
                }
            }

            outputScript.Close();

            //pos unterschied * 10 / zeit
        }
    }
}
