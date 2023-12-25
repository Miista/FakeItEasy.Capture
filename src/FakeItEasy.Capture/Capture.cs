using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy.Configuration;
using FakeItEasy.Core;

// ReSharper disable once CheckNamespace
namespace FakeItEasy
{
    /// <summary>
    /// Create a container to capture parameter values passed into a FakeItEasy fake.
    /// </summary>
    /// <example>
    /// var capture = new Capture&lt;int&gt;();
    /// A.CallTo(() => fake.SomeMethod(capture);
    /// fake.SomeMethod(42);
    /// int capturedValue = capture.Value;
    /// </example>
    /// <returns>A capture container that can be used in A.CallTo(...)</returns>
    public sealed class Capture<T>
    {
        private readonly List<T> _values = new List<T>();
        private bool _pendingConfiguration = true;

        /// <summary>
        /// The captured value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no values have been captured, or when more than one value was captured.</exception>
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

        /// <summary>
        /// The list captured values.
        /// </summary>
        public IReadOnlyList<T> Values => _values.AsReadOnly();

        /// <summary>
        /// Returns true if at least one value was captured.
        /// </summary>
        public bool HasValues => _values.Count > 0;

        public override string ToString()
        {
            if (_values.Count == 0) return "No captured values";
            if (_values.Count == 1) return Value.ToString();
            
            return $"{Values.Count} captured values";
        }

        /// <summary>
        /// Produces a value of type <typeparamref name="T"/> to configure a call. 
        /// </summary>
        /// <param name="capture">The captured value</param>
        /// <returns>An instance of <typeparamref name="T"/></returns>
        /// <exception cref="InvalidOperationException">Thrown if the container has been used to configure a call.</exception>
        public static implicit operator T(Capture<T> capture)
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
                    var stackTrace = new System.Diagnostics.StackTrace();
                    var firstOrDefault = stackTrace.GetFrames()?.Where(f => string.Equals("IsApplicableTo", f.GetMethod().Name) && string.Equals("BuildableCallRule", f.GetMethod().DeclaringType?.Name)).FirstOrDefault();
                    var stackFrame = stackTrace.GetFrames()[7];
                    var methodBase = stackFrame.GetMethod();
                    var methodBody = methodBase.GetMethodBody();
                    var localVariableInfo = methodBody.LocalVariables.FirstOrDefault();
                    var localType = localVariableInfo.LocalType;
                    var localTypeDeclaringType = localType.DeclaringType;
                    var propertyInfo = localTypeDeclaringType.GetProperties()[3];
                    var propertyInfoGetMethod = propertyInfo.GetMethod;
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
    }
    
    public static class Extensions
    {
        public static List<object> Rules { get; } = new List<object>();
        public static IReturnValueConfiguration<T> WithCapture<T>(this IReturnValueConfiguration<T> self)
        {
            var methodInfo = typeof(Extensions).GetMethod(nameof(CastObject));
            var invoke = methodInfo.MakeGenericMethod(self.GetType()).Invoke(null, new object[]{self});
            var propertyInfos = invoke.GetType().GetProperties();

            var x = methodInfo.MakeGenericMethod(self.GetType())
                .Invoke(null, new object[] { self })
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                .GetValue(self, null);
            var rule = x.GetType().GetProperties()[0].GetValue(x, null);
            Rules.Add(rule);
            return self;
        }
        
        public static IThenConfiguration<T> WithCapture<T>(this IThenConfiguration<T> self)
        {
            var methodInfo = typeof(Extensions).GetMethod(nameof(CastObject));
            var invoke = methodInfo.MakeGenericMethod(self.GetType()).Invoke(null, new object[]{self});
            var propertyInfos = invoke.GetType().GetProperties();

            var x = methodInfo.MakeGenericMethod(self.GetType())
                .Invoke(null, new object[] { self })
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                .GetValue(self, null);
            var rule = x.GetType().GetProperties()[0].GetValue(x, null);

            var fakeObjectCallRule = rule as IFakeObjectCallRule;

            Rules.Add(rule);
            return self;
        }

        private class PassThroughRule : IFakeObjectCallRule
        {
            private readonly IFakeObjectCallRule _underlyingRule;
            
            public PassThroughRule(IFakeObjectCallRule underlyingRule)
            {
                _underlyingRule = underlyingRule ?? throw new ArgumentNullException(nameof(underlyingRule));
            }

            public bool IsApplicableTo(IFakeObjectCall fakeObjectCall)
            {
                throw new NotImplementedException();
            }

            public void Apply(IInterceptedFakeObjectCall fakeObjectCall) => _underlyingRule.Apply(fakeObjectCall);

            public int? NumberOfTimesToCall => _underlyingRule.NumberOfTimesToCall;
        }
        public static T CastObject<T>(object input) {   
            return (T) input;   
        }
    }
}