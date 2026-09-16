using System;
using Dotnet.Deps.Core;
using FluentAssertions;
using Xunit;

namespace Dotnet.Deps.Tests
{
    public class MinimumAgeTests
    {
        [Theory]
        [InlineData("2d", 48)]
        [InlineData("2D", 48)]
        [InlineData("12h", 12)]
        [InlineData("12H", 12)]
        [InlineData("2", 48)]
        [InlineData(" 2 d ", 48)]
        [InlineData("0", 0)]
        [InlineData("0.5d", 12)]
        public void ShouldParseMinimumAge(string value, double expectedHours)
        {
            MinimumAge.TryParse(value, out var minimumAge).Should().BeTrue();
            minimumAge.Should().Be(TimeSpan.FromHours(expectedHours));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("rubbish")]
        [InlineData("2w")]
        [InlineData("-2d")]
        [InlineData("d")]
        public void ShouldNotParseInvalidMinimumAge(string value)
        {
            MinimumAge.TryParse(value, out _).Should().BeFalse();
        }

        [Theory]
        [InlineData(48, "2 days")]
        [InlineData(24, "1 day")]
        [InlineData(12, "12 hours")]
        [InlineData(1, "1 hour")]
        [InlineData(36, "1.5 days")]
        public void ShouldFormatMinimumAge(double hours, string expected)
        {
            MinimumAge.Format(TimeSpan.FromHours(hours)).Should().Be(expected);
        }
    }
}
