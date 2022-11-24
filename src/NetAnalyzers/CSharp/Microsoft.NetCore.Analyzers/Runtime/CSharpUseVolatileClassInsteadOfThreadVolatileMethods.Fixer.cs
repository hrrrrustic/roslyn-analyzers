// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.NetCore.Analyzers.Runtime;

namespace Microsoft.NetCore.CSharp.Analyzers.Runtime
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public sealed class CSharpUseVolatileClassInsteadOfThreadVolatileMethodsFixer : UseVolatileClassInsteadOfThreadVolatileMethodsFixer
    {
        protected override bool IsNamespaceNeeded(IReadOnlyList<SyntaxNode> namespaces, IInvocationOperation invocation)
        {
            var invocationSyntax = (InvocationExpressionSyntax)invocation.Syntax;
            if (invocationSyntax.Expression is MemberAccessExpressionSyntax)
                return false;

            return !namespaces.Any(k => k is UsingDirectiveSyntax usingSyntax && IsSystemThreadingUsing(usingSyntax));
        }

        private static bool IsSystemThreadingUsing(UsingDirectiveSyntax usingSyntax)
            => usingSyntax.Name is IdentifierNameSyntax { Identifier: { Text: SystemThreadingNamespace } };
    }
}