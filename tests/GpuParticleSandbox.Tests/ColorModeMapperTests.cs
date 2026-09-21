using System;
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
            float velocityMagnitude = 0.5f;
            float age = 0.2f;
            ColorMode mode = ColorMode.Velocity;

            // Act
            Vector3 result = ColorModeMapper.Map(velocityMagnitude, age, mode);

            // Assert
            Assert.Equal(0.5f, result.X);
            Assert.Equal(0f, result.Y);
            Assert.Equal(0.5f, result.Z);
        }

        [Fact]
        public void Map_WithInvalidEnumValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var invalidMode = (ColorMode)999;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.Map(0.5f, 0.2f, invalidMode));
            Assert.Contains("Unsupported color mode", exception.Message);
        }

        [Fact]
        public void Map_WithVelocityMagnitudeOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            float invalidVelocity = 1.5f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.Map(invalidVelocity, 0.2f, ColorMode.Velocity));
            Assert.Contains("Velocity magnitude must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithNegativeVelocityMagnitude_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            float invalidVelocity = -0.1f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.Map(invalidVelocity, 0.2f, ColorMode.Velocity));
            Assert.Contains("Velocity magnitude must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithAgeOutOfRange_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            float invalidAge = 1.5f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.Map(0.5f, invalidAge, ColorMode.Age));
            Assert.Contains("Age must be in the range [0, 1]", exception.Message);
        }

        [Fact]
        public void Map_WithNegativeAge_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            float invalidAge = -0.1f;

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => ColorModeMapper.Map(0.5f, invalidAge, ColorMode.Age));
            Assert.Contains("Age must be in the range [0, 1]", exception.Message);
        }
    }
}
