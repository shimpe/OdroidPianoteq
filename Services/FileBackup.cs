using System;
using System.IO;

namespace OdroidPianoteq.Services
{
    public class FileBackup
    {
        public static string Backup(ref string logtxt, string srcfile, string extension=".bak")
        {
            if (!File.Exists(srcfile))
            {
                logtxt = $"No backup made since file {srcfile} doesn't exist";
                return "";
            }

            string newFilename = srcfile + extension;
            if (File.Exists(newFilename))
            {
                int suffix = 1;
                var tempNewFilename = $"{newFilename}_{suffix}";
                while (File.Exists(tempNewFilename))
                {
                    suffix++;
                    tempNewFilename = $"{newFilename}_{suffix}";
                }
                newFilename = tempNewFilename;
            }

            try
            {
                logtxt += $"Backup {srcfile} to {newFilename}";
                File.Copy(srcfile, newFilename);
            }
            catch (Exception e)
            {
                return e.ToString();
            }
            return "";
        }
    }
}