namespace PodcastsConsole;
public class History
{
    /// <summary>
    /// The directory where the history file will be located
    /// </summary>
    private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "PodcastDownloader";
    /// <summary>
    /// The position of the historu file
    /// </summary>
    private static readonly string HistoryPath = BasePath + Path.DirectorySeparatorChar + "History.txt";
    /// <summary>
    /// Add an URL to the history file
    /// </summary>
    /// <param name="url">The URL to add</param>
    public static void WriteToHistory(string url)
    {
        Directory.CreateDirectory(BasePath);
        if (File.Exists(HistoryPath)) File.AppendAllLines(HistoryPath, [url]); else File.WriteAllLines(HistoryPath, [url]);
    }
    /// <summary>
    /// Get if an URL is in the history file
    /// </summary>
    /// <param name="url">The URL to check</param>
    /// <returns></returns>
    public static bool GetHistoryEntry(string url)
    {
        if (!File.Exists(HistoryPath)) return false;
        return File.ReadAllLines(HistoryPath).Contains(url);
    }
}