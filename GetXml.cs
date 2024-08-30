namespace PodcastsConsole;

using System.Net;
using System.Text;
using System.Xml;
using Spectre.Console;

public class XMLUtils
{
    /// <summary>
    /// Format the URL, giving a correct link if it starts with "./" or "/"
    /// </summary>
    /// <param name="url">The URL to format</param>
    /// <param name="sourceContent">The source of the RSS feed</param>
    /// <returns>The URL that can be used for downloading the resource</returns>
    private static string? FormatUrl(string? url, string sourceContent)
    {
        if (url == null) return null;
        if (url.StartsWith("./")) url = sourceContent[..sourceContent.LastIndexOf('/')] + url[1..];
        if (url.StartsWith('/')) url = sourceContent[..sourceContent.IndexOf('/', sourceContent.IndexOf("://") + 3)] + url;
        return url;
    }
    /// <summary>
    /// Format the tag by removing indentation and line breaks
    /// </summary>
    /// <param name="source">The string to format</param>
    /// <param name="keepIndentation">If the XML indentation should be kept</param>
    /// <param name="KeepStandardNewLines">If the line breaks should be kept, even if the indentation is discarded</param>
    /// <param name="noSpace">If a space shouldn't be added instead of the line break</param>
    /// <returns>The formatted string</returns>
    private static string? FormatTag(string? source, bool keepIndentation, bool KeepStandardNewLines, bool noSpace)
    {
        if (source == null) return null;
        if (!keepIndentation)
        {
            StringBuilder builder = new();
            foreach (string split in source.Split("\n")) builder.Append(split.Trim()).Append(noSpace ? "" : KeepStandardNewLines ? "\n" : " ");
            source = builder.ToString();
            if (!noSpace) source = source[..^1]; // We do not need to trim the last space if it has never been added. We do this, instead of trimming, so that also new lines can be cut.
        }
        return source;
    }
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
            PodcastContainer container = new(FormatUrl(FormatTag(GetFromTagName(reader, "title"), settings.KeepIndentation, settings.KeepStandardNewLines, false), link), FormatUrl(FormatTag(((XmlElement?)reader.GetElementsByTagName("url")?[0])?.GetElementsByTagName("url")?[0]?.Value, false, false, true) ?? FormatTag(reader.GetElementsByTagName("itunes:image")[0]?.Attributes?["href"]?.Value, false, false, true), link), FormatTag(reader.GetElementsByTagName("itunes:category")?[0]?.Attributes?["text"]?.Value, false, false, false));
            for (int i = 0; i < podcastItems.Count; i++)
            {
                XmlElement? podcastElement = (XmlElement?)podcastItems[i]; // Get it as an XmlElement
                if (podcastElement != null)
                {
                    string? podcastNumber = FormatTag(GetFromTagName(podcastElement, "itunes:episode"), settings.KeepIndentation, settings.KeepStandardNewLines, false);
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
                    string? author = FormatTag(GetFromTagName(podcastElement, "itunes:author"), settings.KeepIndentation, settings.KeepStandardNewLines, false);
                    if (author == null || author?.Trim() == "") author = FormatTag(GetFromTagName(reader, "itunes:author"), settings.KeepIndentation, settings.KeepStandardNewLines, false); // Fallback: if no specific author is found for this episode, look it globally
                    container.PodcastEpisodes.Add(new PodcastItemContainer(FormatTag(GetFromTagName(podcastElement, "title"), settings.KeepIndentation, settings.KeepStandardNewLines, false), FormatTag(GetFromTagName(podcastElement, "description"), settings.KeepIndentation, settings.KeepStandardNewLines, false), FormatTag(GetFromTagName(podcastElement, "pubDate"), settings.KeepIndentation, settings.KeepStandardNewLines, false), podcastNumber, author, FormatUrl(FormatTag(podcastElement.GetElementsByTagName("enclosure")?[0]?.Attributes?["url"]?.Value, false, false, true), link)));
                }
            }
            return container;
        }
        return null;
    }
}