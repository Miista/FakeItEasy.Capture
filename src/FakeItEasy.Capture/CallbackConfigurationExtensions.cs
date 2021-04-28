using System;
using FakeItEasy.Configuration;

// ReSharper disable once CheckNamespace
namespace FakeItEasy
{
    public static class CallbackConfigurationExtensions
    {
        public static TInterface CapturesInto<TInterface, TMember>(
            this ICallbackConfiguration<TInterface> configuration,
            Capture<TMember> capture)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (capture == null) throw new ArgumentNullException(nameof(capture));
            
            return configuration.Invokes((TMember member) => capture.CaptureValue(member));
        }
    }
}