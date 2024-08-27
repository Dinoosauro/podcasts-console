using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Spectre.Console;

namespace PodcastsConsole;

public class Metadata
{
    /// <summary>
    /// The picture to add
    /// </summary>
    private static TagLib.Picture[]? picture = null;
    /// <summary>
    /// Add metadata to the file
    /// </summary>
    /// <param name="Path">The path of the file to edit</param>
    /// <param name="Episode">The PodcastItemContainer of the episode to edit</param>
    /// <param name="ThumbnailUrl">The URL of the thumbnail to fetch</param>
    /// <param name="ShowName">The name of the show</param>
    /// <param name="Category">The specific category of this show</param>
    /// <param name="settings">The settings for the current conversion</param>
    public static async Task Apply(string Path, PodcastItemContainer Episode, string? ThumbnailUrl, string? ShowName, string? Category, Settings settings)
    {
        TagLib.File file = TagLib.File.Create(Path);
        file.Tag.Album = ShowName;
        if (Episode.Author != null)
        {
            file.Tag.AlbumArtists = [Episode.Author];
            file.Tag.Performers = [Episode.Author];
        }
        file.Tag.Comment = Episode.Description;
        file.Tag.Description = Episode.Description;
        file.Tag.Genres = !settings.UseStandardPodcastsGenre ? [Category ?? "Podcasts"] : ["Podcasts"];
        file.Tag.Title = Episode.Title;
        if (uint.TryParse(Episode.EpisodeNumber?.Trim(), out uint result)) file.Tag.Track = result;
        if (Episode.PublishedDate != null)
        {
            if (uint.TryParse(Episode.PublishedDate.Length > 12 ? Episode.PublishedDate[12..Episode.PublishedDate.IndexOf(' ', 12)].Trim() : Episode.PublishedDate.Trim(), out uint year)) file.Tag.Year = year;
        }
        if (picture != null) file.Tag.Pictures = picture; // The picture has already been downlaoded or re-encoded. Use that image
        else if (ThumbnailUrl != null)
        {
            try
            {
                byte[] image = await new HttpClient().GetByteArrayAsync(ThumbnailUrl);
                if (!settings.ReEncodeImage)
                {
                    string outputFormat = ThumbnailUrl[(ThumbnailUrl.LastIndexOf('.') + 1)..];
                    picture = [new TagLib.Picture(){
                        Type = TagLib.PictureType.FrontCover,
                        MimeType = "image/" + (outputFormat.StartsWith("webp") ? "webp" : outputFormat.StartsWith("png") ? "png" : "jpeg"),
                        Data = image
                    }];
                }
                else
                {
                    using SixLabors.ImageSharp.Image reEnc = SixLabors.ImageSharp.Image.Load(image); // Load the new image from the byte[]
                    if (reEnc.Width > settings.MaxWidth || reEnc.Height > settings.MaxHeight) // Resize the image
                    {
                        reEnc.Mutate(operation => operation.Resize(reEnc.Width > reEnc.Height ? settings.MaxWidth : reEnc.Width * settings.MaxWidth / reEnc.Height, reEnc.Width > reEnc.Height ? settings.MaxWidth * reEnc.Height / reEnc.Width : settings.MaxWidth));
                    }
                    using MemoryStream output = new();
                    reEnc.SaveAsJpeg(output, new() { Quality = settings.JpegQuality });
                    picture = [new TagLib.Picture(){
                        Type = TagLib.PictureType.FrontCover,
                        MimeType = "image/jpeg",
                        Data = output.ToArray()
                    }];
                }
                file.Tag.Pictures = picture;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
            }
        }
        file.Save();
        file.Dispose();
    }
}