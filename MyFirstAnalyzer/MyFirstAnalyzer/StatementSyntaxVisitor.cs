using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyFirstAnalyzer
{
    public class StatementSyntaxVisitor : CSharpSyntaxVisitor<StatementSyntax>
    {
        private ExpressionSyntaxVisitor expressionSyntaxVisitor;

        public StatementSyntaxVisitor(ExpressionSyntaxVisitor expressionSyntaxVisitor)
        {
            this.expressionSyntaxVisitor = expressionSyntaxVisitor;
        }

        public override StatementSyntax VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return node.WithExpression(
                node.Expression.Accept(
                    expressionSyntaxVisitor));
        }

        public override StatementSyntax VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var firstVariable = node.Declaration.Variables.FirstOrDefault();
            if (firstVariable != null)
            {
                return node.WithDeclaration(
                    node.Declaration.WithVariables(
                        node.Declaration.Variables.Replace(
                            firstVariable,
                            firstVariable.WithInitializer(
                                firstVariable.Initializer.WithValue(
                                    firstVariable.Initializer.Value.Accept(expressionSyntaxVisitor))))));
            }

            return base.VisitLocalDeclarationStatement(node);
        }

        public override StatementSyntax VisitReturnStatement(ReturnStatementSyntax node)
        {
            return node.WithExpression(
                node.Expression.Accept(
                    expressionSyntaxVisitor));
        }
    }
}
