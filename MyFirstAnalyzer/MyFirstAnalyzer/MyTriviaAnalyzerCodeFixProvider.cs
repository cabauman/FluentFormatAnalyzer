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
            var invocationExpression = expressionStatement.Expression as InvocationExpressionSyntax;
            var formattedExpressionStatement = expressionStatement.WithExpression(InvocationExpressionHelper(invocationExpression));

            var oldRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(expressionStatement, formattedExpressionStatement);

            return document.WithSyntaxRoot(newRoot);
        }

        private static InvocationExpressionSyntax InvocationExpressionHelper(InvocationExpressionSyntax invocationExpression)
        {
            var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;

            var invocationLeadingWhitespace = invocationExpression.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            var indentWidth = invocationLeadingWhitespace.Span.Length;
            var memberAccessIndentWidth = indentWidth + 4;

            return invocationExpression
                .WithExpression(MemberAccessExpressionHelper(memberAccessExpression, indentWidth))
                .WithArgumentList(ArgumentListHelper(invocationExpression.ArgumentList, memberAccessIndentWidth));
        }

        private static MemberAccessExpressionSyntax MemberAccessExpressionHelper(MemberAccessExpressionSyntax memberAccessExpression, int parentIndentWidth)
        {
            var invocationExpression = memberAccessExpression.Expression as InvocationExpressionSyntax;
            if (invocationExpression != null)
            {
                memberAccessExpression = memberAccessExpression.WithExpression(InvocationExpressionHelper(invocationExpression));
            }

            if (memberAccessExpression == null)
            {
                throw new Exception("No member access expression contained in the invocation expression.");
            }

            if (!memberAccessExpression.Expression.GetTrailingTrivia().Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                memberAccessExpression = memberAccessExpression.WithExpression(
                    memberAccessExpression.Expression.WithTrailingTrivia(
                        new[] { SyntaxFactory.EndOfLine("\n") }));
            }

            var oldWhitespaceTrivia = memberAccessExpression.OperatorToken.LeadingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.Span.Length != parentIndentWidth + 4)
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth + 4));
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
            }

            return memberAccessExpression;
        }

        private static ArgumentListSyntax ArgumentListHelper(ArgumentListSyntax argumentList, int parentIndexWidth)
        {
            var oldArgumentTokens = argumentList.ChildTokens().Where(x => !x.IsKind(SyntaxKind.CloseParenToken));
            if (oldArgumentTokens.All(x => x.TrailingTrivia.SingleOrDefault(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)) != default))
            {
                argumentList = argumentList.ReplaceTokens(
                    oldArgumentTokens,
                    (original, _) => original.WithTrailingTrivia(new[] { SyntaxFactory.EndOfLine("\n") }));
            }

            var arguments = argumentList.Arguments;
            var newArguments = arguments;
            foreach (var argument in arguments)
            {
                ArgumentSyntax newArgument = argument;

                var oldWhitespaceTrivia = newArgument.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
                if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.Span.Length != parentIndexWidth + 4)
                {
                    var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndexWidth + 4));
                    SyntaxTriviaList leadingTrivia;
                    if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.IsKind(SyntaxKind.None))
                    {
                        leadingTrivia = newArgument.GetLeadingTrivia().Add(newWhitespaceTrivia);
                    }
                    else
                    {
                        leadingTrivia = newArgument.GetLeadingTrivia().Replace(oldWhitespaceTrivia, newWhitespaceTrivia);
                    }

                    newArgument = newArgument.WithLeadingTrivia(leadingTrivia);
                }

                var lambda = newArgument.Expression as LambdaExpressionSyntax;
                if (lambda != null)
                {
                    if (!lambda.ArrowToken.TrailingTrivia.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                    {
                        lambda = lambda.WithArrowToken(lambda.ArrowToken.WithTrailingTrivia(new[] { SyntaxFactory.EndOfLine("\n") }));
                        newArgument = newArgument.WithExpression(lambda);
                    }

                    var argumentLeadingWhitespaceLength = newArgument.GetLeadingTrivia().First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                    oldWhitespaceTrivia = lambda.Body.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
                    if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.Span.Length != argumentLeadingWhitespaceLength + 4)
                    {
                        var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', argumentLeadingWhitespaceLength + 4));
                        SyntaxTriviaList leadingTrivia;
                        if (oldWhitespaceTrivia == null || oldWhitespaceTrivia.IsKind(SyntaxKind.None))
                        {
                            leadingTrivia = lambda.Body.GetLeadingTrivia().Add(newWhitespaceTrivia);
                        }
                        else
                        {
                            leadingTrivia = lambda.Body.GetLeadingTrivia().Replace(oldWhitespaceTrivia, newWhitespaceTrivia);
                        }

                        lambda = lambda.WithBody(lambda.Body.WithLeadingTrivia(leadingTrivia));
                        newArgument = newArgument.WithExpression(lambda);
                    }
                }

                var argumentInvocationExpression = newArgument.Expression as InvocationExpressionSyntax;
                if (argumentInvocationExpression != null)
                {
                    argumentInvocationExpression = InvocationExpressionHelper(argumentInvocationExpression);
                    newArgument = newArgument.WithExpression(argumentInvocationExpression);
                }

                newArguments = newArguments.Replace(argument, newArgument);
            }

            return argumentList.WithArguments(newArguments);
        }
    }
}
