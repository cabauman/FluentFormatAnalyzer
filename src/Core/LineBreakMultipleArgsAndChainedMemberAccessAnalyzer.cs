//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Threading;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace FluentFormatAnalyzer
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class LineBreakMultipleArgsAndChainedMemberAccessAnalyzer : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = "MyTriviaAnalyzer";
//        internal static readonly LocalizableString Title = "MyTriviaAnalyzer Title";
//        internal static readonly LocalizableString MessageFormat = "MyTriviaAnalyzer '{0}'";
//        internal const string Category = "MyTriviaAnalyzer Category";

//        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

//        public override void Initialize(AnalysisContext context)
//        {
//            context.EnableConcurrentExecution();
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            context.RegisterSyntaxNodeAction(
//                AnalyzeMethodDeclaration,
//                SyntaxKind.ExpressionStatement/*, SyntaxKind.LocalDeclarationStatement, SyntaxKind.ReturnStatement*/);
//        }

//        private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
//        {
//            var expressionStatement = (ExpressionStatementSyntax)context.Node;
//            //context.ReportDiagnostic(Diagnostic.Create(Rule, expressionStatement.GetLocation()));
//            //return;
//            var invocationExpressions = expressionStatement.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>();
//            Helper(context, invocationExpressions);
//        }

//        private static void Helper(SyntaxNodeAnalysisContext context, IEnumerable<InvocationExpressionSyntax> invocationExpressions)
//        {
//            var expressionStatement = (ExpressionStatementSyntax)context.Node;

//            foreach (var invocationExpression in invocationExpressions)
//            {
//                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
//                if (memberAccessExpression == null)
//                {
//                    continue;
//                }

//                var invocationLeadingWhitespaceLength = invocationExpression.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
//                if (!memberAccessExpression.OperatorToken.LeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == invocationLeadingWhitespaceLength + 4))
//                {
//                    context.ReportDiagnostic(Diagnostic.Create(Rule, expressionStatement.GetLocation()));
//                    continue;
//                }

//                var arguments = invocationExpression.ArgumentList.Arguments;
//                foreach (var argument in arguments)
//                {
//                    var memberAcessLeadingWhitespaceLength = memberAccessExpression.OperatorToken.LeadingTrivia.First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
//                    if (!argument.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == memberAcessLeadingWhitespaceLength + 4))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Rule, expressionStatement.GetLocation()));
//                        continue;
//                    }

//                    var lambda = argument.Expression as LambdaExpressionSyntax;
//                    if (lambda is null)
//                    {
//                        continue;
//                    }

//                    var argumentLeadingWhitespaceLength = argument.GetLeadingTrivia().First(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
//                    if (lambda.Body is BlockSyntax block)
//                    {
//                        if (!block.OpenBraceToken.LeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength) ||
//                            !block.CloseBraceToken.LeadingTrivia.Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength))
//                        {
//                            context.ReportDiagnostic(Diagnostic.Create(Rule, expressionStatement.GetLocation()));
//                            continue;
//                        }
//                    }
//                    else if (!lambda.Body.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.WhitespaceTrivia) && x.Span.Length == argumentLeadingWhitespaceLength + 4))
//                    {
//                        context.ReportDiagnostic(Diagnostic.Create(Rule, expressionStatement.GetLocation()));
//                        continue;
//                    }

//                    var nestedInvocationExpressions = argument.DescendantNodes(x => !x.IsKind(SyntaxKind.ArgumentList)).OfType<InvocationExpressionSyntax>();
//                    Helper(context, nestedInvocationExpressions);
//                }
//            }
//        }
//    }
//}
