
using System.IO;


public static class FileHelper
{
    public static string SanitizeFileName(string fileName)
    {
        fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        fileName = fileName.Replace(" ", "_");

        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "Default_Beatmap";
        }

        return fileName;
    }
}
