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
        Task Delay(int n, int x = 0);
    }

    public static class ArgumentConstraintMatcherExtensions
    {
        public static IArgumentConstraintManager<T> IsCapturedTo<T>(
            this IArgumentConstraintManager<T> manager,
            TmpCapture<T> capture,
            Func<INegatableArgumentConstraintManager<T>, T> x
        )
        {
            return new CapturingArgumentConstraintMatcher<T>(manager, capture, x);
        }
        
        // public static T IsCapturedTo<T>(
        //     this IArgumentConstraintManager<T> manager,
        //     TmpCapture<T> capture
        // )
        // {
        //     return capture;
        // }
    }

    public class CapturingArgumentConstraintMatcher<T> : IArgumentConstraintManager<T>
    {
        private readonly IArgumentConstraintManager<T> _manager;
        private readonly TmpCapture<T> _capture;
        private readonly Func<INegatableArgumentConstraintManager<T>, T> _predicate;
    
        public CapturingArgumentConstraintMatcher(
            IArgumentConstraintManager<T> manager,
            TmpCapture<T> capture,
            Func<INegatableArgumentConstraintManager<T>, T> predicate
        )
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
        
        public T Matches(Func<T, bool> predicate, Action<IOutputWriter> descriptionWriter)
        {
            var predicateTarget = predicate.Target;
            var d = predicateTarget as dynamic;
            return _manager.Matches(predicate, descriptionWriter);
        }
    }
    
    public sealed class TmpCaptureWithThrowaway<T>
    {
        private readonly List<T> _values = new List<T>();
        private bool _pendingConfiguration = true;

        public T Value
        {
            get
            {
                if (_values.Count == 0)
                    throw new InvalidOperationException("No values have been captured.");

                if (_values.Count > 1)
                    throw new InvalidOperationException("Multiple values were captured. Use Values property instead.");

                return _values[0];
            }
        }

        public IReadOnlyList<T> Values => _values.AsReadOnly();

        public bool HasValues => _values.Count > 0;

        public override string ToString()
        {
            if (_values.Count == 0) return "No captured values";
            if (_values.Count == 1) return Value.ToString();
            
            return $"{Values.Count} captured values";
        }

        public static implicit operator T(TmpCaptureWithThrowaway<T> capture)
        {
            if (!capture._pendingConfiguration)
            {
                throw new InvalidOperationException("Capture can only be used to configure a single call." +
                " If you're trying to access the captured value, use the Value property instead of relying on an implicit conversion.");
            }

            // Some FakeItEasy trickery to get the parameter value
            A<T>.That.Matches(
                input =>
                {
                    capture.CaptureValue(input);
                    return true;
                }, "Captured parameter " + typeof(T).FullName);
            capture._pendingConfiguration = false;
            
            return default(T);
        }

        private void CaptureValue(T value)
        {
            _values.Add(value);
        }

        internal void CommitValues()
        {
            
        }
    }
    
        public sealed class TmpCapture<T>
    {
        private readonly List<T> _values = new List<T>();
        private bool _pendingConfiguration = true;

        internal Func<INegatableArgumentConstraintManager<T>, T> _predicate;

        public T Value
        {
            get
            {
                if (_values.Count == 0)
                    throw new InvalidOperationException("No values have been captured.");

                if (_values.Count > 1)
                    throw new InvalidOperationException("Multiple values were captured. Use Values property instead.");

                return _values[0];
            }
        }

        public IReadOnlyList<T> Values => _values.AsReadOnly();

        public bool HasValues => _values.Count > 0;

        public override string ToString()
        {
            if (_values.Count == 0) return "No captured values";
            if (_values.Count == 1) return Value.ToString();
            
            return $"{Values.Count} captured values";
        }

        public static implicit operator T(TmpCapture<T> capture)
        {
            if (!capture._pendingConfiguration)
            {
                throw new InvalidOperationException("Capture can only be used to configure a single call." +
                " If you're trying to access the captured value, use the Value property instead of relying on an implicit conversion.");
            }

            if (capture._predicate == null)
            {
                return A<T>.That.Matches(
                    input =>
                    {
                        capture.CaptureValue(input);
                        return true;
                    },
                    "Captured parameter " + typeof(T).FullName
                );
            }
            
            var x = capture._predicate(A<T>.That);
            capture.CaptureValue(x);
            return x;

            // Some FakeItEasy trickery to get the parameter value
            // A<T>.That.Matches(
            //     input =>
            //     {
            //         if (capture._predicate(input))
            //         {
            //             capture.CaptureValue(input);
            //             return true;
            //         }
            //
            //         return false;
            //     }, "Captured parameter " + typeof(T).FullName);
            // capture._pendingConfiguration = false;
            //
            // return default(T);
        }

        private void CaptureValue(T value)
        {
            _values.Add(value);
        }
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
                var delayCapture1 = new Capture<int>();
                
                // A.CallTo(() => clock.Delay(A<int>.That.IsCapturedTo(delayCapture).IsEqualTo(1)))
                //     .Returns(Task.CompletedTask).Once();
                // A.CallTo(() => clock.Delay(A<int>.That.IsCapturedTo(delayCapture).IsEqualTo(1)))
                //     .Returns(Task.FromException<Exception>(new Exception())).Once();
                // A.CallTo(() => clock.Delay(A<int>.That.Not.IsEqualTo(1)))
                //     .Returns(Task.FromException<Exception>(new Exception()));

                A.CallTo(() => clock.Delay(delayCapture, delayCapture1))
                    .Returns(Task.CompletedTask)
                    .NumberOfTimes(1)
                    .WithCapture(delayCapture, delayCapture1)
                    .Then
                    .Returns(Task.CompletedTask)
                    .NumberOfTimes(1)
                    .WithCapture(delayCapture, delayCapture1);
                
                // Func<INegatableArgumentConstraintManager<int>, int> f = that => that.IsEqualTo(1);
                // //A.CallTo(() => clock.Delay(A<int>.That.IsCapturedTo(delayCapture, f).IsEqualTo(1)))
                // A.CallTo(() => clock.Delay(delayCapture))
                //     .Returns(Task.CompletedTask).NumberOfTimes(2)
                //     .WithCapture()
                //     .Then
                //     .Returns(Task.CompletedTask).NumberOfTimes(1)
                //     .Then
                //     .ReturnsLazily(_ => Task.CompletedTask);
                
                // Måske vi kan finde den regel, som FakeItEasy forsøger at køre.
                // Så kan vi se, om den regel er applicable.
                // Hvis den ikke er, så "recorder" vi ikke argumentet.
                
                // Ovenstående var ikke muligt.
                // Dog har jeg formået at få fat i en instans af ExpressionCallRule.
                // Nu kan jeg i det mindste se, hvor mange gange en regel skal kaldes.
                // Så kan jeg måske selv holde styr på, hvor mange gange reglen er kaldt.
                await clock.Delay(1);
                //await clock.Delay(1);
                await clock.Delay(2, 10);
                //await clock.Delay(3);
                Console.WriteLine("Hey");
                
                /*
                 * Næste ide er, at vi kalder "WithCapture" efter reglen er defineret. Fx:
                 *  .Returns(Task.CompletedTask).NumberOfTimes(2)
                    .WithCapture()
                    
                   Det kræver dog, at vi kan finde frem til RuleBuilder, så vi kan attache os selv som en regel, der skal køres.
                   Når vi gør det, kan vi tilgå parametre, som metoden kaldes med.
                   Så kan vi holde styr på:
                   1. Hvor mange gange metoden er kaldt
                   2. Hvor mange gange vi er blevet evalueret, og dermed
                   3. Om vi skal "capture" argumentet
                 */
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