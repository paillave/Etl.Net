using System;
using System.Linq;
using Paillave.Etl;

namespace SubProcessQuickStart
{
    class Program
    {
        public class SetupSubProcess
        {

        }
        public class ListedMain
        {
            public int Val { get; set; }
        }
        static void Main(string[] args)
        {
            var main = StreamProcessDefinition.Create<SetupSubProcess>(setupS =>
            {
                setupS.CrossApplyAction<SetupSubProcess, ListedMain>("new rows", (setup, push) => { Enumerable.Range(0, 10).Select(i => new ListedMain { Val = i }).ToList().ForEach(push); });
            });
            var sub = StreamProcessDefinition.Create<SetupSubProcess>(setupS =>
            {
            });

            Console.WriteLine("Hello World!");
        }
    }
}
