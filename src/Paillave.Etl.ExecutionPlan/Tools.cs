using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Paillave.Etl.ExecutionPlan
{
    internal class Tools
    {
        public static void OpenFile(string content, string extension)
        {
            string tempFilePath = Path.GetTempFileName();
            string htmlTempFilePath = Path.ChangeExtension(tempFilePath, extension);
            File.Move(tempFilePath, htmlTempFilePath);
            File.WriteAllText(htmlTempFilePath, content);
            new Process { StartInfo = new ProcessStartInfo(htmlTempFilePath) { UseShellExecute = true } }.Start();
        }
    }
}
