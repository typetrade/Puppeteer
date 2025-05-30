using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Puppeteer.BrowserData;
using Puppeteer.Helpers;
using Puppeteer.Helpers.Linux;
using static Puppeteer.BrowserFetcherOptions;

namespace Puppeteer
{
    /// <inheritdoc/>
    public class BrowserFetcher : IBrowserFetcher
    {
        private const string PublishSingleFileLocalApplicationDataFolderName = "Puppeteer";

        private static readonly Dictionary<SupportedBrowser, Func<Platform, string, string, string>> _downloadsUrl = new()
        {
            [SupportedBrowser.Chrome] = Chrome.ResolveDownloadUrl,
            [SupportedBrowser.ChromeHeadlessShell] = ChromeHeadlessShell.ResolveDownloadUrl,
            [SupportedBrowser.Chromium] = Chromium.ResolveDownloadUrl,
            [SupportedBrowser.Firefox] = Firefox.ResolveDownloadUrl,
        };

        private readonly CustomFileDownloadAction _customFileDownload;
        private readonly PuppeteerHttpClient _httpClient = new PuppeteerHttpClient();
        private bool _isDisposed;

        /// <inheritdoc cref="BrowserFetcher"/>
        public BrowserFetcher()
        {
            CacheDir = GetBrowsersLocation();
            Platform = GetCurrentPlatform();
            Browser = SupportedBrowser.Chrome;
            _customFileDownload = this._httpClient.DownloadFileAsync;
        }

        /// <inheritdoc cref="BrowserFetcher"/>
        public BrowserFetcher(SupportedBrowser browser) : this(new BrowserFetcherOptions { Browser = browser })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserFetcher"/> class.
        /// </summary>
        /// <param name="options"></param>
        public BrowserFetcher(BrowserFetcherOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.Browser = options.Browser;
            this.CacheDir = string.IsNullOrEmpty(options.Path) ? GetBrowsersLocation() : options.Path;
            this.Platform = options.Platform ?? GetCurrentPlatform();
            this._customFileDownload = options.CustomFileDownload ?? this._httpClient.DownloadFileAsync;
        }

        /// <inheritdoc/>
        public event DownloadProgressHandler DownloadProgressChanged;

        /// <inheritdoc/>
        public string CacheDir { get; set; }

        /// <inheritdoc/>
        public string BaseUrl { get; set; }

        /// <inheritdoc/>
        public Platform Platform { get; set; }

        /// <inheritdoc/>
        public SupportedBrowser Browser { get; set; }

