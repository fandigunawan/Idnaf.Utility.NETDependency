//Idnaf Utility .NET Assembly Dependency Checker
//Copyright (C) 2014  Idnaf


//Idnaf Utility .NET Assembly Dependency Checker
//is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 2 of the License, or
//(at your option) any later version.

//Foobar is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Idnaf Utility .NET Assembly Dependency Checker.
//If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace Idnaf.Utility.NETDependency
{
    
    public class Program
    {
        public struct AssemblyInformation
        {
            public string Path;
            public string Name;
            public Version Version;
            public System.Globalization.CultureInfo CultureInfo;

        }
        private static List<AssemblyInformation> asmInformation = new List<AssemblyInformation>();
        private static bool isVerbose = false;
        public static void DisplayHeader()
        {
            ConsoleColor c1 = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Idnaf Utility .NET Assembly Dependency Checker");
            Console.WriteLine("Coded by Idnaf 2014. Governed under GPL v. 2.0");
            Console.ForegroundColor = c1;
        }
        public static void DisplayHelp()
        {
            
            Console.WriteLine("Usage : ");
            Console.WriteLine(Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase) + " [param] path");
            Console.WriteLine("[...] Indicate optional parameter");
            Console.WriteLine("/H showing this help");
            Console.WriteLine("/V Verbose");
            Console.WriteLine("/I: include path, separated by ; e.g. /I:lib;lib\\new");
            Console.WriteLine("path can contains wildcard such as *.dll:");

        }
        static void Main(string[] args)
        {

            List<string> includedPath = new List<string>();
            string mainAsm = string.Empty;

            DisplayHeader();

            if(args == null)
            {
                DisplayHelp();
                return;
            }
            if(args.Length == 0 )
            {
                DisplayHelp();
                return;
            }
            for (int i = 0; i < args.Length; i++)
            {

                if(i == args.Length - 1)
                {
                    if(args[i].ToUpper() == "/H")
                    {
                        DisplayHelp();
                    }
                    mainAsm = args[i];
                    break;
                }
                if(args[i].ToUpper().StartsWith("/I:"))
                {
                    string[] sliced = args[i].Replace("/I", string.Empty).Replace("/i:", string.Empty).Split(new char[] { ';' });
                    includedPath.AddRange(sliced);
                }
                if (args[i].ToUpper().StartsWith("/V"))
                {
                    isVerbose = true;
                }
            }
            includedPath.Add(Path.GetDirectoryName(mainAsm));
            // Add .NET framework paths
            includedPath.Add(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
            foreach(string s in includedPath)
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), s);
                DirectoryInfo di = new DirectoryInfo(fullPath);
                FileInfo[] fi = new string[] { "*.dll", "*.exe" }.SelectMany(i => di.GetFiles(i, SearchOption.TopDirectoryOnly)).Distinct().ToArray();
                foreach(FileInfo f in fi)
                {
                    if (f.FullName.Contains(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()) && f.Extension == ".dll")
                    {
                        GrabAssemblyInformation(f.FullName);
                    }
                    else
                    {
                        if (!f.FullName.Contains(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()))
                        {
                            GrabAssemblyInformation(f.FullName);
                        }
                    }
                }
            }
            string[] fileScanned = new string[] { mainAsm };
            if(mainAsm.Contains("*"))
            {
                string pattern = mainAsm.Replace(Path.GetDirectoryName(mainAsm), string.Empty).Replace(Path.DirectorySeparatorChar.ToString(), string.Empty);
                string mainPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(mainAsm));
                fileScanned = Directory.GetFiles(mainPath, pattern, SearchOption.TopDirectoryOnly);
            }
            Console.WriteLine("-------------------------------------------------------------------------------");
            foreach (string f in fileScanned)
            {
                Assembly a = Assembly.ReflectionOnlyLoadFrom(f);
                
                Console.WriteLine("Scan : " + f.Replace(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar, string.Empty));
                foreach (AssemblyName ad in a.GetReferencedAssemblies())
                {
                    bool isFound = false;
                    foreach (AssemblyInformation ai in asmInformation)
                    {
                        if (ad.Name == ai.Name)
                        {
                            isFound = true;
                            break;
                        }
                    }
                    ConsoleColor c1 = Console.ForegroundColor;
                    Console.Write(" => Dependency ");
                    if (isFound == false)
                    {
                        
                        
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Failed ");
                        Console.ForegroundColor = c1;
                        Console.Write(" : ");
                        Console.ForegroundColor = ConsoleColor.Red;                        
                    }
                    else
                    {                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("OK     ");
                        Console.ForegroundColor = c1;
                        Console.Write(" : ");
                        Console.ForegroundColor = ConsoleColor.Green;
                     
                    }
                    Console.WriteLine(ad.Name);
                    Console.ForegroundColor = c1;
                    
                }
                Console.WriteLine("-------------------------------------------------------------------------------");
            }

            Console.ReadKey();

        }
        /// <summary>
        /// Grab asembly information from a given path
        /// </summary>
        /// <param name="path"></param>
        public static void GrabAssemblyInformation(string path)
        {
            ConsoleColor c1 = Console.ForegroundColor;
            if (File.Exists(path))
            {
                try
                {
                    Assembly a = Assembly.ReflectionOnlyLoadFrom(path);
                    AssemblyInformation ai = new AssemblyInformation();
                    ai.Path = a.CodeBase;
                    ai.Name = a.GetName().Name;
                    ai.Version = a.GetName().Version;
                    ai.CultureInfo = a.GetName().CultureInfo;
                    if (isVerbose)
                    {
                        
                        Console.Write("Adding dependency Name=");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("{0}, ", ai.Name);
                        Console.ForegroundColor = c1;
                        Console.Write("Version=");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("{0}, ", ai.Version);
                        Console.ForegroundColor = c1;
                        Console.WriteLine("Culture={0}, PublicKey token={1}" , ai.CultureInfo.Name, (BitConverter.ToString (a.GetName().GetPublicKeyToken())));
                    }
                    asmInformation.Add(ai);
                }
                catch(Exception e)
                {
                    if (!path.Contains(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error in file : " + path + " Message" + e.Message);
                        Console.ForegroundColor = c1;
                    }
                    
                }
            }
            else 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File does not exists : " + path);
                Console.ForegroundColor = c1;
            }
 
        }
    }
}
