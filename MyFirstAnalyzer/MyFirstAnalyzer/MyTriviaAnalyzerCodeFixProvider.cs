using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace MyFirstAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MyTriviaAnalyzerCodeFixProvider)), Shared]
    public class MyTriviaAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Format Expression";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MyTriviaAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var expressionStatement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ExpressionStatementSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => ApplyFormattingAsync(context.Document, expressionStatement, ct),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> ApplyFormattingAsync(Document document, ExpressionStatementSyntax expressionStatement, CancellationToken ct)
        {
            var invocationExpressions = expressionStatement.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>().ToList();
            var newNodes = Helper(invocationExpressions).ToList();
            var formattedExpressionStatement = expressionStatement;
            for (int i = 0; i < invocationExpressions.Count; ++i)
            {
                formattedExpressionStatement = formattedExpressionStatement.ReplaceNode(invocationExpressions[i], newNodes[i]);
            }
            

            var oldRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(expressionStatement, formattedExpressionStatement);

            return document.WithSyntaxRoot(newRoot);
        }

        private static IEnumerable<InvocationExpressionSyntax> Helper(IEnumerable<InvocationExpressionSyntax> invocationExpressions)
        {
            var newInvocationExpressions = new List<InvocationExpressionSyntax>();
            foreach (var invocationExpression in invocationExpressions)
            {
                var newInvocationExpression = invocationExpression;

                var memberAccessExpression = newInvocationExpression.Expression as MemberAccessExpressionSyntax;
                if (memberAccessExpression == null)
                {
                    continue;
                }

                if (!memberAccessExpression.Expression.GetTrailingTrivia().Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                {
                    memberAccessExpression = memberAccessExpression.WithExpression(memberAccessExpression.Expression.WithTrailingTrivia(new[] { SyntaxFactory.EndOfLine("\n") }));
                    newInvocationExpression = newInvocationExpression.WithExpression(memberAccessExpression);
                }

                var invocationLeadingWhitespace = newInvocationExpression.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
                var invocationLeadingWhitespaceLength = invocationLeadingWhitespace.Span.Length;
                var oldWhitespaceTrivia = memberAccessExpression.OperatorToken.LeadingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
                if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.Span.Length != invocationLeadingWhitespaceLength + 4)
                {
                    var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', invocationLeadingWhitespaceLength + 4));
                    SyntaxTriviaList leadingTrivia;
                    if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.IsKind(SyntaxKind.None))
                    {
                        leadingTrivia = memberAccessExpression.OperatorToken.LeadingTrivia.Add(newWhitespaceTrivia);
                    }
                    else
                    {
                        leadingTrivia = memberAccessExpression.OperatorToken.LeadingTrivia.Replace(oldWhitespaceTrivia, newWhitespaceTrivia);
                    }
                    
                    memberAccessExpression = memberAccessExpression.WithOperatorToken(memberAccessExpression.OperatorToken.WithLeadingTrivia(leadingTrivia));
                    newInvocationExpression = newInvocationExpression.WithExpression(memberAccessExpression);
                }

                //var arguments = invocationExpression.ArgumentList.Arguments;
                //foreach (var argument in arguments)
                //{
                //    var memberAcessLeadingWhitespaceLength = memberAccessExpression.OperatorToken.LeadingTrivia.First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                //    if (!argument.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == memberAcessLeadingWhitespaceLength + 4))
                //    {
                //        continue;
                //    }

                //    var lambda = argument.Expression as LambdaExpressionSyntax;
                //    if (lambda is null)
                //    {
                //        continue;
                //    }

                //    var argumentLeadingWhitespaceLength = argument.GetLeadingTrivia().First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                //    if (!lambda.Body.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength + 4))
                //    {
                //        continue;
                //    }

                //    var nestedInvocationExpressions = argument.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>();
                //    Helper(nestedInvocationExpressions);
                //}

                newInvocationExpressions.Add(newInvocationExpression);
            }

            return newInvocationExpressions;
        }
    }
}
