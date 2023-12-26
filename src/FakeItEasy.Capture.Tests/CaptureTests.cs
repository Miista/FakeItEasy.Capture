using FluentAssertions;
using Xunit;

namespace FakeItEasy.Capture.Tests
{
    public class CaptureTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public interface IClock
        {
            Task Delay(int n);
        }

        [Fact]
        public async Task Commits_only_the_latest_value_for_complex_rules()
        {
            // Arrange
            var expectedCapturedValues = new[] { 1, 2, 3 };
            
            var clock = A.Fake<IClock>();

            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .Returns(Task.CompletedTask);
            
            // Act
            await clock.Delay(1);
            await clock.Delay(2);
            await clock.Delay(3);
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }
        
        [Fact]
        public async Task Commits_values_immediately()
        {
            // Arrange
            var expectedCapturedValues = new[] { 1, 2, 3 };
            
            var clock = A.Fake<IClock>();

            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .WithCapture(delayCapture)
                .Then
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .WithCapture(delayCapture)
                .Then
                .ReturnsLazily(_ => Task.CompletedTask)
                .WithCapture(delayCapture);
            
            // Act
            await clock.Delay(1);
            await clock.Delay(2);
            await clock.Delay(3);
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }
        
        [Fact]
        public async Task Captures_the_correct_values()
        {
            // Arrange
            var expectedCapturedValues = new[] { 1 };
            
            var clock = A.Fake<IClock>();

            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .WithCapture(delayCapture);
            
            // Act
            await clock.Delay(1);
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }
    }
}