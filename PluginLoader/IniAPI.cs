using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PluginLoader
{
    public static class IniAPI
    {
        private static readonly string iniPath = Environment.CurrentDirectory + "\\Plugins.ini";

        [DllImport("kernel32", EntryPoint = "WritePrivateProfileString")]
        public static extern long WriteIni(string section, string key, string val, string path);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, [In, Out] char[] retVal, int size, string filePath);

        public static long WriteIni(string section, string key, string val)
        {
            return WriteIni(section, key, val, iniPath);
        }

        public static string ReadIni(string section, string key, string def, int size = 255, string path = null, bool writeIt = false)
        {
            if (path == null)
                path = iniPath;

            var temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, writeIt ? "" : def, temp, size, path);
            string ret = temp.ToString();

            if (writeIt && string.IsNullOrEmpty(ret))
            {
                ret = def;
                WriteIni(section, key, ret, path);
            }

            return ret;
        }

        /// <summary>
        /// Retrieves the .ini file's sections.
        /// </summary>
        public static IEnumerable<string> GetIniSections(string path)
        {
            char[] ret = new char[ushort.MaxValue];
            GetPrivateProfileString(null, null, null, ret, ushort.MaxValue, path);
            return new List<string>(new string(ret).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}