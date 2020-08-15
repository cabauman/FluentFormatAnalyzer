using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentFormatAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LineBreakEveryArgAndMemberAccessAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MyTriviaAnalyzer";
        internal static readonly LocalizableString Title = "MyTriviaAnalyzer Title";
        internal static readonly LocalizableString MessageFormat = "MyTriviaAnalyzer '{0}'";
        internal const string Category = "MyTriviaAnalyzer Category";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                AnalyzeMethodDeclaration,
                SyntaxKind.ExpressionStatement, SyntaxKind.LocalDeclarationStatement, SyntaxKind.ReturnStatement);
        }

        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var invocationExpressions = context.Node.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>();
           Helper(context, invocationExpressions, false);
        }

        private static void Helper(SyntaxNodeAnalysisContext context, IEnumerable<InvocationExpressionSyntax> invocationExpressions, bool isArgumentDescendant)
        {
            foreach (var invocationExpression in invocationExpressions)
            {
                var operatorTokenLeadingTrivia = default(SyntaxTriviaList);
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    operatorTokenLeadingTrivia = memberAccessExpression.OperatorToken.LeadingTrivia;
                }
                else if (invocationExpression.Expression is MemberBindingExpressionSyntax memberBindingExpression)
                {
                    operatorTokenLeadingTrivia = memberBindingExpression.OperatorToken.LeadingTrivia;
                }
                else
                {
                    continue;
                }    

                var invocationLeadingWhitespaceLength = context.Node.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                if (isArgumentDescendant)
                {
                    invocationLeadingWhitespaceLength = invocationExpression.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                }

                if (!operatorTokenLeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == invocationLeadingWhitespaceLength + 4))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                    continue;
                }

                var arguments = invocationExpression.ArgumentList.Arguments;
                foreach (var argument in arguments)
                {
                    var memberAcessLeadingWhitespaceLength = operatorTokenLeadingTrivia.First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                    if (!argument.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == memberAcessLeadingWhitespaceLength + 4))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                        return;
                    }

                    var lambda = argument.Expression as LambdaExpressionSyntax;
                    if (lambda is null)
                    {
                        continue;
                    }

                    var argumentLeadingWhitespaceLength = argument.GetLeadingTrivia().First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
                    if (lambda.Body is BlockSyntax block)
                    {
                        if (!block.OpenBraceToken.LeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength) ||
                            !block.CloseBraceToken.LeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                            return;
                        }
                    }
                    else if (!lambda.Body.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength + 4))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation()));
                        return;
                    }

                    var nestedInvocationExpressions = argument.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>();
                    Helper(context, nestedInvocationExpressions, true);
                }
            }
        }
    }
}
