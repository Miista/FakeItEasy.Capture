using System;

namespace FakeItEasy.Capture.Sandbox
{
    public interface Test
    {
        bool X(string str);
    }
    
    class Program
    {
        
        static void Main(string[] args)
        {
            var test = A.Fake<Test>();
            var stringCapture = new Capture<string>();
            A.CallTo(() => test.X(stringCapture)).Returns(true);

            var b = test.X("Hello, World");
            var b1 = test.X(", World");
            
            Console.WriteLine(b);
            foreach (var v in stringCapture.Values)
            {
                Console.WriteLine(v);
            }
        }
    }
}