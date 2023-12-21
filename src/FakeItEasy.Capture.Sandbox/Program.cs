using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy.Configuration;
using FakeItEasy.Core;

namespace FakeItEasy.Capture.Sandbox
{
    public interface SomeDependency
    {
        void SomeMethod(string str);
        void SomeOtherMethod(string str);
    }

    public interface IClock
    {
        Task Delay(int n);
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // {
            //     // Capturing a single argument
            //     var singleArgument = new Capture<string>();
            //     var dependency = A.Fake<SomeDependency>();
            //     A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();
            //
            //     dependency.SomeMethod("I am captured!");
            //
            //     Console.WriteLine(singleArgument.Value == "I am captured!");
            //
            //     dependency.SomeMethod("I am captured!");
            // }

            // {
            //     // Capturing multiple arguments
            //     var multipleArguments = new Capture<string>();
            //     var dependency = A.Fake<SomeDependency>();
            //     A.CallTo(() => dependency.SomeMethod(multipleArguments)).DoesNothing();
            //
            //     dependency.SomeMethod("I am captured!");
            //     dependency.SomeMethod("I, too, am captured!");
            //     dependency.SomeMethod("I am also captured!");
            //
            //     Console.WriteLine(multipleArguments.Values.Count == 3);
            // }

            {
                var clock = A.Fake<IClock>();
                var delayCapture = new Capture<int>();
                
                A.CallTo(() => clock.Delay(delayCapture))
                    .WithCapture()
                    .Returns(Task.CompletedTask).NumberOfTimes(1)
                    .Then
                    .WithCapture()
                    .Returns(Task.CompletedTask).NumberOfTimes(1)
                    .Then
                    .WithCapture()
                    .ReturnsLazily(_ => Task.CompletedTask);
                
                // Måske vi kan finde den regel, som FakeItEasy forsøger at køre.
                // Så kan vi se, om den regel er applicable.
                // Hvis den ikke er, så "recorder" vi ikke argumentet.
                
                // Ovenstående var ikke muligt.
                // Dog har jeg formået at få fat i en instans af ExpressionCallRule.
                // Nu kan jeg i det mindste se, hvor mange gange en regel skal kaldes.
                // Så kan jeg måske selv holde styr på, hvor mange gange reglen er kaldt.
                await clock.Delay(1);
                await clock.Delay(2);
                //await clock.Delay(3);
                Console.WriteLine("Hey");
            }
            
            // {
            //     // Configuring multiple call using the same Capture
            //     var singleArgument = new Capture<string>();
            //     var dependency = A.Fake<SomeDependency>();
            //     A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();
            //     A.CallTo(() => dependency.SomeOtherMethod(singleArgument)).DoesNothing(); // This fails
            // }
        }
    }
}