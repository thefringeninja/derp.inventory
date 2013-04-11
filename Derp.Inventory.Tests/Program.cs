using System;
using System.Linq;
using Simple.Testing.Framework;

namespace Derp.Inventory.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var results = SimpleRunner
                .RunFromGenerator(new Generator(typeof (Program).Assembly))
                .ToArray();
            results.ForEach(Print);
            Environment.ExitCode = results.Where(SpecificationFailed).Count();
        }

        private static void Print(RunResult runResult)
        {
            SpecificationPrinter.Print(runResult, Console.Out);
        }

        private static bool SpecificationFailed(RunResult result)
        {
            return false == result.Passed;
        }
    }
}