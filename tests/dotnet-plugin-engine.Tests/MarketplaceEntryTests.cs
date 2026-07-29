using System;
using System.Collections.Generic;
using Xunit;
using PluginEngine.Marketplace;

namespace PluginEngine.Tests
{
    public class MarketplaceEntryTests
    {
        [Fact]
        public void Constructor_InitializesWithDefaultValues()
        {
            var entry = new MarketplaceEntry();

            Assert.Equal(Guid.Empty, entry.Id);
            Assert.Equal(string.Empty, entry.Name);
            Assert.Equal(string.Empty, entry.LatestVersion);
            Assert.Equal(string.Empty, entry.Author);
            Assert.Equal(string.Empty, entry.Description);
            Assert.Empty(entry.Tags);
            Assert.Equal(0L, entry.Downloads);
            Assert.Equal(0.0, entry.Rating);
            Assert.Equal(DateTime.MinValue, entry.PublishedAtUtc);
            Assert.Equal(DateTime.MinValue, entry.UpdatedAtUtc);
        }

        [Fact]
        public void ObjectInitializer_SetsAllPropertiesCorrectly()
        {
            var id = Guid.NewGuid();
            var name = "Test Plugin";
            var version = "1.2.3";
            var author = "Author Name";
            var description = "A test plugin";
            var tags = new List<string> { "tag1", "tag2" };
            var downloads = 100L;
            var rating = 4.5;
            var published = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var updated = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc);

            var entry = new MarketplaceEntry
            {
                Id = id,
                Name = name,
                LatestVersion = version,
                Author = author,
                Description = description,
                Tags = tags,
                Downloads = downloads,
                Rating = rating,
                PublishedAtUtc = published,
                UpdatedAtUtc = updated
            };

            Assert.Equal(id, entry.Id);
            Assert.Equal(name, entry.Name);
            Assert.Equal(version, entry.LatestVersion);
            Assert.Equal(author, entry.Author);
            Assert.Equal(description, entry.Description);
            Assert.Equal(tags, entry.Tags);
            Assert.Equal(downloads, entry.Downloads);
            Assert.Equal(rating, entry.Rating);
            Assert.Equal(published, entry.PublishedAtUtc);
            Assert.Equal(updated, entry.UpdatedAtUtc);
        }

        [Fact]
        public void StringProperties_CanBeSetToNull()
        {
            var entry = new MarketplaceEntry
            {
                Name = null,
                LatestVersion = null,
                Author = null,
                Description = null
            };

            Assert.Null(entry.Name);
            Assert.Null(entry.LatestVersion);
            Assert.Null(entry.Author);
            Assert.Null(entry.Description);
        }

        [Fact]
        public void TagsProperty_CanBeSetToNullAndEmptyList()
        {
            var entry = new MarketplaceEntry();

            // Setting null
            entry.Tags = null;
            Assert.Null(entry.Tags);

            // Setting empty list
            entry.Tags = new List<string>();
            Assert.Empty(entry.Tags);

            // Setting list with items
            var tags = new List<string> { "a", "b" };
            entry.Tags = tags;
            Assert.Same(tags, entry.Tags);
        }

        [Fact]
        public void DownloadsProperty_AcceptsNegativeAndLargeValues()
        {
            var entry = new MarketplaceEntry();

            entry.Downloads = 0;
            Assert.Equal(0L, entry.Downloads);

            entry.Downloads = -1;
            Assert.Equal(-1L, entry.Downloads);

            entry.Downloads = long.MaxValue;
            Assert.Equal(long.MaxValue, entry.Downloads);

            entry.Downloads = long.MinValue;
            Assert.Equal(long.MinValue, entry.Downloads);
        }

        [Fact]
        public void RatingProperty_AcceptsRangeOfValues()
        {
            var entry = new MarketplaceEntry();

            entry.Rating = 0.0;
            Assert.Equal(0.0, entry.Rating);

            entry.Rating = -1.0;
            Assert.Equal(-1.0, entry.Rating);

            entry.Rating = 5.0;
            Assert.Equal(5.0, entry.Rating);

            entry.Rating = double.MaxValue;
            Assert.Equal(double.MaxValue, entry.Rating);

            entry.Rating = double.MinValue;
            Assert.Equal(double.MinValue, entry.Rating);
        }

        [Fact]
        public void DateTimeProperties_AcceptMinAndMaxValues()
        {
            var entry = new MarketplaceEntry();

            entry.PublishedAtUtc = DateTime.MinValue;
            entry.UpdatedAtUtc = DateTime.MaxValue;

            Assert.Equal(DateTime.MinValue, entry.PublishedAtUtc);
            Assert.Equal(DateTime.MaxValue, entry.UpdatedAtUtc);

            // Setting to specific values
            var date = new DateTime(2023, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            entry.PublishedAtUtc = date;
            entry.UpdatedAtUtc = date;

            Assert.Equal(date, entry.PublishedAtUtc);
            Assert.Equal(date, entry.UpdatedAtUtc);
        }
    }
}