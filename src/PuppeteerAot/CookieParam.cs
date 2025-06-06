using System.Text.Json.Serialization;

namespace Puppeteer
{
    /// <summary>
    /// Cookie data.
    /// </summary>
    /// <seealso cref="IPage.SetContentAsync(string, NavigationOptions)"/>
    /// <seealso cref="IPage.DeleteCookieAsync(CookieParam[])"/>
    /// <seealso cref="IPage.GetCookiesAsync(string[])"/>
    public class CookieParam
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>The URL.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets if it's secure.
        /// </summary>
        /// <value>Whether it's secure or not.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Secure { get; set; }

        /// <summary>
        /// Gets or sets if it's HTTP only.
        /// </summary>
        /// <value>Whether it's http only or not.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? HttpOnly { get; set; }

        /// <summary>
        /// Gets or sets the cookies SameSite value.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SameSite? SameSite { get; set; }

        /// <summary>
        /// Gets or sets the expiration. Unix time in seconds.
        /// </summary>
        /// <value>Expiration.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Expires { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Size { get; set; }

        /// <summary>
        /// Gets or sets if it's session only.
        /// </summary>
        /// <value>Whether it's session only or not.</value>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Session { get; set; }

        /// <summary>
        /// Cookie Priority. Supported only in Chrome.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CookiePriority? Priority { get; set; }

        /// <summary>
        /// True if cookie is SameParty. Supported only in Chrome.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? SameParty { get; set; }

        /// <summary>
        /// Cookie source scheme type. Supported only in Chrome.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CookieSourceScheme? SourceScheme { get; set; }

        /// <summary>
        /// Cookie partition key. The site of the top-level URL the browser was visiting at the
        /// start of the request to the endpoint that set the cookie. Supported only in Chrome.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string PartitionKey { get; set; }

        /// <summary>
        /// True if cookie partition key is opaque. Supported only in Chrome.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? PartitionKeyOpaque { get; set; }
    }
}
