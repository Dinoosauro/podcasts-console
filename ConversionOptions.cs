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
    /// A list of the RSS feed URLs to download. If not provided, it'll be prompted when running the application.
    /// </summary>
    public string[]? RSSUrl = null;
    /// <summary>
    /// The directory where the file will be downloaded. Defaults to the binary's location.
    /// </summary>
    public string? DownloadDirectory = null;
    /// <summary>
    /// The options for skipping the download of a file
    /// </summary>
    public enum SkipOptions
    {
        /// <summary>
        /// The file should be always downloaded. If it already exists, it'll be overwritten
        /// </summary>
        NO_SKIP = -1,
        /// <summary>
        /// If the file exists, don't overwrite it, and skip the download
        /// </summary>
        SKIP_FILE_OVERWRITE = 0,
        /// <summary>
        /// If the URL has already been downloaded, skip the download
        /// </summary>
        SKIP_FILE_DOWNLOADED_URL = 1
    }
    /// <summary>
    /// The behavior the application should have for skipping duplicate files (already-downloaded or with the same name)
    /// </summary>
    public SkipOptions DuplicateLogic = SkipOptions.NO_SKIP;
    /// <summary>
    /// If true, the script will stop downloading everything when a duplicate is found
    /// </summary>
    public bool BreakAtFirstDuplicate = false;
    /// <summary>
    /// Reverse the order the podcasts are displayed in the list. This also affects passing the podcast number from argument
    /// </summary>
    public bool ReversePodcastOrder = false;
    /// <summary>
    /// Keep the indentation of the XML file
    /// </summary>
    public bool KeepIndentation = false;
    /// <summary>
    /// Keep the new lines in the XML file, even if the indentation shouldn't be kept
    /// </summary>
    public bool KeepStandardNewLines = false;
    /// <summary>
    /// Save the RSS feed URLs in the RSSFeed.txt file
    /// </summary>
    public bool SaveRSSFeed = false;

}