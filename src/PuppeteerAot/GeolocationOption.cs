using System;

namespace Puppeteer
{
    /// <summary>
    /// Geolocation option.
    /// </summary>
    /// <seealso cref="IPage.SetGeolocationAsync(GeolocationOption)"/>
    public class GeolocationOption : IEquatable<GeolocationOption>
    {
        /// <summary>
        /// Latitude between -90 and 90.
        /// </summary>
        /// <value>The latitude.</value>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Longitude between -180 and 180.
        /// </summary>
        /// <value>The longitude.</value>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Optional non-negative accuracy value.
        /// </summary>
        /// <value>The accuracy.</value>
        public decimal Accuracy { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="Puppeteer.GeolocationOption"/> is equal to the current <see cref="T:Puppeteer.GeolocationOption"/>.
        /// </summary>
        /// <param name="other">The <see cref="Puppeteer.GeolocationOption"/> to compare with the current <see cref="T:Puppeteer.GeolocationOption"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Puppeteer.GeolocationOption"/> is equal to the current
        /// <see cref="T:Puppeteer.GeolocationOption"/>; otherwise, <c>false</c>.</returns>
        public bool Equals(GeolocationOption other)
            => other != null &&
                Latitude == other.Latitude &&
                Longitude == other.Longitude &&
                Accuracy == other.Accuracy;

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as GeolocationOption);

        /// <inheritdoc/>
        public override int GetHashCode()
            => (Latitude.GetHashCode() ^ 2014) +
                (Longitude.GetHashCode() ^ 2014) +
                (Accuracy.GetHashCode() ^ 2014);
    }
}
