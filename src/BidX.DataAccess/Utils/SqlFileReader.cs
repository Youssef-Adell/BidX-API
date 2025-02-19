namespace BidX.DataAccess.Utils;
using System.IO;

public static class SqlFileReader
{
    public static string ReadSql(string sqlFileName)
    {
        var baseDirectory = AppContext.BaseDirectory; // Base directory of the application
        var fullPath = Path.Combine(baseDirectory, "SQL", sqlFileName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"SQL file not found: {fullPath}");
        }
        return File.ReadAllText(fullPath);
    }
}
