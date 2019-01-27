using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;             //pro beep

namespace ZENITH_computing
{
    public static class AFunc
    {
        /// <summary>
        /// Vrátí název souboru: 1 - název bez přípony, 2 - název s příponou, 3 - adresář
        /// </summary>
        /// <param name="file">soubor s cestou</param>
        /// <param name="type">typ názvu</param>
        /// <returns></returns>
        public static string getFile(string file, int type)
        {
            if (type == 1) file = System.IO.Path.GetFileNameWithoutExtension(file);
            if (type == 2) file = System.IO.Path.GetFileName(file);
            if (type == 3) file = System.IO.Path.GetDirectoryName(file);
            return file;
        }
		
		
		public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.LastIndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static void Delay(int ds) { System.Threading.Thread.Sleep(ds * 10); }

        //---pípání
        public static void oneBeep() { SystemSounds.Beep.Play(); }

        public static void nBeep(int n)
        {
            for (int i = 0; i < n; i++)
            {
                oneBeep();
                Delay(60);
            }
        }


    }
}
