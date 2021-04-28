using System;

namespace FakeItEasy.Capture.Sandbox
{
    public interface SomeDependency
    {
        void SomeMethod(string str);
        void SomeOtherMethod(string str);
        void SomeMethodWithTwoParameters(string str1, string str2);
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            {
                // Capturing a single argument
                var singleArgument = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();
            
                dependency.SomeMethod("I am captured!");
            
                Console.WriteLine(singleArgument.Value == "I am captured!");
            
                dependency.SomeMethod("I am captured!");
            }
            
            {
                // Capturing multiple arguments
                var multipleArguments = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethod(multipleArguments)).DoesNothing();
            
                dependency.SomeMethod("I am captured!");
                dependency.SomeMethod("I, too, am captured!");
                dependency.SomeMethod("I am also captured!");
            
                Console.WriteLine(multipleArguments.Values.Count == 3);
            }
            
            {
                // Capturing a configured call (with Capture)
                var singleArgument = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethod(singleArgument))
                    .WhenArgumentsMatch((string s) => s == "I am captured!")
                    .CapturesInto(singleArgument)
                    .DoesNothing();
            
                dependency.SomeMethod("I am captured!");
            
                Console.WriteLine(singleArgument.Value == "I am captured!");
            
                dependency.SomeMethod("I am captured!");
            }
            
            {
                // Capturing multiple arguments to a configured call (in one call)
                var arg1 = new Capture<string>();
                var arg2 = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethodWithTwoParameters(arg1, arg2)).DoesNothing();
            
                dependency.SomeMethodWithTwoParameters("I am captured!", "I am also captured!");
            
                Console.WriteLine(arg1.Value == "I am captured!");
                Console.WriteLine(arg2.Value == "I am also captured!");
            }
            
            {
                // Capturing a configured call (with A<string>._)
                var singleArgument = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethod(A<string>._))
                    .CapturesInto(singleArgument)
                    .DoesNothing();
            
                dependency.SomeMethod("I am captured!");
            
                Console.WriteLine(singleArgument.Value == "I am captured!");
            
                dependency.SomeMethod("I am captured!");
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