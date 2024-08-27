namespace PodcastsConsole;
/// <summary>
/// The main container of the Podcast that is being downloaded
/// </summary>
/// <param name="Name">The name of the show</param>
/// <param name="AlbumArt">The link of the album art to download</param>
/// <param name="Category">The category/genre of the podcast</param>
public class PodcastContainer(string? Name, string? AlbumArt, string? Category)
{
    /// <summary>
    /// The name of the show
    /// </summary>
    public string? Name = Name;
    /// <summary>
    /// The link of the album art to download
    /// </summary>
    public string? AlbumArt = AlbumArt;
    /// <summary>
    /// The category/genre of the podcast
    /// </summary>
    public string? Category = Category;
    /// <summary>
    /// The container of all the episodes of this show
    /// </summary>
    public List<PodcastItemContainer> PodcastEpisodes = [];
}
public class PodcastItemContainer(string? Title, string? Description, string? PublishedDate, string? EpisodeNumber, string? Author, string? URL)
{
    /// <summary>
    /// The title of this single episode
    /// </summary>
    public string? Title = Title;
    /// <summary>
    /// The description of this single episode
    /// </summary>
    public string? Description = Description;
    /// <summary>
    /// The date when this episode was published
    /// </summary>
    public string? PublishedDate = PublishedDate;
    /// <summary>
    /// The episode number of the current episode
    /// </summary>
    public string? EpisodeNumber = EpisodeNumber;
    /// <summary>
    /// The author of the current episode, or of the show if none is found
    /// </summary>
    public string? Author = Author;
    /// <summary>
    /// The URL of the resource to fetch
    /// </summary>
    public string? URL = URL;
    public override string ToString() // Override the ToString method so that the epsideo title can be displayed in the multiple selection
    {
        return (Title + " [Episode " + EpisodeNumber + "]").Replace("[", "[[").Replace("]", "]]");
    }
}