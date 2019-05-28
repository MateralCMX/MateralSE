namespace VRage.Scripting.Rewriters
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Scripting.Analyzers;
    using VRage.Scripting.CompilerMethods;

    internal class PerfCountingRewriter : CSharpSyntaxRewriter
    {
        private const string COMPILER_METHODS_POSTFIX = "";
        private const string COMPILER_METHOD_EXIT_METHOD = "ExitMethod";
        private const string COMPILER_METHOD_ENTER_METHOD = "EnterMethod";
        private const string COMPILER_METHOD_YIELD_GUARD_METHOD = "YieldGuard";
        private const string COMPILER_METHOD_REENTER_YIELD_METHOD = "ReenterYieldMethod";
        private readonly int m_modID;
        private readonly SyntaxTree m_syntaxTree;
        private readonly SemanticModel m_semanticModel;
        private readonly CSharpCompilation m_compilation;

        protected PerfCountingRewriter(CSharpCompilation compilation, SyntaxTree syntaxTree, int modId) : base(false)
        {
            this.m_modID = modId;
            this.m_syntaxTree = syntaxTree;
            this.m_compilation = compilation;
            this.m_semanticModel = compilation.GetSemanticModel(syntaxTree, false);
        }

        private static T Annotated<T>(T node) where T: SyntaxNode
        {
            SyntaxAnnotation[] annotations = new SyntaxAnnotation[] { WhitelistDiagnosticAnalyzer.INJECTED_ANNOTATION };
            return node.WithAdditionalAnnotations<T>(annotations);
        }

        private static NameSyntax AnnotatedIdentifier(string identifierName)
        {
            int length = identifierName.LastIndexOf('.');
            return ((length < 0) ? ((NameSyntax) Annotated<IdentifierNameSyntax>(SyntaxFactory.IdentifierName(identifierName))) : ((NameSyntax) Annotated<QualifiedNameSyntax>(SyntaxFactory.QualifiedName(AnnotatedIdentifier(identifierName.Substring(0, length)), Annotated<IdentifierNameSyntax>(SyntaxFactory.IdentifierName(identifierName.Substring(length + 1)))))));
        }

        private AccessorListSyntax ArrowToGetter(ArrowExpressionClauseSyntax expression) => 
            SyntaxFactory.AccessorList(SyntaxFactory.SingletonList<AccessorDeclarationSyntax>(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, this.ProcessMethod(expression.Expression, false))));

        private StatementSyntax EnterMethodCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(ModPerfCounter).FullName), (SimpleNameSyntax) AnnotatedIdentifier("EnterMethod"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(this.MakeModIdArg()))));

        private StatementSyntax ExitMethodCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(ModPerfCounter).FullName), (SimpleNameSyntax) AnnotatedIdentifier("ExitMethod"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(this.MakeModIdArg()))));

        private BlockSyntax ExpressionBodyToBlock(ExpressionSyntax expression, bool isVoid)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            StatementSyntax syntax = isVoid ? ((StatementSyntax) SyntaxFactory.ExpressionStatement(expression)) : ((StatementSyntax) SyntaxFactory.ReturnStatement(expression));
            StatementSyntax[] statements = new StatementSyntax[] { syntax };
            return SyntaxFactory.Block(statements);
        }

        private BlockSyntax InjectMethodBody(BlockSyntax block)
        {
            StatementSyntax[] statements = new StatementSyntax[] { this.EnterMethodCall(), block };
            StatementSyntax[] syntaxArray2 = new StatementSyntax[] { this.ExitMethodCall() };
            StatementSyntax[] syntaxArray3 = new StatementSyntax[] { SyntaxFactory.TryStatement(SyntaxFactory.Block(statements), SyntaxFactory.List<CatchClauseSyntax>(), SyntaxFactory.FinallyClause(SyntaxFactory.Block(syntaxArray2))) };
            return SyntaxFactory.Block(syntaxArray3);
        }

        private ArgumentSyntax MakeModIdArg() => 
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(this.m_modID)));

        private BlockSyntax ProcessAnonymousFunction(AnonymousFunctionExpressionSyntax node, INamedTypeSymbol type)
        {
            BlockSyntax body = node.Body as BlockSyntax;
            if (body != null)
            {
                return this.ProcessMethod(body);
            }
            if ((type == null) || (type.DelegateInvokeMethod == null))
            {
                return null;
            }
            return this.ProcessMethod(node.Body as ExpressionSyntax, type.DelegateInvokeMethod.ReturnsVoid);
        }

        private BlockSyntax ProcessMethod(BlockSyntax body) => 
            ((body != null) ? this.InjectMethodBody(body) : null);

        private BlockSyntax ProcessMethod(ArrowExpressionClauseSyntax expression, bool isVoid) => 
            this.ProcessMethod(expression.Expression, isVoid);

        private BlockSyntax ProcessMethod(ExpressionSyntax body, bool isVoid) => 
            this.ProcessMethod(this.ExpressionBodyToBlock(body, isVoid));

        private StatementSyntax ReenterYieldMethodCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(ModPerfCounter).FullName), (SimpleNameSyntax) AnnotatedIdentifier("ReenterYieldMethod"))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(this.MakeModIdArg()))));

        public static SyntaxTree Rewrite(CSharpCompilation compilation, SyntaxTree syntaxTree, int modId)
        {
            CancellationToken token = new CancellationToken();
            SyntaxNode root = new PerfCountingRewriter(compilation, syntaxTree, modId).Visit(syntaxTree.GetRoot(token));
            return syntaxTree.WithRootAndOptions(root, syntaxTree.Options);
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration(node);
            return node.WithBody(this.ProcessMethod(node.Body));
        }

        public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (AnonymousMethodExpressionSyntax) base.VisitAnonymousMethodExpression(node);
            BlockSyntax body = this.ProcessAnonymousFunction(node, convertedType);
            return ((body != null) ? node.WithBody(body) : node);
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            node = (ConstructorDeclarationSyntax) base.VisitConstructorDeclaration(node);
            return node.WithBody(this.InjectMethodBody(node.Body));
        }

        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            node = (DestructorDeclarationSyntax) base.VisitDestructorDeclaration(node);
            return node.WithBody(this.InjectMethodBody(node.Body));
        }

        public override SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            node = (IndexerDeclarationSyntax) base.VisitIndexerDeclaration(node);
            return ((node.ExpressionBody == null) ? node : node.WithExpressionBody(null).WithAccessorList(this.ArrowToGetter(node.ExpressionBody)));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            node = (MethodDeclarationSyntax) base.VisitMethodDeclaration(node);
            if (node.ExpressionBody == null)
            {
                return node.WithBody(this.ProcessMethod(node.Body));
            }
            PredefinedTypeSyntax returnType = node.ReturnType as PredefinedTypeSyntax;
            bool isVoid = (returnType != null) && returnType.Keyword.IsKind(SyntaxKind.VoidKeyword);
            return node.WithExpressionBody(null).WithBody(this.ProcessMethod(node.ExpressionBody, isVoid));
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            node = (OperatorDeclarationSyntax) base.VisitOperatorDeclaration(node);
            return ((node.ExpressionBody == null) ? node.WithBody(this.InjectMethodBody(node.Body)) : node.WithExpressionBody(null).WithBody(this.ProcessMethod(node.ExpressionBody, false)));
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (ParenthesizedLambdaExpressionSyntax) base.VisitParenthesizedLambdaExpression(node);
            BlockSyntax body = this.ProcessAnonymousFunction(node, convertedType);
            return ((body != null) ? node.WithBody(body) : node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            node = (PropertyDeclarationSyntax) base.VisitPropertyDeclaration(node);
            return ((node.ExpressionBody == null) ? node : node.WithExpressionBody(null).WithAccessorList(this.ArrowToGetter(node.ExpressionBody)));
        }

        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (SimpleLambdaExpressionSyntax) base.VisitSimpleLambdaExpression(node);
            BlockSyntax body = this.ProcessAnonymousFunction(node, convertedType);
            return ((body != null) ? node.WithBody(body) : node);
        }

        public override SyntaxNode VisitYieldStatement(YieldStatementSyntax node)
        {
            TypeSyntax syntax2;
            node = (YieldStatementSyntax) base.VisitYieldStatement(node);
            if (node.ReturnOrBreakKeyword.IsKind(SyntaxKind.BreakKeyword))
            {
                return node;
            }
            if ((node.Expression == null) || node.Expression.IsMissing)
            {
                return node;
            }
            MethodDeclarationSyntax syntax = node.FirstAncestorOrSelf<MethodDeclarationSyntax>((Func<MethodDeclarationSyntax, bool>) null, true);
            if (syntax == null)
            {
                return node;
            }
            NameSyntax returnType = syntax.ReturnType as NameSyntax;
            if (returnType == null)
            {
                return node;
            }
            QualifiedNameSyntax syntax4 = returnType as QualifiedNameSyntax;
            if (syntax4 != null)
            {
                returnType = syntax4.Right;
            }
            if (returnType.Arity == 0)
            {
                syntax2 = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword));
            }
            else
            {
                syntax2 = ((GenericNameSyntax) returnType).TypeArgumentList.Arguments[0];
            }
            LinePosition position1 = new LinePosition(node.Expression.GetLocation().GetMappedLineSpan().EndLinePosition.Line + 1, 0);
            StatementSyntax[] statements = new StatementSyntax[] { node.WithExpression(this.YieldGuard(node.Expression, syntax2)), this.ReenterYieldMethodCall() };
            return SyntaxFactory.Block(statements);
        }

        private ExpressionSyntax YieldGuard(ExpressionSyntax expression, TypeSyntax genericAttribute = null)
        {
            SyntaxToken identifier = SyntaxFactory.Identifier("YieldGuard");
            SimpleNameSyntax node = (genericAttribute != null) ? ((SimpleNameSyntax) SyntaxFactory.GenericName(identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(genericAttribute)))) : ((SimpleNameSyntax) SyntaxFactory.IdentifierName(identifier));
            ArgumentSyntax[] syntaxArray1 = new ArgumentSyntax[] { this.MakeModIdArg(), SyntaxFactory.Argument(expression.WithoutTrivia<ExpressionSyntax>()) };
            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(ModPerfCounter).FullName), Annotated<SimpleNameSyntax>(node))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(syntaxArray1))).WithTriviaFrom<InvocationExpressionSyntax>(expression);
        }
    }
}

