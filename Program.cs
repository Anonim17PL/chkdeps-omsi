using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

namespace chkdeps
{
    class Program
    {
        static List<string> listaPlikówMapy = new List<string>();

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Verifies OMSI 2 map files.");
                Console.WriteLine("\nCHKDEPS map_path game_path\n");
                Console.WriteLine("map_path\tPath to the folder containing the map's global.cfg file. Can be relative.");
                Console.WriteLine("map_path\tPath to the game's main folder. Can be relative.\n");
                Console.WriteLine("\tCHKDEPS maps\\Grundorf .");
                Console.WriteLine("\tCHKDEPS Berlin-Spandau ..\\");
                Console.WriteLine("\nPaths containing whitespaces must be enclosed in quotation marks:\n");
                Console.WriteLine("\tCHKDEPS \"Krummenaab Updated\" ..\\");
                Console.WriteLine("\tCHKDEPS \"maps\\Grande Merda 2022\" .");
                Console.WriteLine("\tCHKDEPS \"C:\\games\\OMSI 2\\maps\\Novi Smulj\" \"C:\\games\\OMSI 2\"");
                Console.WriteLine("\n\nCopyright (c) 2025 Anonim17PL. MIT License.");
                return;
            }
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            string sciezka = args[0] + "\\";
            string sciezkagl = args[1] + "\\";
            enumerateMapFiles(sciezka + "global.cfg");

            parseClass parseMap = new parseClass();
            Console.WriteLine("Number of tiles: " + listaPlikówMapy.Count.ToString());
            enumerateChronoMapFiles(sciezka);
            Console.WriteLine("\nAnalyzing map files...");

            string sfilepath = "";

            if (enumerateSimple(sciezka, "drivers.txt", out sfilepath))
                parseMap.enumerateTextObjectsFiles(sfilepath);
            if (enumerateSimple(sciezka, "humans.txt", out sfilepath))
                parseMap.enumerateTextObjectsFiles(sfilepath);
            if (enumerateSimple(sciezka, "parklist_p.txt", out sfilepath))
                parseMap.enumerateTextObjectsFiles(sfilepath);

            foreach (string plikMapy in listaPlikówMapy)
            {
                if (!File.Exists(sciezka + plikMapy))
                    ConsoleWarn("Map file not found: " + plikMapy);
                else
                    parseMap.enumerateDeps(sciezka + plikMapy);
            }

            Console.WriteLine("\nVerifying map files...");
            foreach (string plikobj in parseMap.depsFiles)
            {
                string sciezkaobj = sciezkagl + plikobj;
                if (!File.Exists(sciezkaobj))
                    ConsoleErr("Not found: " + plikobj);
                if (HasNonASCIIChars(sciezkaobj))
                    ConsoleWarn("The file path contains Unicode characters: " + plikobj);
            }

            Console.WriteLine("\nVerifying objects files...");
            foreach (string plikobj in parseMap.depsFiles)
            {
                string sciezkaobj = sciezkagl + plikobj;
                if (!File.Exists(sciezkaobj))
                    continue;

                parseMap.enumerateSubDeps(sciezkaobj);

            }
            foreach (string subplikobj in parseMap.subdepsFiles)
            {
                if (!File.Exists(subplikobj))
                    ConsoleErr("Not found: " + subplikobj);
                if (HasNonASCIIChars(subplikobj))
                    ConsoleWarn("The file path contains Unicode characters: " + subplikobj);
            }

            Console.ForegroundColor = defaultColor;
        }

        static bool enumerateSimple(string path, string file, out string filepath)
        {
            filepath = path + file;
            if (!File.Exists(filepath))
            {
                ConsoleWarn("Map file not found: " + file);
                return false;
            }
            return true;
        }

        static void enumerateMapFiles(string plikGlobal)
        {
            int skiplines = -1;
            var lines = File.ReadLines(plikGlobal);
            foreach (string line in lines)
            {
                if (skiplines >= 0)
                {
                    skiplines--;
                    if (skiplines < 0 && !listaPlikówMapy.Contains(line))
                        listaPlikówMapy.Add(line);
                    continue;
                }

                if (string.Compare(line, "[map]") == 0)
                {
                    skiplines = 2;
                }
            }
        }
        static void enumerateChronoMapFiles(string path)
        {
            if (!Directory.Exists(path + "Chrono"))
                return;

            var chronoMapFiles = Directory.GetFiles(path+"Chrono", "*.map", SearchOption.AllDirectories);
            bool chronoDetected = false;
            foreach (string fpath in chronoMapFiles)
            {
                chronoDetected = true;
                string fname = Path.GetFileName(fpath);
                string frpath = fpath.Substring(path.Length);
                if (listaPlikówMapy.Contains(fname))
                    listaPlikówMapy.Add(frpath);
            }
            if (chronoDetected)
                Console.WriteLine("Chrono detected!");
        }

        static bool HasNonASCIIChars(string str)
        {
            return (Encoding.UTF8.GetByteCount(str) != str.Length);
        }

        static void ConsoleWarn(string str)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + str);
            Console.ForegroundColor = ConsoleColor.Green;
        }
        static void ConsoleErr(string str)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + str);
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }
}
