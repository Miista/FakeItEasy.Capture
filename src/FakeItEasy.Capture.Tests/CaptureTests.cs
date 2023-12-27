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
            Task DelayEvenMore(string n);
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
        public async Task Does_not_fail_on_unused_captures()
        {
            // Arrange
            var clock = A.Fake<IClock>();

            var evenMoreCapture = new Capture<string>();
            A.CallTo(() => clock.DelayEvenMore(evenMoreCapture))
                .Returns(Task.CompletedTask);
            
            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .WithCapture()
                .Returns(Task.CompletedTask);
            
            // Act
            await clock.DelayEvenMore("Hello, World!");
            await clock.Delay(1);
            
            // Assert
            delayCapture.Value.Should().Be(1, because: "that is the input given to the call");

            evenMoreCapture.HasValues.Should().BeFalse(because: "there have been no calls to that method");
        }
        
        [Fact]
        public async Task Does_not_block_other_captures()
        {
            // Arrange
            var clock = A.Fake<IClock>();

            // We *NEED* to register the type+method that is invoked.
            // That way we can ensure which captures we must commit.
            var evenMoreCapture = new Capture<string>();
            A.CallTo(() => clock.DelayEvenMore(evenMoreCapture))
                .Returns(Task.CompletedTask);
            
            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .WithCapture()
                .Returns(Task.CompletedTask);
            
            // Act
            await clock.DelayEvenMore("Hello, World!");
            
            // Assert
            evenMoreCapture.HasValues.Should().BeTrue(because: "a value has been captured");
            evenMoreCapture.Value.Should().Be("Hello, World!", because: "that is the input given to the call");

            delayCapture.HasValues.Should().BeFalse(because: "no calls have been made to the method");
        }
        
        [Fact]
        public async Task Captures_values_as_pending_when_used_with_WithCapture()
        {
            // Arrange
            var expectedCapturedValues = new[] { 1, 2, 3 };
            
            var clock = A.Fake<IClock>();

            var delayCapture = new Capture<int>();
            A.CallTo(() => clock.Delay(delayCapture))
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .Then
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .Then
                .WithCapture()
                .ReturnsLazily(_ => Task.CompletedTask);
            
            // Act
            await clock.Delay(1);
            await clock.Delay(2);
            await clock.Delay(3);
            
            // Assert
            delayCapture.Values.Should().HaveSameCount(expectedCapturedValues, because: "that is the number of calls made");
            delayCapture.Values.Should().ContainInOrder(expectedCapturedValues, because: "that is the arguments given");
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public interface ISecondClock
        {
            Task Invoke(string s);
        }
        
        [Fact]
        public async Task Supports_Invokes()
        {
            // Arrange
            var capture1 = new Capture<int>();
            var capture2 = new Capture<string>();

            var secondClock = A.Fake<ISecondClock>();
            A.CallTo(() => secondClock.Invoke(capture2))
                .Returns(Task.CompletedTask);
            var clock = A.Fake<IClock>();
            A.CallTo(() => clock.Delay(capture1))
                .Invokes(() => secondClock.Invoke("Hello, World!"))
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .Then
                .Invokes(() => secondClock.Invoke("Wee"))
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .Then
                .Invokes(() => secondClock.Invoke("Third!"))
                .WithCapture()
                .Returns(Task.CompletedTask);

            // Act
            await clock.Delay(1);
            await clock.Delay(2);
            await clock.Delay(3);
            
            // Assert
            capture1.Values.Should().HaveCount(3, because: "that is the number of captures");
            capture1.Values.Should().Contain(new[] { 1, 2, 3 }, because: "those are the captured values");
            
            capture2.Values.Should().HaveCount(3, because: "that is the number of captures");
            capture2.Values.Should().Contain(new[] { "Hello, World!", "Wee", "Third!" }, because: "those are the captured values");
            
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
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1)
                .Then
                .WithCapture()
                .Returns(Task.CompletedTask)
                .NumberOfTimes(1);
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