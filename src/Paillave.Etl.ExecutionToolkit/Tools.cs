
using System.IO;

namespace Paillave.Etl.ExecutionToolkit;

internal class Tools
{
    public static void OpenFile(string content, string extension)
    {
        string tempFilePath = Path.GetTempFileName();
        string htmlTempFilePath = Path.ChangeExtension(tempFilePath, extension);
        File.Move(tempFilePath, htmlTempFilePath);
        File.WriteAllText(htmlTempFilePath, content);
        new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(htmlTempFilePath) { UseShellExecute = true } }.Start();
    }
}
