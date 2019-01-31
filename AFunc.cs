using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;             //pro beep
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AFunc
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

        /// <summary>
        /// Regex.Replace can act upon spaces. It can merge multiple spaces in a string to one space. We use a pattern to change multiple spaces to single spaces. The characters \s+ come in handy.
        /// </summary>
        /// <param name="value">Input String</param>
        /// <returns>Output String</returns>
        public static string RemoveWhiteSpaces(string value)
        {
            value = value.Trim();
            return Regex.Replace(value, @"\s+", " ");
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

        /// <summary>
        /// Launch the legacy application with some options set.
        /// </summary>
        public static void LaunchCommandLineApp(string exeFile, string parameter)
        {
            // Use ProcessStartInfo class.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = exeFile;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = parameter;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using-statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }

        }


    }
}
