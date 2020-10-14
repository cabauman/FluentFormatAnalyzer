using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentFormatAnalyzer
{
    public class ExpressionSyntaxVisitor : CSharpSyntaxVisitor<ExpressionSyntax>
    {
        public override ExpressionSyntax Visit(SyntaxNode node)
        {
            return base.Visit(node);
        }

        public override ExpressionSyntax VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            return node
                .WithExpression(Visit(node.Expression))
                .WithArgumentList(UpdateArgumentList(node.ArgumentList, node.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)));
        }

        public ArgumentListSyntax UpdateArgumentList(ArgumentListSyntax node, bool isMemberAccess)
        {
            var newArguments = new SeparatedSyntaxList<ArgumentSyntax>();

            var parentIndentWidth = node.Parent.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
            var argumentIndentWidth = isMemberAccess ? parentIndentWidth + 8 : parentIndentWidth + 4;

            foreach (var argument in node.Arguments)
            {

                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', argumentIndentWidth));
                var newArgument = argument
                    .WithLeadingTrivia(newWhitespaceTrivia);

                var newExpression = Visit(newArgument.Expression);
                if (newExpression != null)
                {
                    newArgument = newArgument
                        .WithExpression(newExpression);
                }

                newArguments = newArguments.Add(newArgument);
            }

            node = node.WithArguments(newArguments);

            if (newArguments.Count > 0)
            {
                var oldArgumentTokens = node.ChildTokens().Where(x => !x.IsKind(SyntaxKind.CloseParenToken));
                if (!oldArgumentTokens.All(x => x.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))))
                {
                    node = node.ReplaceTokens(
                        oldArgumentTokens,
                        (original, _) => original.WithTrailingTrivia(new[] { SyntaxFactory.EndOfLine("\n") }));
                }
            }

            return node;
        }

        public override ExpressionSyntax VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            node = node.WithExpression(Visit(node.Expression));

            var parentIndentWidth = node
                .GetLeadingTrivia()
                .FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia))
                .Span
                .Length;

            if (!node.Expression.GetTrailingTrivia().Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                node = node.WithExpression(
                    node.Expression.WithTrailingTrivia(
                        new[] { SyntaxFactory.EndOfLine("\n") }));
            }

            var oldWhitespaceTrivia = node.OperatorToken.LeadingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            if (oldWhitespaceTrivia.Span.Length != parentIndentWidth + 4)
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth + 4));
                node = node
                    .WithOperatorToken(
                        node.OperatorToken.WithLeadingTrivia(newWhitespaceTrivia));
            }

            return node;
        }

        public override ExpressionSyntax VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            return node
                .WithExpression(
                    Visit(node.Expression))
                .WithWhenNotNull(
                    Visit(node.WhenNotNull));

        }

        public override ExpressionSyntax VisitMemberBindingExpression(MemberBindingExpressionSyntax node)
        {
            return node;
        }

        public override ExpressionSyntax VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            return VisitLambdaExpression(node);
        }

        public override ExpressionSyntax VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            return VisitLambdaExpression(node);
        }

        public override ExpressionSyntax VisitIdentifierName(IdentifierNameSyntax node)
        {
            return node;
        }

        public CSharpSyntaxNode UpdateBlock(BlockSyntax node)
        {
            var parentIndentWidth = node.Parent.GetLeadingTrivia().Span.Length;
            var openBrace = node.OpenBraceToken;
            var closeBrace = node.CloseBraceToken;

            var oldWhitespaceTrivia = node.OpenBraceToken.LeadingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            if (oldWhitespaceTrivia.Span.Length != parentIndentWidth)
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth));
                openBrace = node.OpenBraceToken.WithLeadingTrivia(newWhitespaceTrivia);
            }

            openBrace = openBrace
                .WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));

            oldWhitespaceTrivia = node.CloseBraceToken.LeadingTrivia.FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia));
            if (oldWhitespaceTrivia.Span.Length != parentIndentWidth)
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth));
                closeBrace = closeBrace
                    .WithLeadingTrivia(
                        SyntaxFactory.EndOfLine("\n"),
                        newWhitespaceTrivia);
            }

            var newStatements = new SyntaxList<StatementSyntax>();
            foreach (var statement in node.Statements)
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth + 4));
                var newStatement = statement
                    .WithLeadingTrivia(SyntaxFactory.EndOfLine("\n"), newWhitespaceTrivia)
                    .WithTrailingTrivia(SyntaxFactory.EndOfLine("\n"));
                newStatements = newStatements.Add(newStatement);
            }

            node = node
                .WithStatements(newStatements)
                .WithOpenBraceToken(openBrace)
                .WithCloseBraceToken(closeBrace);

            return node;
        }

        //public override ExpressionSyntax VisitEqualsValueClause(EqualsValueClauseSyntax node)
        //{
        //    return base.VisitEqualsValueClause(node) ?? node;
        //}

        public override ExpressionSyntax VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            return node
                .WithLeft(
                    Visit(node.Left))
                .WithRight(
                    Visit(node.Right));
        }

        public override ExpressionSyntax VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return node;
        }

        public override ExpressionSyntax VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            return node
                .WithCondition(
                    Visit(node.Condition))
                .WithWhenTrue(
                    Visit(node.WhenTrue))
                .WithWhenFalse(
                    Visit(node.WhenFalse));
        }

        private ExpressionSyntax VisitLambdaExpression(LambdaExpressionSyntax node)
        {
            var parentIndentWidth = node.Parent.GetLeadingTrivia().FirstOrDefault(x => x.IsKind(SyntaxKind.WhitespaceTrivia)).Span.Length;
            if (!node.ArrowToken.TrailingTrivia.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                node = node.WithArrowToken(node.ArrowToken.WithTrailingTrivia(new[] { SyntaxFactory.EndOfLine("\n") }));
            }

            if (node.Body is BlockSyntax block)
            {
                node = node.WithBody(UpdateBlock(block));
            }
            else
            {
                var newWhitespaceTrivia = SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, new string(' ', parentIndentWidth + 4));
                node = node.WithBody(node.Body.WithLeadingTrivia(newWhitespaceTrivia));

                var body = Visit(node.Body);
                if (body != null)
                {
                    node = node.WithBody(body);
                }
            }

            return node;
        }
    }
}
