' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.NetCore.Analyzers.Runtime

Namespace Microsoft.NetCore.VisualBasic.Analyzers.Runtime
    <ExportCodeFixProvider(LanguageNames.VisualBasic), [Shared]>
    Public NotInheritable Class BasicUseVolatileClassInsteadOfThreadVolatileMethodsFixer
        Inherits UseVolatileClassInsteadOfThreadVolatileMethodsFixer

        Protected Overrides Function IsNamespaceNeeded(namespaces As IReadOnlyList(Of SyntaxNode), invocation As IInvocationOperation) As Boolean
            Dim invocationSyntax = DirectCast(invocation.Syntax, InvocationExpressionSyntax)
            If TypeOf invocationSyntax.Expression Is MemberAccessExpressionSyntax Then
                Return False
            End If

            Return (From import In namespaces.OfType(Of SimpleImportsClauseSyntax)()
                    Select IsSystemThreadingImport(import)).All(Function(isThreading) Not isThreading)
        End Function

        Private Shared Function IsSystemThreadingImport(import As SimpleImportsClauseSyntax) As Boolean
            Dim identifier = TryCast(import.Name, IdentifierNameSyntax)
            If identifier Is Nothing Then
                Return False
            End If

            Return identifier.Identifier.Text Is SystemThreadingNamespace
        End Function
    End Class
End Namespace