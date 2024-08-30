using System.Text;
using System.Web;
using PodcastsConsole;
using Spectre.Console;

internal class Program
{
    /// <summary>
    /// Make sure the name of the file doesn't create conflict with the file structure of some OS (Windows)
    /// </summary>
    /// <param name="str">The string with the file/directory name. Do not pass the directory separator char, since it would be replaced</param>
    /// <returns>The formatted string</returns>
    private static string NameSanitizer(string str)
    {
        if (str.Contains('\n'))
        { // Delete new lines, since they aren't allowed in file names. We'll also trim each line, so that we can delete XML indentation
            string[] split = str.Split("\n");
            StringBuilder output = new();
            for (int i = 0; i < split.Length; i++) output.Append(split[i].Trim() + (split.Length - 1 == i ? "" : " ")); // Add the space for each line, excluding the last one
            str = output.ToString();
        }
        return str.Replace("<", "‹").Replace(">", "›").Replace(":", "∶").Replace("\"", "″").Replace("/", "∕").Replace("\\", "∖").Replace("|", "¦").Replace("?", "¿").Replace("*", "");
    }
    private static PodcastContainer? PodcastContainer = null;
    private static async Task Main(string[] args)
    {
        AnsiConsole.Write(new FigletText("PodcastsConsole").LeftJustified().Color(Color.Blue));
        Settings settings = new();
        List<string> downloadableItems = []; // The list of the position of the episodes to download. This is used only if the user passes this as an argument
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
                    settings.RSSUrl = [.. (settings.RSSUrl ?? []), args[i + 1]];
                    break;
                case "--dir":
                case "-d":
                case "--download-directory":
                    settings.DownloadDirectory = args[i + 1];
                    break;
                case "--skip-download-if":
                case "--skip-download":
                    settings.DuplicateLogic = (Settings.SkipOptions)int.Parse(args[i + 1]);
                    break;
                case "--exit-with-first-duplicate":
                case "--exit-duplicate":
                case "--exit-duplicates":
                case "--exit":
                case "-e":
                    settings.BreakAtFirstDuplicate = true;
                    break;
                case "--download-index":
                    int position = (settings.RSSUrl?.Length ?? 1) - 1; // Where the index should be added
                    while (downloadableItems.Count <= position) downloadableItems.Add(""); // Add an empty string so that that position is editable
                    downloadableItems[position] = args[i + 1];
                    break;
                case "--reverse-order":
                    settings.ReversePodcastOrder = true;
                    break;
                case "--keep-indentation":
                case "-i":
                    settings.KeepIndentation = true;
                    break;
                case "--keep-new-lines":
                    settings.KeepStandardNewLines = true;
                    break;
                case "-a":
                case "--batch-files":
                case "--batch-file":
                case "--from-file":
                    if (File.Exists(args[i + 1])) settings.RSSUrl = File.ReadAllLines(args[i + 1]).Where(line => line.Trim() != "").ToArray();
                    break;
                case "--save-rss-history":
                case "--rss-history":
                    settings.SaveRSSFeed = true;
                    break;
            }
        }
        if (settings.RSSUrl == null)
        {
            string? url = AnsiConsole.Prompt<string>(new TextPrompt<string>("Please provide a link to the Podcast URL:"));
            if (url != null) settings.RSSUrl = [url];
        }
        if (settings.RSSUrl != null)
        {
            if (settings.SaveRSSFeed) // Save the new URLs in the RSS feed
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "PodcastDownloader";
                if (!Directory.Exists(basePath)) Directory.CreateDirectory(basePath);
                string filePath = basePath + Path.DirectorySeparatorChar + "RSSSources.txt";
                if (!File.Exists(filePath)) File.Create(filePath).Close();
                string[] alreadyAddedSources = File.ReadAllLines(filePath);
                File.AppendAllLines(filePath, settings.RSSUrl.Where(line => !alreadyAddedSources.Contains(line))); // Avoid adding multiple times the same URL
            }
            for (int i = 0; i < settings.RSSUrl.Length; i++)
            {
                string finalUrl = settings.RSSUrl[i];
                if (settings.RSSUrl[i].Contains("://antennapod.org") && settings.RSSUrl[i].Contains("url=")) // Get RSS feed from Antennapod URL
                {
                    finalUrl = finalUrl[(finalUrl.LastIndexOf("url=") + 4)..];
                    if (finalUrl.Contains('&')) finalUrl = finalUrl[..finalUrl.IndexOf('&')];
                    finalUrl = HttpUtility.UrlDecode(finalUrl);
                }
                await AnsiConsole.Status().StartAsync("Getting URL information...", async ctx =>
                {
                    PodcastContainer = await XMLUtils.GetAvailableFiles(finalUrl, settings);
                });
                if (PodcastContainer != null)
                {
                    if (settings.ReversePodcastOrder) PodcastContainer.PodcastEpisodes.Reverse();
                    List<PodcastItemContainer>? prompt = null;
                    if (downloadableItems.Count > i && downloadableItems[i].Trim() != "") // Download the podcast episodes passed in the argument
                    {
                        prompt = [];
                        foreach (string item in downloadableItems[i].Split(","))
                        {
                            if (int.TryParse(item.Trim(), out int result) && PodcastContainer.PodcastEpisodes.Count > result) prompt.Add(PodcastContainer.PodcastEpisodes[result]);
                        }
                    }
                    else
                    { // Ask the user which episodes to download
                        prompt = AnsiConsole.Prompt(new MultiSelectionPrompt<PodcastItemContainer>() // Ask the user the podcast entries to download
                        .Title("Which podcast episode(s) you want to download?")
                        .PageSize(10)
                        .MoreChoicesText("[grey]Go up/down to show more podcast episodes[/]")
                        .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle a podcast episode, " +
                        "[green]<enter>[/] to start the download)[/]")
                        .AddChoices(PodcastContainer.PodcastEpisodes));
                    }
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
                                string? FileExtension = GetFileExtensionFromUrl(container.URL);
                                settings.DownloadDirectory ??= AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "PodcastDownloader" + Path.DirectorySeparatorChar + NameSanitizer(PodcastContainer.Name ?? "Unknown Podcast"); // If the user hasn't specified the download directory, we'll use the path of the executable
                                if (!settings.DownloadDirectory.EndsWith(Path.DirectorySeparatorChar)) settings.DownloadDirectory += Path.DirectorySeparatorChar;
                                string path = settings.DownloadDirectory + NameSanitizer(container.Title + FileExtension);
                                int shouldBreak = -1; // If 0, the download should be skipped. If 1, the application must break.
                                switch (settings.DuplicateLogic)
                                {
                                    case Settings.SkipOptions.SKIP_FILE_OVERWRITE: // If a file with the same name exists, skip it
                                        if (File.Exists(path)) shouldBreak = settings.BreakAtFirstDuplicate ? 1 : 0;
                                        break;
                                    case Settings.SkipOptions.SKIP_FILE_DOWNLOADED_URL: // If the same URL has been downloaded previously, skip it
                                        if (History.GetHistoryEntry(container.URL)) shouldBreak = settings.BreakAtFirstDuplicate ? 1 : 0;
                                        break;
                                }
                                if (shouldBreak == 0)
                                {
                                    AnsiConsole.MarkupLine("[grey]Skipping download of " + container.Title.Replace("[", "[[").Replace("]", "]]") + "[/]");
                                    continue;
                                }
                                else if (shouldBreak == 1)
                                {
                                    AnsiConsole.MarkupLine("[red]Found duplicate with " + container.Title.Replace("[", "[[").Replace("]", "]]") + ". The application will be closed.[/]");
                                    break;
                                }
                                var Task = ctx.AddTask((container.Title ?? container.URL ?? "").Replace("[", "[[").Replace("]", "]]"), new ProgressTaskSettings { AutoStart = false });
                                try
                                {
                                    // Download audio file
                                    using HttpResponseMessage response = await new HttpClient().GetAsync(container.URL);
                                    response.EnsureSuccessStatusCode();
                                    long? bytes = response.Content.Headers.ContentLength;
                                    using Stream stream = await response.Content.ReadAsStreamAsync();
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
                                    History.WriteToHistory(container.URL); // Save the entry to the history, so that, if the user wants so, this URL won't be re-downloaded
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