using System;
using System.Drawing;
using System.Numerics;
using GpuParticleSandbox;
using Xunit;

namespace GpuParticleSandbox.Tests
{
    public class ColorModeMapperTests
    {
        [Fact]
        public void Map_WithValidInputs_ReturnsExpectedColor()
        {
            // Arrange
            var mapper = ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Velocity)
                .Build();

            float velocityMagnitude = 0.5f;
            float age = 0.2f;

            // Act
            Vector3 result = mapper.Map(velocityMagnitude, age);

            // Assert
            Assert.Equal(0.5f, result.X);
            Assert.Equal(0f, result.Y);
            Assert.Equal(0.5f, result.Z);
        }

        [Fact]
        public void Builder_WithInvalidEnumValue_ThrowsArgumentException()
        {
            // Arrange
            var invalidMode = (ColorMode)999;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.CreateBuilder()
                .WithMode(invalidMode)
                .Build());
            Assert.Contains("Unsupported color mode", exception.Message);
        }

        [Fact]
        public void Map_WithVelocityMagnitudeOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mapper = ColorModeMapper.CreateBuilder().Build();
            float invalidVelocity = 1.5f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => mapper.Map(invalidVelocity, 0.2f));
            Assert.Contains("Velocity magnitude must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithNegativeVelocityMagnitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mapper = ColorModeMapper.CreateBuilder().Build();
            float invalidVelocity = -0.1f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => mapper.Map(invalidVelocity, 0.2f));
            Assert.Contains("Velocity magnitude must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithAgeOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mapper = ColorModeMapper.CreateBuilder().Build();
            float invalidAge = 1.5f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => mapper.Map(0.5f, invalidAge));
            Assert.Contains("Age must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithNegativeAge_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var mapper = ColorModeMapper.CreateBuilder().Build();
            float invalidAge = -0.1f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => mapper.Map(0.5f, invalidAge));
            Assert.Contains("Age must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Builder_WithInvalidGradientStops_ThrowsArgumentException()
        {
            // Arrange
            var stops = new (float T, Color Color)[] { (0.5f, Color.Red) };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Velocity)
                .WithGradientStops(stops)
                .Build());
            Assert.Contains("at least two entries", exception.Message);
        }

        [Fact]
        public void Builder_WithGradientStopOutOfRange_ThrowsArgumentException()
        {
            // Arrange
            var stops = new (float T, Color Color)[] { (-0.1f, Color.Red), (1.1f, Color.Blue) };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Velocity)
                .WithGradientStops(stops)
                .Build());
            Assert.Contains("range [0, 1]", exception.Message);
        }

        [Fact]
        public void Builder_WithUniformModeAndMissingFallback_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Uniform)
                .Build());
            Assert.Contains("Fallback color must be specified", exception.Message);
        }

        [Fact]
        public void Builder_WithCustomPalette_ReturnsInterpolatedColor()
        {
            // Arrange
            var customMapper = ColorModeMapper.CreateBuilder()
                .WithMode(ColorMode.Velocity)
                .WithPalette(Color.Blue, Color.Red)
                .Build();

            // Act
            Vector3 result = customMapper.Map(0.5f, 0.0f);

            // Assert (0.5 should be exactly in the middle: Green)
            Assert.Equal(0f, result.X);
            Assert.Equal(0.5f, result.Y);
            Assert.Equal(0.5f, result.Z);
        }
    }
}
