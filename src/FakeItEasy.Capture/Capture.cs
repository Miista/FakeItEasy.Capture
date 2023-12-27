using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy.Capture;

// ReSharper disable once CheckNamespace
namespace FakeItEasy
{
    public interface ICapture
    {
        void Commit();
        
        bool CommitImmediately { get; set; }
    }

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
    public sealed class Capture<T> : ICapture
    {
        private readonly List<T> _values = new List<T>();
        private readonly List<T> _valuesPendingCommit = new List<T>();
        
        private bool _pendingConfiguration = true;

        public bool CommitImmediately { get; set; } = true;
        
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

        public void Commit()
        {
            if (_valuesPendingCommit.Any())
            {
                _values.Add(_valuesPendingCommit.Last());
                _valuesPendingCommit.Clear();
            }
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
            Register.RegisterForCapture(capture);
            A<T>.That.Matches(
                input =>
                {
                    var methodInfo = new System.Diagnostics.StackTrace().GetFrames()[8].GetMethod().GetMethodBody().LocalVariables[0].GetType().GetMethods()[6];
//                    methodInfo.Invoke(null)
                    capture.CaptureValue(input);
                    return true;
                }, "Captured parameter " + typeof(T).FullName);
            capture._pendingConfiguration = false;

            return default(T);
        }

        private void CaptureValue(T value)
        {
            if (CommitImmediately)
            {
                _values.Add(value);
            }
            else
            {
                _valuesPendingCommit.Add(value);
            }
        }
    }
}