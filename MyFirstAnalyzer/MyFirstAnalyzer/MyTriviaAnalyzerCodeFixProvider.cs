using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var visitor = new StatementSyntaxVisitor(new ExpressionSyntaxVisitor());
            var formattedExpressionStatement = expressionStatement.Accept(visitor);

            var oldRoot = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            var newRoot = oldRoot.ReplaceNode(expressionStatement, formattedExpressionStatement);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
