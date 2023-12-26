using System.Collections.Generic;

namespace FakeItEasy.Capture
{
    public static class Register
    {
        public static List<ICapture> _capturesForRegister = new List<ICapture>();

        public static List<Extensions.ActionHandler> _handlers = new List<Extensions.ActionHandler>();

        public static List<object> _rules = new List<object>();
        
        public static void AddRule(object o) => _rules.Add(o);
        
        public static void AddHandler(Extensions.ActionHandler handler) => _handlers.Add(handler);
        public static void RegisterForCapture(ICapture capture)
        {
            _capturesForRegister.Add(capture);
        }

        public static void ClearRegister()
        {
            
        }
    }
}