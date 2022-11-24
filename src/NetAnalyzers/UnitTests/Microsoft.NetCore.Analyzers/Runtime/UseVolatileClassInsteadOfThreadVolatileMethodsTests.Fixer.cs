// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
    Microsoft.NetCore.CSharp.Analyzers.Runtime.CSharpUseVolatileClassInsteadOfThreadVolatileMethodsFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeAnalysis.Testing.EmptyDiagnosticAnalyzer,
    Microsoft.NetCore.VisualBasic.Analyzers.Runtime.BasicUseVolatileClassInsteadOfThreadVolatileMethodsFixer>;

namespace Microsoft.CodeAnalysis.NetAnalyzers.UnitTests.Microsoft.NetCore.Analyzers.Runtime
{
    public class UseVolatileClassInsteadOfThreadVolatileMethodsFixerTests
    {
        [Fact(Skip = "How to test only fixer without analyzer?")]
        public async Task FixSimpleCall_CSharp()
        {
            var source = @"
using System;

class C
{
    public void TestMethod()
    {
        int value = 4;
        var result = Thread.VolatileRead(ref value);
    }
}";

            var fixedSource = @"
using System;

class C
{
    public void TestMethod()
    {
        int value = 4;
        var result = Volatile.Read(ref value);
    }
}";
            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact(Skip = "How to test only fixer without analyzer?")]
        public async Task FixSimpleCall_VisualBasic()
        {
            var source = @"
Imports System.Threading

Public Class C
    Public Sub Test()
        Dim location As Integer = 5
        Thread.VolatileRead(location)
    End Sub
End Class";

            var fixedSource = @"
Imports System.Threading

Public Class C
    Public Sub Test()
        Dim location As Integer = 5
        Volatile.Read(location)
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact(Skip = "How to test only fixer without analyzer?")]
        public async Task FixUsingStaticCall_CSharp()
        {
            var source = @"
using static System.Threading.Thread;

class C
{
    public void TestMethod()
    {
        int value = 4;
        var result = VolatileRead(ref value);
    }
}";

            var fixedSource = @"
using static System.Threading.Thread;
using System.Threading;

class C
{
    public void TestMethod()
    {
        int value = 4;
        var result = Volatile.Read(ref value);
    }
}";
            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact(Skip = "How to test only fixer without analyzer?")]
        public async Task FixImportedThreadClassCall_VisualBasic()
        {
            var source = @"
Imports System.Threading.Thread

Public Class C
    Public Sub Test()
        Dim location As Integer = 5
        VolatileRead(location)
    End Sub
End Class";

            var fixedSource = @"
Imports System.Threading.Thread
Imports System.Threading

Public Class C
    Public Sub Test()
        Dim location As Integer = 5
        Volatile.Read(location)
    End Sub
End Class";
            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }
    }
}