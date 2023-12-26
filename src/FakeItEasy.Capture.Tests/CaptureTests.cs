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
                .Returns(Task.CompletedTask);
            
            // Act
            await clock.Delay(1);
            delayCapture.Values.Should().HaveCount(1, because: "that is the number of call made");
            delayCapture.Values.Should().Contain(1, because: "that is the value passed to the call");
            
            await clock.Delay(2);
            delayCapture.Values.Should().HaveCount(2, because: "that is the number of call made");
            delayCapture.Values.Should().Contain(2, because: "that is the value passed to the call");
            
            await clock.Delay(3);
            delayCapture.Values.Should().HaveCount(3, because: "that is the number of call made");
            delayCapture.Values.Should().Contain(3, because: "that is the value passed to the call");
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }
        
        [Fact]
        public async Task Captures_values_as_pending_when_used_with_WithCapture()
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
        public async Task Has_simple_API()
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
        public async Task Captures_the_correct_values()
        {
            // Arrange
            var expectedCapturedValues = new[] { 1, 2 };
            
            var clock = A.Fake<IClock>();

            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .WithCapture(delayCapture)
                .Then
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .WithCapture(delayCapture);
            A.CallTo(() => clock.Delay(3))
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1);
            
            // Act
            await clock.Delay(1);
            await clock.Delay(2);
            await clock.Delay(3);
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }

        public class Concurrency
        {
            [Theory(Skip = "For now")]
            //[InlineData(1)]
            [InlineData(2)]
            //[InlineData(10)]
            public async Task Commits_values_immediately(int numberOfThreads)
            {
                // Arrange
                var expectedCapturedValues = Enumerable.Range(1, numberOfThreads).ToArray();
            
                var clock = A.Fake<IClock>();

                var delayCapture = new Capture<int>();
                A.CallTo(() => clock.Delay(delayCapture))
                    .Returns(Task.CompletedTask)
                    .NumberOfTimes(numberOfThreads)
                    .WithCapture(delayCapture);

                var barrier = new Barrier(1 + numberOfThreads);

                // Act
                var threads = new Task[numberOfThreads];

                for (var i = 0; i < numberOfThreads; i++)
                {
                    var n = 1 + i;

                    threads[i] = Task.Run(
                        async () =>
                        {
                            barrier.SignalAndWait();
                            await clock.Delay(n);
                        }
                    );
                }

                barrier.SignalAndWait();
            
                await Task.WhenAll(threads);
            
                // Assert
                delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
                delayCapture.Values.Should().Contain(expectedCapturedValues, because: "that is the arguments given");
            }
        }
    }
}