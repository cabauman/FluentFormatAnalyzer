using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.CodeFixVerifier<
    FluentFormatAnalyzer.LineBreakEveryArgAndMemberAccessAnalyzer,
    FluentFormatAnalyzer.MyFirstAnalyzerCodeFixProvider>;

namespace FluentFormatAnalyzer.Tests
{
    [TestClass]
    public class FluentFormatAnalyzerTests
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            Console.WriteLine(""Hello, world."");
        }
    }";

            var fixtest = test;

            var expected = Verify.Diagnostic("MyTriviaAnalyzer").WithLocation(11, 15).WithArguments("TypeName");
            await Verify.VerifyCodeFixAsync(test, expected, fixtest);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = Verify.Diagnostic("MyFirstAnalyzer").WithLocation(11, 15).WithArguments("TypeName");
            await Verify.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
