using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FakeItEasy.Configuration;
using FakeItEasy.Core;

namespace FakeItEasy.Capture
{
    public static class Extensions
    {
        // ReSharper disable once UnusedMember.Global
        public static IReturnValueConfiguration<T> WithCapture<T>(
            this IReturnValueConfiguration<T> self,
            ICapture capture,
            params ICapture[] captures
        )
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (capture == null) throw new ArgumentNullException(nameof(capture));
            
            return AddAction(self, capture, captures);
        }

        // ReSharper disable once UnusedMember.Global
        public static IAfterCallConfiguredConfiguration<T> WithCapture<T>(
            this IAfterCallConfiguredConfiguration<T> self,
            ICapture capture,
            params ICapture[] captures
        )
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (capture == null) throw new ArgumentNullException(nameof(capture));
            
            return AddAction(self, capture, captures);
        }

        // ReSharper disable once UnusedMember.Global
        // public static IRepeatConfiguration<T> WithCapture<T>(
        //     this FakeItEasy.Configuration.IRepeatConfiguration<T> self,
        //     ICapture capture,
        //     params ICapture[] captures
        // )
        // {
        //     if (self == null) throw new ArgumentNullException(nameof(self));
        //     if (capture == null) throw new ArgumentNullException(nameof(capture));
        //     
        //     return AddAction(self, capture, captures);
        // }
        
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static IAfterCallConfiguredWithOutAndRefParametersConfiguration<IReturnValueConfiguration<T>> WithCapture<T>(
            this IAfterCallConfiguredWithOutAndRefParametersConfiguration<IReturnValueConfiguration<T>> self,
            ICapture capture,
            params ICapture[] captures
        )
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (capture == null) throw new ArgumentNullException(nameof(capture));
            
            return AddAction(self, capture, captures);
        }

        private static T AddAction<T>(T self, ICapture capture, params ICapture[] captures)
        {
            var methodInfo = typeof(Extensions).GetMethod(nameof(CastObject));
            //var invoke = methodInfo.MakeGenericMethod(self.GetType()).Invoke(null, new object[]{self});

            var x = methodInfo.MakeGenericMethod(self.GetType())
                .Invoke(null, new object[] { self })
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                .GetValue(self, null);
            var rule = x.GetType().GetProperties()[0].GetValue(x, null);

            var actionsProperty = rule.GetType().GetProperties().FirstOrDefault(p => p.Name == "Actions");
            var collection = actionsProperty?.GetValue(rule) as ICollection<Action<IFakeObjectCall>> ?? new List<Action<IFakeObjectCall>>();

            var allCaptures = new[]{capture}.Concat(captures).ToArray();
            
            // Add action
            var actionHandler = new ActionHandler(allCaptures);
            Register.AddHandler(actionHandler);
            Register.AddRule(rule);
            collection.Add(actionHandler.Handle);
            
            foreach (var c in allCaptures)
            {
                c.CommitImmediately = false;
            }
            
            return self;

        }
        
        public static IThenConfiguration<T> WithCapture<T>(this IThenConfiguration<T> self, ICapture capture, params ICapture[] captures)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));
            if (capture == null) throw new ArgumentNullException(nameof(capture));
            
            return AddAction(self, capture, captures);
        }

        public class ActionHandler
        {
            private readonly IEnumerable<ICapture> _captures;

            public ActionHandler(IEnumerable<ICapture> captures)
            {
                _captures = captures ?? throw new ArgumentNullException(nameof(captures));
            }

            public void Handle(IFakeObjectCall call)
            {
                foreach (var capture in _captures)
                {
                    capture.Commit();
                }
            }
        }
        
        public static T CastObject<T>(object input) {   
            return (T) input;   
        }
    }
}