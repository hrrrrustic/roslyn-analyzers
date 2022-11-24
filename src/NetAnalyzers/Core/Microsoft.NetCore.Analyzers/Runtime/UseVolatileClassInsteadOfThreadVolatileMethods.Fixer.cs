// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.NetCore.Analyzers.Runtime
{
    public abstract class UseVolatileClassInsteadOfThreadVolatileMethodsFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create("SYSLIB0033");
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected const string SystemThreadingNamespace = "System.Threading";
        private static readonly string s_title = MicrosoftNetCoreAnalyzersResources.UseVolatileClassInseadOfThreadVolatileReadMethodsCodeFixTitle;
        protected abstract bool IsNamespaceNeeded(IReadOnlyList<SyntaxNode> namespaces, IInvocationOperation invocation);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var codeAction = CodeAction.Create(s_title, async ct => await RedirectToVolatileClassAsync(context, ct).ConfigureAwait(false), s_title);
            context.RegisterCodeFix(codeAction, context.Diagnostics);
            return Task.CompletedTask;
        }

        private async Task<Document> RedirectToVolatileClassAsync(CodeFixContext context, CancellationToken token)
        {
            Document doc = context.Document;
            SyntaxNode root = await doc.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
                return doc;

            var nodeForFix = root.FindNode(context.Span);
            var model = await doc.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (model is null)
                return doc;

            var nodeOperation = model.GetOperation(nodeForFix, context.CancellationToken);
            IInvocationOperation invocation = GetInvocationOperation(nodeOperation);

            var editor = await DocumentEditor.CreateAsync(doc, token).ConfigureAwait(false);
            var generator = editor.Generator;

            var volatileNode = generator.IdentifierName("Volatile");
            var newMethodName = invocation.TargetMethod.Name.Replace("Volatile", string.Empty);
            var access = generator.MemberAccessExpression(volatileNode, newMethodName);
            var newInvocation = generator.InvocationExpression(access, invocation.Arguments.Select(k => k.Syntax));

            var newRoot = generator.ReplaceNode(root, invocation.Syntax, newInvocation);
            if (IsNamespaceNeeded(generator.GetNamespaceImports(root), invocation))
                newRoot = generator.AddNamespaceImports(newRoot, generator.NamespaceImportDeclaration(nameof(System.Threading)));

            return doc.WithSyntaxRoot(newRoot);
        }

        private static IInvocationOperation GetInvocationOperation(IOperation nodeOperation)
        {
            //In C# calling obsoleted method will produce TextSpan that references to InvocationExpressionSyntax
            if (nodeOperation is IInvocationOperation invocation)
                return invocation;

            //But in Visual Basic it will reference to ExpressionStatementSyntax
            return (IInvocationOperation)((IExpressionStatementOperation)nodeOperation).Operation;
        }
    }
}