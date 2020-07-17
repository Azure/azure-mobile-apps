using Azure.Mobile.Server.Entity;
using System;
using System.Linq;

namespace Azure.Mobile.Common.Test
{
    public class Movie : EntityTableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }

        public Movie()
        {
            // Fills in the EntityTableData fields.
            Id = Guid.NewGuid().ToString("N");
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-(180 + (new Random()).Next(180)));
            Version = Guid.NewGuid().ToByteArray();
            Deleted = false;
        }

        /// <summary>
        /// Returns a copy of the current movie (so that the reference in
        /// EF core is broken).
        /// </summary>
        /// <returns></returns>
        public Movie Clone()
        {
            return new Movie()
            {
                Id = this.Id,
                UpdatedAt = this.UpdatedAt,
                Version = this.Version,
                Deleted = this.Deleted,
                Title = this.Title,
                Duration = this.Duration,
                MpaaRating = this.MpaaRating,
                ReleaseDate = this.ReleaseDate,
                BestPictureWinner = this.BestPictureWinner,
                Year = this.Year
            };
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            var c = obj as Movie;
            return c.Id == Id
                && c.UpdatedAt == UpdatedAt
                && c.Deleted == Deleted
                && c.Title == Title
                && c.Duration == Duration
                && c.MpaaRating == MpaaRating
                && c.ReleaseDate == ReleaseDate
                && c.BestPictureWinner == BestPictureWinner
                && c.Year == Year
                && c.Version.SequenceEqual(Version);
        }
    }
}
