using System.Data;

namespace PodcastsConsole;
public class Settings()
{
    /// <summary>
    /// What should be done with the episode number if not provided
    /// </summary>
    public enum MissingPodcastNumber
    {
        /// <summary>
        /// No podcast number will be added
        /// </summary>
        KEEP_NULL = -1,
        /// <summary>
        /// Add the podcast number: the oldest item will be the first track in the order
        /// </summary>
        ADD_FROM_THE_LAST = 0,
        /// <summary>
        /// Add the podcast number: the newest item will be the first track in the order
        /// </summary>
        ADD_FROM_THE_FIRST = 1
    }
    /// <summary>
    /// What should be done with the episode number if not provided
    /// </summary>
    public MissingPodcastNumber EpisodeNumberFallback = MissingPodcastNumber.KEEP_NULL;
    /// <summary>
    /// If there is no provided podcast nubmer, start from this:
    /// </summary>
    public int PodcastNumberStartFrom = 1;
    /// <summary>
    /// Add metadata to the output file
    /// </summary>
    public bool AddMetadata = true;
    /// <summary>
    /// Write the episode description in the "Comment" tag
    /// </summary>
    public bool WriteDescriptionAlsoInComment = true;
    /// <summary>
    /// In the genre, use "Podcasts" instead of the specific category
    /// </summary>
    public bool UseStandardPodcastsGenre = false;
    /// <summary>
    /// Re-encode the downloaded image
    /// </summary>
    public bool ReEncodeImage = true;
    /// <summary>
    /// The maximum width of the downloaded image
    /// </summary>
    public int MaxWidth = 700;
    /// <summary>
    /// The maximum height of the downloaded image
    /// </summary>
    public int MaxHeight = 700;
    /// <summary>
    /// The quality of the newly encoded JPEG image
    /// </summary>
    public int JpegQuality = 75;
    /// <summary>
    /// The time the application should wait before downloading the next podcast
    /// </summary>
    public int SleepTime = 100;
    /// <summary>
    /// The URL of the RSS feed. If not provided, it'll be prompted when running the application.
    /// </summary>
    public string? RSSUrl = null;
    /// <summary>
    /// The directory where the file will be downloaded. Defaults to the binary's location.
    /// </summary>
    public string? DownloadDirectory = null;

}