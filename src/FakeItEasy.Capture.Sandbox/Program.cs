using System;

namespace FakeItEasy.Capture.Sandbox
{
    public interface SomeDependency
    {
        void SomeMethod(string str);
        void SomeOtherMethod(string str);
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
                // Configuring multiple call using the same Capture
                var singleArgument = new Capture<string>();
                var dependency = A.Fake<SomeDependency>();
                A.CallTo(() => dependency.SomeMethod(singleArgument)).DoesNothing();
                A.CallTo(() => dependency.SomeOtherMethod(singleArgument)).DoesNothing(); // This fails
            }
        }
    }
}