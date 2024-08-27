namespace PodcastsConsole;

using System.Net;
using System.Xml;

public class XMLUtils
{
    /// <summary>
    /// Get the inner text of a XmlElement by providing a tag
    /// </summary>
    /// <param name="element">The XmlElement to use for this operation</param>
    /// <param name="tag">The tag used for this search</param>
    /// <returns>The string with the inner text if found; otherwise null</returns>
    private static string? GetFromTagName(XmlElement element, string tag)
    {
        return element.GetElementsByTagName(tag)?[0]?.InnerText;
    }
    /// <summary>
    /// Get the inner text of a XmlElement by providing a tag
    /// </summary>
    /// <param name="element">The XmlDocument to use for this operation</param>
    /// <param name="tag">The tag used for this search</param>
    /// <returns>The string with the inner text if found; otherwise null</returns>
    private static string? GetFromTagName(XmlDocument element, string tag)
    {
        return element.GetElementsByTagName(tag)?[0]?.InnerText;
    }
    /// <summary>
    /// Get all the available podcasts from a well-formatted RSS feed
    /// </summary>
    /// <param name="link">The link of the RSS feed</param>
    /// <param name="settings">The settings for this operation</param>
    /// <returns></returns>
    public static async Task<PodcastContainer?> GetAvailableFiles(string link, Settings settings)
    {
        XmlDocument reader = new();
        HttpClient httpClient = new();
        reader.LoadXml(await httpClient.GetStringAsync(link));
        XmlNodeList? podcastItems = reader.GetElementsByTagName("item");
        if (podcastItems != null)
        {
            PodcastContainer container = new(GetFromTagName(reader, "title"), ((XmlElement?)reader.GetElementsByTagName("url")?[0])?.GetElementsByTagName("url")?[0]?.Value ?? reader.GetElementsByTagName("itunes:image")[0]?.Attributes?["href"]?.Value, reader.GetElementsByTagName("itunes:category")?[0]?.Attributes?["text"]?.Value);
            for (int i = 0; i < podcastItems.Count; i++)
            {
                XmlElement? podcastElement = (XmlElement?)podcastItems[i]; // Get it as an XmlElement
                if (podcastElement != null)
                {
                    string? podcastNumber = GetFromTagName(podcastElement, "itunes:episode");
                    if (podcastNumber == null)
                    {
                        switch (settings.EpisodeNumberFallback)
                        {
                            case Settings.MissingPodcastNumber.ADD_FROM_THE_LAST:
                                podcastNumber = (podcastItems.Count - 1 - i + settings.PodcastNumberStartFrom).ToString();
                                break;
                            case Settings.MissingPodcastNumber.ADD_FROM_THE_FIRST:
                                podcastNumber = (i + settings.PodcastNumberStartFrom).ToString();
                                break;
                        }
                    }
                    string? author = GetFromTagName(podcastElement, "itunes:author");
                    if (author == null || author?.Trim() == "") author = GetFromTagName(reader, "itunes:author"); // Fallback: if no specific author is found for this episode, look it globally
                    container.PodcastEpisodes.Add(new PodcastItemContainer(GetFromTagName(podcastElement, "title"), GetFromTagName(podcastElement, "description"), GetFromTagName(podcastElement, "pubDate"), podcastNumber, author, podcastElement.GetElementsByTagName("enclosure")?[0]?.Attributes?["url"]?.Value));
                }
            }
            return container;
        }
        return null;
    }
}