        /// <inheritdoc/>
        public IWebProxy WebProxy
        {
            get
            {
                return this._httpClient.WebProxy;
            }

            set
            {
                if (this._httpClient != null)
                {
                    this._httpClient.WebProxy = value;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanDownloadAsync(string revision)
        {
            try
            {
                var url = GetDownloadURL(Browser, Platform, BaseUrl, revision);
                return await this._httpClient.CanDownloadAsync(url).ConfigureAwait(false);
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<InstalledBrowser> DownloadAsync()
        {
            var buildId = Browser == SupportedBrowser.Firefox
                ? await Firefox.GetDefaultBuildIdAsync().ConfigureAwait(false)
                : Chrome.DefaultBuildId;

            return await DownloadAsync(buildId).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<InstalledBrowser> DownloadAsync(BrowserTag tag)
        {
            var revision = await ResolveBuildIdAsync(tag).ConfigureAwait(false);
            return await DownloadAsync(revision).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public IEnumerable<InstalledBrowser> GetInstalledBrowsers()
            => new Cache(CacheDir).GetInstalledBrowsers();

        /// <inheritdoc/>
        public void Uninstall(string buildId)
            => new Cache(CacheDir).Uninstall(Browser, Platform, buildId);

        /// <inheritdoc/>
        public async Task<InstalledBrowser> DownloadAsync(string buildId)
        {
            var installedBrowser = await DownloadAsync(Browser, buildId).ConfigureAwait(false);

            if (Browser == SupportedBrowser.Chrome)
            {
                await DownloadAsync(SupportedBrowser.ChromeHeadlessShell, buildId).ConfigureAwait(false);
            }

            return installedBrowser;
        }

        /// <inheritdoc/>
        public string GetExecutablePath(string buildId)
            => new InstalledBrowser(
                new Cache(CacheDir),
                Browser,
                buildId,
                Platform).GetExecutablePath();

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static Platform GetCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.OSArchitecture == Architecture.Arm64 ? Platform.MacOSArm64 : Platform.MacOS;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Platform.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return RuntimeInformation.OSArchitecture == Architecture.X64 ? Platform.Win64 : Platform.Win32;
            }

            return Platform.Unknown;
        }

        public static string GetBrowsersLocation()
        {
            var assembly = typeof(Puppeteer).Assembly;
            var assemblyName = assembly.GetName().Name + ".dll";
            DirectoryInfo assemblyDirectory = new(AppContext.BaseDirectory);

            if (!assemblyDirectory.Exists || !File.Exists(Path.Combine(assemblyDirectory.FullName, assemblyName)))
            {
                var assemblyLocation = assembly.Location;

                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    var singleFilePublishFilePathForBrowserExecutables = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PublishSingleFileLocalApplicationDataFolderName);
                    if (!Directory.Exists(singleFilePublishFilePathForBrowserExecutables))
                    {
                        Directory.CreateDirectory(singleFilePublishFilePathForBrowserExecutables);
                    }

                    return singleFilePublishFilePathForBrowserExecutables;
                }

                assemblyDirectory = new FileInfo(assemblyLocation).Directory;
            }

            if (!assemblyDirectory.Exists || !File.Exists(Path.Combine(assemblyDirectory.FullName, assemblyName)))
            {
                assemblyDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            return assemblyDirectory.FullName;
        }

        /// <summary>
        /// Dispose <see cref="WebClient"/>.
        /// </summary>
        /// <param name="disposing">Indicates whether disposal was initiated by <see cref="Dispose()"/> operation.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        private static string GetDownloadURL(SupportedBrowser product, Platform platform, string baseUrl, string buildId)
            => _downloadsUrl[product](platform, buildId, baseUrl);

        private static void ExtractTar(string zipPath, string folderPath)
        {
            new DirectoryInfo(folderPath).Create();
            using var process = new Process();
            process.StartInfo.FileName = "tar";
            process.StartInfo.Arguments = $"-xvjf \"{zipPath}\" -C \"{folderPath}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
        }

        private async Task<InstalledBrowser> DownloadAsync(SupportedBrowser browser, string buildId)
        {
            var url = _downloadsUrl[browser](this.Platform, buildId, this.BaseUrl);
            var fileName = url.Split('/').Last();
            var cache = new Cache(this.CacheDir);
            var archivePath = Path.Combine(this.CacheDir, fileName);
            var downloadFolder = new DirectoryInfo(this.CacheDir);

            if (!downloadFolder.Exists)
            {
                downloadFolder.Create();
            }

            if (DownloadProgressChanged != null)
            {
                this._httpClient.DownloadProgressChanged += DownloadProgressChanged;
            }

            var outputPath = cache.GetInstallationDir(browser, Platform, buildId);

            if (new DirectoryInfo(outputPath).Exists)
            {
                return new InstalledBrowser(cache, browser, buildId, Platform);
            }

            try
            {
                await _customFileDownload(url, archivePath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new PuppeteerException($"Failed to download {browser} for {Platform} from {url}", ex);
            }

            await UnpackArchiveAsync(archivePath, outputPath, fileName).ConfigureAwait(false);
            new FileInfo(archivePath).Delete();

            return new InstalledBrowser(cache, browser, buildId, Platform);
        }

        private Task InstallDMGAsync(string dmgPath, string folderPath)
        {
            try
            {
                var destinationDirectoryInfo = new DirectoryInfo(folderPath);

                if (!destinationDirectoryInfo.Exists)
                {
                    destinationDirectoryInfo.Create();
                }

                var mountAndCopyTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                using var process = new Process();
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = "hdiutil";
                process.StartInfo.Arguments = $"attach -nobrowse -noautoopen \"{dmgPath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data == null || mountAndCopyTcs.Task.IsCompleted)
                    {
                        return;
                    }

                    var volumes = new Regex("\\/Volumes\\/(.*)").Match(e.Data);

                    if (!volumes.Success)
                    {
                        return;
                    }

                    var mountPath = volumes.Captures[0];
                    var appFile = new DirectoryInfo(mountPath.Value).GetDirectories("*.app").FirstOrDefault();
                    if (appFile == null)
                    {
                        mountAndCopyTcs.TrySetException(new PuppeteerException($"Cannot find app in {mountPath.Value}"));
                        return;
                    }

                    using var copyProcess = new Process();
                    copyProcess.StartInfo.FileName = "cp";
                    copyProcess.StartInfo.Arguments = $"-R \"{appFile.FullName}\" \"{folderPath}\"";
                    copyProcess.Start();
                    copyProcess.WaitForExit();
                    mountAndCopyTcs.TrySetResult(true);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();

                return mountAndCopyTcs.Task.WithTimeout(Puppeteer.DefaultTimeout);
            }
            finally
            {
                UnmountDmg(dmgPath);
            }
        }

        private void UnmountDmg(string dmgPath)
        {
            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "hdiutil";
                process.StartInfo.Arguments = $"detach \"{dmgPath}\" -quiet";
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();
            }
            catch
            {
                // swallow
            }
        }

        private Task<string> ResolveBuildIdAsync(BrowserTag tag)
        {
            switch (Browser)
            {
                case SupportedBrowser.Firefox:
                    return tag switch
                    {
                        BrowserTag.Latest => Firefox.ResolveBuildIdAsync("FIREFOX_NIGHTLY"),
                        _ => throw new PuppeteerException($"{tag} is not supported for {Browser}. Use 'latest' instead."),
                    };
                case SupportedBrowser.Chrome:
                    return tag switch
                    {
                        BrowserTag.Latest => Chrome.ResolveBuildIdAsync(ChromeReleaseChannel.Canary),
                        BrowserTag.Beta => Chrome.ResolveBuildIdAsync(ChromeReleaseChannel.Beta),
                        BrowserTag.Canary => Chrome.ResolveBuildIdAsync(ChromeReleaseChannel.Canary),
                        BrowserTag.Dev => Chrome.ResolveBuildIdAsync(ChromeReleaseChannel.Dev),
                        BrowserTag.Stable => Chrome.ResolveBuildIdAsync(ChromeReleaseChannel.Stable),
                        _ => throw new PuppeteerException($"{tag} is not supported for {Browser}."),
                    };
                case SupportedBrowser.Chromium:
                    return tag switch
                    {
                        BrowserTag.Latest => Chromium.ResolveBuildIdAsync(Platform),
                        _ => throw new PuppeteerException($"{tag} is not supported for {Browser}. Use 'latest' instead."),
                    };
                default:
                    throw new PuppeteerException($"{Browser} not supported.");
            }
        }

        private async Task UnpackArchiveAsync(string archivePath, string outputPath, string archiveName)
        {
            if (archivePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(archivePath, outputPath);
            }
            else if (archivePath.EndsWith(".tar.bz2", StringComparison.OrdinalIgnoreCase))
            {
                ExtractTar(archivePath, outputPath);
            }
            else
            {
                await InstallDMGAsync(archivePath, outputPath).ConfigureAwait(false);
            }

            if (GetCurrentPlatform() == Platform.Linux)
            {
                var executables = new[]
                {
                    "chrome",
                    "chrome_crashpad_handler",
                    "chrome-management-service",
                    "chrome_sandbox", // setuid
                    "crashpad_handler",
                    "google-chrome",
                    "libvulkan.so.1",
                    "nacl_helper",
                    "nacl_helper_bootstrap",
                    "xdg-mime",
                    "xdg-settings",
                    "cron/google-chrome",
                };

                foreach (var executable in executables)
                {
                    var execPath = Path.Combine(outputPath, archiveName, executable);

                    if (File.Exists(execPath))
                    {
                        var code = LinuxSysCall.Chmod(execPath, LinuxSysCall.ExecutableFilePermissions);

                        if (code != 0)
                        {
                            throw new Exception("Chmod operation failed");
                        }
                    }
                }
            }
        }
    }
}
