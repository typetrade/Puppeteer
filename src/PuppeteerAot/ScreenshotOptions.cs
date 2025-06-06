using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Puppeteer.Media;

namespace Puppeteer
{
    /// <summary>
    /// Options to be used in <see cref="IPage.ScreenshotAsync(string, ScreenshotOptions)"/>, <see cref="IPage.ScreenshotStreamAsync(ScreenshotOptions)"/> and <see cref="IPage.ScreenshotDataAsync(ScreenshotOptions)"/>.
    /// </summary>
    public class ScreenshotOptions
    {
        private static readonly Dictionary<string, ScreenshotType?> _extensionScreenshotTypeMap = new()
        {
            ["jpe"] = ScreenshotType.Jpeg,
            ["jpeg"] = ScreenshotType.Jpeg,
            ["jpg"] = ScreenshotType.Jpeg,
            ["png"] = ScreenshotType.Png,
        };

        /// <summary>
        /// Specifies clipping region of the page.
        /// </summary>
        /// <value>The clip.</value>
        public Clip? Clip { get; set; }

        /// <summary>
        /// When <c>true</c>, takes a screenshot of the full scrollable page. Defaults to <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if full page; otherwise, <c>false</c>.</value>
        public bool FullPage { get; set; }

        /// <summary>
        /// Hides default white background and allows capturing screenshots with transparency. Defaults to <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if omit background; otherwise, <c>false</c>.</value>
        public bool OmitBackground { get; set; }

        /// <summary>
        /// Specify screenshot type, can be either jpeg, png or webp. Defaults to 'png'.
        /// </summary>
        /// <value>The type.</value>
        public ScreenshotType? Type { get; set; }

        /// <summary>
        /// The quality of the image, between 0-100. Not applicable to png images.
        /// </summary>
        /// <value>The quality.</value>
        public int? Quality { get; set; }

        /// <summary>
        /// When BurstMode is <c>true</c> the screenshot process will only execute all the screenshot setup actions (background and metrics overrides)
        /// before the first screenshot call and it will ignore the reset actions after the screenshot is taken.
        /// <see cref="IPage.SetBurstModeOffAsync"/> needs to be called after the last screenshot is taken.
        /// </summary>
        /// <example><![CDATA[
        /// var screenShotOptions = new ScreenshotOptions
        /// {
        ///     FullPage = true,
        ///     BurstMode = true
        /// };
        /// await page.GoToAsync("https://www.google.com");
        /// for(var x = 0; x < 100; x++)
        /// {
        ///     await page.ScreenshotBase64Async(screenShotOptions);
        /// }
        /// await page.SetBurstModeOffAsync();
        /// ]]></example>
        [JsonIgnore]
        public bool BurstMode { get; set; } = false;

        /// <summary>
        /// Capture the screenshot beyond the viewport.
        /// </summary>
        public bool CaptureBeyondViewport { get; set; } = true;

        /// <summary>
        /// Capture the screenshot from the surface, rather than the view. Defaults to <c>true</c>.
        /// </summary>
        public bool? FromSurface { get; set; }

        /// <summary>
        /// Optimize image encoding for speed.
        /// </summary>
        public bool? OptimizeForSpeed { get; set; }

        public static ScreenshotType? GetScreenshotTypeFromFile(string file)
        {
            var extension = new FileInfo(file).Extension.Replace(".", string.Empty);
            _extensionScreenshotTypeMap.TryGetValue(extension, out var result);
            return result;
        }
    }
}
