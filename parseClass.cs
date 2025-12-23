using System.Collections.Generic;
using System.IO;

namespace chkdeps
{
    class fileSyntaxs
    {
        public string name;
        public string pathPostfix;
        public int skiplines;

        public fileSyntaxs(string name, int skiplines, string pathPostfix = "\\")
        {
            this.name = name;
            this.skiplines = skiplines;
            this.pathPostfix = pathPostfix;
        }
    }
    class parseClass
    {
        public List<fileSyntaxs> mapFile = new List<fileSyntaxs>();
        public List<fileSyntaxs> scoFile = new List<fileSyntaxs>();
        public List<fileSyntaxs> sliFile = new List<fileSyntaxs>();
        public List<fileSyntaxs> modelFile = new List<fileSyntaxs>();

        public parseClass()
        {
            mapFile.Add(new fileSyntaxs("[object]", 1));
            mapFile.Add(new fileSyntaxs("[attachObj]", 1));
            mapFile.Add(new fileSyntaxs("[spline]", 1));
            mapFile.Add(new fileSyntaxs("[splineAttachement]", 1));
            mapFile.Add(new fileSyntaxs("[splineAttachement_repeater]", 3));

            scoFile.Add(new fileSyntaxs("[mesh]", 0, "\\model\\"));
            scoFile.Add(new fileSyntaxs("[collision_mesh]", 0, "\\model\\"));
            scoFile.Add(new fileSyntaxs("[model]", 0));
            scoFile.Add(new fileSyntaxs("[sound]", 0));

            sliFile.Add(new fileSyntaxs("[texture]", 0, "\\texture\\"));

            modelFile.Add(new fileSyntaxs("[mesh]", 0));
        }

        public List<string> depsFiles = new List<string>();
        public List<string> subdepsFiles = new List<string>();

        private int compareList(string chkLine, List<fileSyntaxs> list)
        {
            foreach (fileSyntaxs lpos in list)
            {
                if (string.Compare(chkLine, lpos.name) == 0)
                    return lpos.skiplines;
            }
            return -1;
        }

        private fileSyntaxs compareListEx(string chkLine, List<fileSyntaxs> list)
        {
            foreach (fileSyntaxs lpos in list)
            {
                if (string.Compare(chkLine, lpos.name) == 0)
                    return lpos;
            }
            return null;
        }

        public void enumerateTextObjectsFiles(string plikText)
        {
            if (!File.Exists(plikText))
            {
                return;
            }

            var lines = File.ReadLines(plikText);
            foreach (string line in lines)
            {
                if (!depsFiles.Contains(line))
                    depsFiles.Add(line);
            }
        }

        public void enumerateDeps(string plikMapy)
        {
            int skiplines = -1;
            //var lines = File.ReadLines(plikMapy,System.Text.Encoding.Unicode);
            List<string> lines = new List<string>();
            using (var reader = new StreamReader(plikMapy, System.Text.Encoding.Default, true))
            {
                while (reader.Peek() > -1)
                    lines.Add(reader.ReadLine());

            }
            foreach (string line in lines)
            {
                if (skiplines >= 0)
                {
                    skiplines--;

                    if (skiplines < 0 && !depsFiles.Contains(line))
                        depsFiles.Add(line);

                    continue;
                }

                skiplines = compareList(line, mapFile);
            }
        }
        public void enumerateSubDeps(string plikobj)
        {
            //subdepsFiles.Clear();
            fileSyntaxs fsx = null;
            string mpath = Path.GetDirectoryName(plikobj);
            string ext = Path.GetExtension(plikobj).ToLower();
            if (ext == string.Empty)
                return;

            int skiplines = -1;
            //var lines = File.ReadLines(plikobj);
            List<string> lines = new List<string>();
            using (var reader = new StreamReader(plikobj, System.Text.Encoding.Default, true))
            {
                while (reader.Peek() > -1)
                    lines.Add(reader.ReadLine());

            }
            foreach (string line in lines)
            {
                if (skiplines >= 0)
                {
                    skiplines--;

                    string path = mpath + fsx.pathPostfix + line;

                    if (skiplines < 0 && !subdepsFiles.Contains(path))
                        subdepsFiles.Add(path);

                    if (fsx.name == "[model]")
                        enumerateSubDepsModel(path);

                    continue;
                }

                if (ext == ".sco")
                {
                    fsx = compareListEx(line, scoFile);
                    if (fsx == null)
                        continue;
                    skiplines = fsx.skiplines;
                    continue;
                }
                /*
                if (ext == ".sli")
                {
                    fsx = compareListEx(line, sliFile);
                    if (fsx == null)
                        continue;
                    skiplines = fsx.skiplines;
                    continue;
                }
                */
            }
        }

        public void enumerateSubDepsModel(string plikcfg)
        {
            if (!File.Exists(plikcfg))
                return;
            fileSyntaxs fsx = null;
            string mpath = Path.GetDirectoryName(plikcfg);

            int skiplines = -1;
            //var lines = File.ReadLines(plikcfg);
            List<string> lines = new List<string>();
            using (var reader = new StreamReader(plikcfg, System.Text.Encoding.Default, true))
            {
                while (reader.Peek() > -1)
                    lines.Add(reader.ReadLine());

            }
            foreach (string line in lines)
            {
                if (skiplines >= 0)
                {
                    skiplines--;

                    string path = mpath + fsx.pathPostfix + line;

                    if (skiplines < 0 && !subdepsFiles.Contains(path))
                        subdepsFiles.Add(path);

                    continue;
                }

                fsx = compareListEx(line, modelFile);
                if (fsx == null)
                    continue;
                skiplines = fsx.skiplines;
                continue;
            }
        }
    }
}
