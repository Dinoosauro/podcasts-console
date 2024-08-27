using System.Net;
using System.Web;
using System.Xml;
using PodcastsConsole;
using Spectre.Console;

internal class Program
{
    private static PodcastContainer? PodcastContainer = null;
    private static async Task Main(string[] args)
    {
        AnsiConsole.Write(new FigletText("PodcastsConsole").LeftJustified().Color(Color.Blue));
        Settings settings = new();
        for (int i = 0; i < args.Length; i++) // Handle settings arguments
        {
            switch (args[i])
            {
                case "--episode-fallback":
                    settings.EpisodeNumberFallback = (Settings.MissingPodcastNumber)int.Parse(args[i + 1]);
                    break;
                case "--episode-start":
                    settings.PodcastNumberStartFrom = int.Parse(args[i + 1]);
                    break;
                case "--no-metadata":
                    settings.AddMetadata = false;
                    break;
                case "--no-description-in-comment":
                    settings.WriteDescriptionAlsoInComment = false;
                    break;
                case "--standard-podcast-genre":
                case "-g":
                    settings.UseStandardPodcastsGenre = true;
                    break;
                case "--keep-image":
                case "--no-img-reencode":
                case "-r":
                    settings.ReEncodeImage = false;
                    break;
                case "--max-width":
                case "-w":
                    settings.MaxWidth = int.Parse(args[i + 1]);
                    break;
                case "--max-height":
                case "-h":
                    settings.MaxHeight = int.Parse(args[i + 1]);
                    break;
                case "--jpeg-quality":
                case "-q":
                    settings.JpegQuality = int.Parse(args[i + 1]);
                    break;
                case "--sleep-time":
                case "--sleep":
                case "-s":
                    settings.SleepTime = int.Parse(args[i + 1]);
                    break;
                case "--url":
                case "-u":
                    settings.RSSUrl = args[i + 1];
                    break;
                case "--dir":
                case "-d":
                case "--download-directory":
                    settings.DownloadDirectory = args[i + 1];
                    break;

            }
        }
        if (settings.RSSUrl == null)
        {
            AnsiConsole.Write("Please provide a link to the Podcast URL:");
            settings.RSSUrl = Console.ReadLine();
        }
        if (settings.RSSUrl != null)
        {
            if (settings.RSSUrl.Contains("://antennapod.org") && settings.RSSUrl.Contains("url=")) // Get RSS feed from Antennapod URL
            {
                settings.RSSUrl = settings.RSSUrl[(settings.RSSUrl.LastIndexOf("url=") + 4)..];
                if (settings.RSSUrl.Contains('&')) settings.RSSUrl = settings.RSSUrl[..settings.RSSUrl.IndexOf('&')];
                settings.RSSUrl = HttpUtility.UrlDecode(settings.RSSUrl);
            }
            await AnsiConsole.Status().StartAsync("Getting URL information...", async ctx =>
            {
                PodcastContainer = await XMLUtils.GetAvailableFiles(settings.RSSUrl, settings);
            });
            if (PodcastContainer != null)
            {
                List<PodcastItemContainer> prompt = AnsiConsole.Prompt(new MultiSelectionPrompt<PodcastItemContainer>() // Ask the user the podcast entries to download
                .Title("Which podcast episode(s) you want to download?")
                .PageSize(10)
                .MoreChoicesText("[grey]Go up/down to show more podcast episodes[/]")
                .InstructionsText(
                "[grey](Press [blue]<space>[/] to toggle a podcast episode, " +
                "[green]<enter>[/] to start the download)[/]")
                .AddChoices(PodcastContainer.PodcastEpisodes));
                if (prompt != null)
                {
                    await AnsiConsole.Progress()
                    .Columns(
                    [
                        new TaskDescriptionColumn(), // Task description
                        new ProgressBarColumn(), // Progress bar
                        new PercentageColumn(), // Percentage
                        new SpinnerColumn(), // Spinner
                    ]).StartAsync(async ctx =>
                    {
                        foreach (PodcastItemContainer container in prompt)
                        {
                            if (container.Title == null || container.URL == null) continue; // Required fields
                            var Task = ctx.AddTask(container.Title ?? container.URL ?? "", new ProgressTaskSettings { AutoStart = false });
                            try
                            {
                                // Download audio file
                                using HttpResponseMessage response = await new HttpClient().GetAsync(container.URL);
                                response.EnsureSuccessStatusCode();
                                long? bytes = response.Content.Headers.ContentLength;
                                using Stream stream = await response.Content.ReadAsStreamAsync();
                                string? FileExtension = GetFileExtensionFromUrl(container.URL);
                                settings.DownloadDirectory ??= AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "PodcastDownloader" + Path.DirectorySeparatorChar + PodcastContainer.Name; // If the user hasn't specified the download directory, we'll use the path of the executable
                                if (!settings.DownloadDirectory.EndsWith(Path.DirectorySeparatorChar)) settings.DownloadDirectory += Path.DirectorySeparatorChar;
                                string path = settings.DownloadDirectory + container.Title + FileExtension;
                                Directory.CreateDirectory(path[..path.LastIndexOf(Path.DirectorySeparatorChar)]);
                                using FileStream file = new(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                                byte[] buffer = new byte[8192];
                                long totalRead = 0;
                                int bytesRead = 0;
                                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await file.WriteAsync(buffer, 0, bytesRead);
                                    totalRead += bytesRead;
                                    Task.Increment(bytes != null ? ((double)bytesRead / (bytes ?? 1) * 100) : 50);
                                }
                                file.Close(); // Close the file so that it can be later read by TagLib-Sharp
                                file.Dispose();
                                if (settings.AddMetadata) await Metadata.Apply(path, container, PodcastContainer.AlbumArt, PodcastContainer.Name, PodcastContainer.Category, settings);
                                Task.Increment(100); // Mark it completed
                                AnsiConsole.MarkupLine("[grey]Sleeping... [[" + settings.SleepTime + " ms]][/]");
                                Thread.Sleep(settings.SleepTime);
                            }
                            catch (Exception ex)
                            {
                                AnsiConsole.WriteException(ex);
                            }
                        }
                    });
                    AnsiConsole.MarkupLine("[green]Download completed![/]");
                }
            }
        }
    }
    /// <summary>
    /// Get the extension of the file to download from an URL
    /// </summary>
    /// <param name="url">the URL of the resource to download</param>
    /// <returns>the extension, with the dot (".") included</returns>
    private static string GetFileExtensionFromUrl(string url)
    {
        url = url[url.LastIndexOf('.')..];
        if (url.Contains('?')) url = url[..url.IndexOf('?')];
        if (url.Contains('&')) url = url[..url.IndexOf('&')];
        return url;
    }
}