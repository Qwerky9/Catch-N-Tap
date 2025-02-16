using System;
using System.IO;
using TagLib;

public class MP3MetadataChecker 
{
    public static bool HasCoverImage(string filePath)
    {
        try
        {
            // Open the MP3 file using TagLib
            var file = TagLib.File.Create(filePath);

            // Check if the file has pictures
            if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
            {
                Console.WriteLine("Cover image found!");
                return true;
            }

            Console.WriteLine("No cover image found.");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file metadata: {ex.Message}");
            return false;
        }
    }
}
