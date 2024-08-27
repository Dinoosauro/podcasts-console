# podcasts-console

Download podcast episodes directly from the console

## Installation

You can find pre-built binaries from GitHub Releases. Otherwise, you can clone
this repository, and then build it locally (`dotnet restore` to install the
dependencies; `dotnet publish` to generate the executables)

## How it works

The usage of this tool is really simple. Run it, paste the RSS feed and choose
the podcast episodes you want to download. The application will take care of
everything else

## Command-line arguments

You can pass lots of arguments in the command line to change the behaviour of
this application. You can find all of them in the following table:

| Argument                                          | Description                                                                                                                                               | Followed by                                                                                                                |
| ------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| `--episode-fallback`                              | An integer that indicates what should be done if there's no episode number in the metadata                                                                | `-1`: Don't add the episode number</br> `0`: Make the oldest episode the first</br> `1`: Make the newest episode the first |
| `--episode-start`                                 | The oldest/newest podcast should have this number. Basically, the podcast starts with this index. Defaults to `1`                                         | A number                                                                                                                   |
| `--no-metadata`                                   | Don't add metadata                                                                                                                                        | Nothing                                                                                                                    |
| `--no-description-in-comment`                     | Avoid adding the description in the "Comment" tag. If passed, the description will be added only in the "Description" tag, that usually is more suitable  | Nothing                                                                                                                    |
| `--standard-podcast-genre`</br> `-g`              | In the Genre metadata tag, add `Podcasts` instead of the specific category of the podcast (if available)                                                  | Nothing                                                                                                                    |
| `--keep-image`</br> `--no-img-reencode`</br> `-r` | Add the original image to the audio file, without re-encoding it                                                                                          | Nothing                                                                                                                    |
| `--max-width`</br> `-w`                           | The maximum width of the re-encoded image                                                                                                                 | An integer                                                                                                                 |
| `--max-height`</br> `-h`                          | The maximum height of the re-encoded image                                                                                                                | An integer                                                                                                                 |
| `--sleep-time`</br> `--sleep`</br> `-s`           | The time to wait between downloads                                                                                                                        | An integer                                                                                                                 |
| `--url`</br> `-u`                                 | The URL of the RSS feed. If not provided, it'll be prompted when running the application                                                                  | A string with the URL                                                                                                      |
| `--dir`</br> `-d`</br> `--download-directory`     | The directory where the files will be saved. Defaults to the directory where the application binary is located, plus `ApplicationDownloder` / `Show name` | A string                                                                                                                   |

## Open source licenses

This project is published under the MIT license.

podcasts-console uses the following open source libraries:

- [TagLib-Sharp](https://github.com/mono/taglib-sharp) – LGPL 2.1
- [ImageSharp](https://github.com/SixLabors/ImageSharp) – Six Labors Split
  License
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) – MIT
  License
