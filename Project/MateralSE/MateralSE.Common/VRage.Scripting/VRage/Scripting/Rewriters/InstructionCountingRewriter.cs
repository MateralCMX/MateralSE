namespace VRage.Scripting.Rewriters
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Compiler;
    using VRage.Scripting;
    using VRage.Scripting.Analyzers;

    internal class InstructionCountingRewriter : CSharpSyntaxRewriter
    {
        private const string COMPILER_METHODS_POSTFIX = "";
        private const string COMPILER_METHOD_IS_DEAD = "IsDead";
        private const string COMPILER_METHOD_COUNT_INSTRUCTION = "CountInstructions";
        private const string COMPILER_METHOD_EXIT_METHOD = "ExitMethod";
        private const string COMPILER_METHOD_ENTER_METHOD = "EnterMethod";
        private const string COMPILER_METHOD_YIELD_GUARD_METHOD = "YieldGuard";
        private readonly CSharpCompilation m_compilation;
        private readonly MyScriptCompiler m_compiler;
        private SemanticModel m_semanticModel;
        private SyntaxTree m_syntaxTree;

        public InstructionCountingRewriter(MyScriptCompiler compiler, CSharpCompilation compilation, SyntaxTree syntaxTree) : base(false)
        {
            this.m_compiler = compiler;
            this.m_compilation = compilation;
            this.m_syntaxTree = syntaxTree;
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

        private BlockSyntax CreateDelegateMethodBody(ExpressionSyntax expression, bool hasReturnValue)
        {
            StatementSyntax syntax = !hasReturnValue ? ((StatementSyntax) SyntaxFactory.ExpressionStatement(expression)) : ((StatementSyntax) SyntaxFactory.ReturnStatement(expression));
            StatementSyntax[] statements = new StatementSyntax[] { syntax };
            return this.InjectBlockAsMethodBody(SyntaxFactory.Block(statements));
        }

        private StatementSyntax DeadCheckIfStatement(StatementSyntax body) => 
            SyntaxFactory.IfStatement(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, IsDeadCall()), this.InjectedBlock(body, null));

        private StatementSyntax EnterMethodCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(IlInjector).FullName), (SimpleNameSyntax) AnnotatedIdentifier("EnterMethod"))));

        private StatementSyntax ExitMethodCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(IlInjector).FullName), (SimpleNameSyntax) AnnotatedIdentifier("ExitMethod"))));

        private static string GenerateUniqueIdentifier(FileLinePositionSpan location) => 
            $"__gen_{location.StartLinePosition.Line}_{location.StartLinePosition.Character}";

        private FileLinePositionSpan GetBlockResumeLocation(SyntaxNode node)
        {
            BlockSyntax syntax = node as BlockSyntax;
            if (syntax == null)
            {
                return node.GetLocation().GetMappedLineSpan();
            }
            if (syntax.Statements.Count == 0)
            {
                return syntax.CloseBraceToken.GetLocation().GetMappedLineSpan();
            }
            return syntax.Statements[0].GetLocation().GetMappedLineSpan();
        }

        private BlockSyntax InjectBlockAsMethodBody(BlockSyntax methodBody)
        {
            StatementSyntax[] statements = new StatementSyntax[3];
            statements[0] = this.EnterMethodCall();
            statements[1] = this.InstructionCounterCall();
            SyntaxList<CatchClauseSyntax> catches = new SyntaxList<CatchClauseSyntax>();
            StatementSyntax[] syntaxArray2 = new StatementSyntax[] { this.ExitMethodCall() };
            statements[2] = SyntaxFactory.TryStatement(methodBody, catches, SyntaxFactory.FinallyClause(SyntaxFactory.Block(syntaxArray2)));
            return SyntaxFactory.Block(statements);
        }

        private BlockSyntax InjectedBlock(StatementSyntax node, StatementSyntax injection = null)
        {
            injection = injection ?? this.InstructionCounterCall();
            BlockSyntax syntax = node as BlockSyntax;
            if (syntax != null)
            {
                return syntax.WithStatements(syntax.Statements.Insert(0, injection));
            }
            StatementSyntax[] statements = new StatementSyntax[] { injection, node };
            return SyntaxFactory.Block(statements);
        }

        private FinallyClauseSyntax InjectedFinally(StatementSyntax body)
        {
            StatementSyntax[] statements = new StatementSyntax[] { this.DeadCheckIfStatement(body) };
            return SyntaxFactory.FinallyClause(SyntaxFactory.Block(statements));
        }

        private StatementSyntax InstructionCounterCall() => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(IlInjector).FullName), (SimpleNameSyntax) AnnotatedIdentifier("CountInstructions"))));

        private static ExpressionSyntax IsDeadCall() => 
            SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(IlInjector).FullName), (SimpleNameSyntax) AnnotatedIdentifier("IsDead")));

        private SyntaxNode ProcessAnonymousFunction(AnonymousFunctionExpressionSyntax node, INamedTypeSymbol type)
        {
            BlockSyntax body = node.Body as BlockSyntax;
            if (body != null)
            {
                return node.WithBody(this.InjectBlockAsMethodBody(body));
            }
            if ((type == null) || (type.DelegateInvokeMethod == null))
            {
                return node;
            }
            return node.WithBody(this.CreateDelegateMethodBody((ExpressionSyntax) node.Body, !type.DelegateInvokeMethod.ReturnsVoid));
        }

        private SyntaxNode ProcessMethod(BaseMethodDeclarationSyntax node)
        {
            if (node.Body != null)
            {
                return node.WithBody(this.InjectBlockAsMethodBody(node.Body));
            }
            MethodDeclarationSyntax syntax = node as MethodDeclarationSyntax;
            if (syntax != null)
            {
                PredefinedTypeSyntax returnType = syntax.ReturnType as PredefinedTypeSyntax;
                bool flag = (returnType != null) && returnType.Keyword.IsKind(SyntaxKind.VoidKeyword);
                return syntax.WithExpressionBody(null).WithBody(this.CreateDelegateMethodBody(syntax.ExpressionBody.Expression, !flag));
            }
            OperatorDeclarationSyntax syntax2 = node as OperatorDeclarationSyntax;
            if (syntax2 != null)
            {
                return syntax2.WithExpressionBody(null).WithBody(this.CreateDelegateMethodBody(syntax2.ExpressionBody.Expression, true));
            }
            ConversionOperatorDeclarationSyntax syntax3 = node as ConversionOperatorDeclarationSyntax;
            if (syntax3 != null)
            {
                return syntax3.WithExpressionBody(null).WithBody(this.CreateDelegateMethodBody(syntax3.ExpressionBody.Expression, true));
            }
            if ((node is ConstructorDeclarationSyntax) || (node is DestructorDeclarationSyntax))
            {
                throw new ArgumentException("Constructors and destructors have to have bodies!", "node");
            }
            throw new ArgumentException("Unknown " + node.GetType().FullName, "node");
        }

        public SyntaxTree Rewrite()
        {
            CancellationToken token = new CancellationToken();
            CSharpSyntaxNode root = (CSharpSyntaxNode) this.m_syntaxTree.GetRoot(token);
            this.m_semanticModel = this.m_compilation.GetSemanticModel(this.m_syntaxTree, false);
            CSharpSyntaxNode node2 = (CSharpSyntaxNode) this.Visit(root);
            return this.m_syntaxTree.WithRootAndOptions(node2, this.m_syntaxTree.Options);
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            if (node.Body == null)
            {
                return base.VisitAccessorDeclaration(node);
            }
            node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration(node);
            return node.WithBody(this.InjectBlockAsMethodBody(node.Body));
        }

        public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (AnonymousMethodExpressionSyntax) base.VisitAnonymousMethodExpression(node);
            return this.ProcessAnonymousFunction(node, convertedType);
        }

        public override SyntaxNode VisitCatchClause(CatchClauseSyntax node)
        {
            if (node.Span.IsEmpty || node.Block.Span.IsEmpty)
            {
                return base.VisitCatchClause(node);
            }
            FileLinePositionSpan blockResumeLocation = this.GetBlockResumeLocation(node.Block);
            node = (CatchClauseSyntax) base.VisitCatchClause(node);
            if (node.Declaration == null)
            {
                node = node.WithDeclaration(SyntaxFactory.CatchDeclaration(SyntaxFactory.ParseTypeName(typeof(Exception).FullName, 0, true), SyntaxFactory.Identifier(GenerateUniqueIdentifier(blockResumeLocation))));
            }
            else
            {
                if (node.Declaration.Type.IsMissing)
                {
                    return node;
                }
                if (node.Declaration.Identifier.IsKind(SyntaxKind.None))
                {
                    node = node.WithDeclaration(node.Declaration.WithIdentifier(SyntaxFactory.Identifier(GenerateUniqueIdentifier(blockResumeLocation))));
                }
            }
            string exceptionIdentifier = node.Declaration.Identifier.ValueText;
            SyntaxTrivia[] trivia = new SyntaxTrivia[] { SyntaxFactory.Trivia(SyntaxFactory.PragmaWarningDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.DisableKeyword), SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.IdentifierName("CS0184")), true)) };
            SyntaxTrivia[] triviaArray2 = new SyntaxTrivia[] { SyntaxFactory.Trivia(SyntaxFactory.PragmaWarningDirectiveTrivia(SyntaxFactory.Token(SyntaxKind.RestoreKeyword), SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.IdentifierName("CS0184")), true)) };
            PrefixUnaryExpressionSyntax left = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(this.m_compiler.UnblockableIngameExceptions.Aggregate<Type, BinaryExpressionSyntax>(null, delegate (BinaryExpressionSyntax aggregation, Type current) {
                BinaryExpressionSyntax syntax = SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, SyntaxFactory.IdentifierName(exceptionIdentifier), AnnotatedIdentifier(current.FullName));
                return (aggregation != null) ? SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, syntax, aggregation) : syntax;
            }))).WithLeadingTrivia<PrefixUnaryExpressionSyntax>(trivia).WithTrailingTrivia<PrefixUnaryExpressionSyntax>(triviaArray2);
            node = (node.Filter != null) ? node.WithFilter(SyntaxFactory.CatchFilterClause(SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, left, SyntaxFactory.ParenthesizedExpression(node.Filter.FilterExpression)))) : node.WithFilter(SyntaxFactory.CatchFilterClause(left));
            return node.WithBlock(this.InjectedBlock(node.Block, null));
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (node.Body == null)
            {
                return base.VisitConstructorDeclaration(node);
            }
            node = (ConstructorDeclarationSyntax) base.VisitConstructorDeclaration(node);
            return this.ProcessMethod(node);
        }

        public override SyntaxNode VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
        {
            if ((node.Body == null) && (node.ExpressionBody == null))
            {
                return base.VisitConversionOperatorDeclaration(node);
            }
            node = (ConversionOperatorDeclarationSyntax) base.VisitConversionOperatorDeclaration(node);
            return this.ProcessMethod(node);
        }

        public override SyntaxNode VisitDestructorDeclaration(DestructorDeclarationSyntax node)
        {
            if (node.Body == null)
            {
                return base.VisitDestructorDeclaration(node);
            }
            node = (DestructorDeclarationSyntax) base.VisitDestructorDeclaration(node);
            return this.ProcessMethod(node);
        }

        public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
        {
            node = (DoStatementSyntax) base.VisitDoStatement(node);
            node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            return node;
        }

        public override SyntaxNode VisitElseClause(ElseClauseSyntax node)
        {
            node = (ElseClauseSyntax) base.VisitElseClause(node);
            if (node.Statement.Kind() != SyntaxKind.IfStatement)
            {
                node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            }
            return node;
        }

        public override SyntaxNode VisitFinallyClause(FinallyClauseSyntax node)
        {
            if (node.Span.IsEmpty || node.Block.Span.IsEmpty)
            {
                return base.VisitFinallyClause(node);
            }
            node = (FinallyClauseSyntax) base.VisitFinallyClause(node);
            return this.InjectedFinally(node.Block);
        }

        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            node = (ForEachStatementSyntax) base.VisitForEachStatement(node);
            node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            return node;
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            node = (ForStatementSyntax) base.VisitForStatement(node);
            node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            return node;
        }

        public override SyntaxNode VisitGotoStatement(GotoStatementSyntax node)
        {
            if (node.CaseOrDefaultKeyword.Kind() != SyntaxKind.None)
            {
                return base.VisitGotoStatement(node);
            }
            node = (GotoStatementSyntax) base.VisitGotoStatement(node);
            return this.InjectedBlock(node, null);
        }

        public override SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            node = (IfStatementSyntax) base.VisitIfStatement(node);
            node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            return node;
        }

        public override unsafe SyntaxNode VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            SyntaxList<AccessorDeclarationSyntax> list;
            if (node.ExpressionBody == null)
            {
                return base.VisitIndexerDeclaration(node);
            }
            if (node.AccessorList != null)
            {
                return node;
            }
            node = (IndexerDeclarationSyntax) base.VisitIndexerDeclaration(node);
            SyntaxList<AccessorDeclarationSyntax>* listPtr1 = &list;
            listPtr1 = (SyntaxList<AccessorDeclarationSyntax>*) new SyntaxList<AccessorDeclarationSyntax>();
            return node.WithExpressionBody(null).WithAccessorList(SyntaxFactory.AccessorList(listPtr1.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, this.CreateDelegateMethodBody(node.ExpressionBody.Expression, true)))));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if ((node.Body == null) && (node.ExpressionBody == null))
            {
                return base.VisitMethodDeclaration(node);
            }
            node = (MethodDeclarationSyntax) base.VisitMethodDeclaration(node);
            return this.ProcessMethod(node);
        }

        public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            if ((node.Body == null) && (node.ExpressionBody == null))
            {
                return base.VisitOperatorDeclaration(node);
            }
            node = (OperatorDeclarationSyntax) base.VisitOperatorDeclaration(node);
            return this.ProcessMethod(node);
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (ParenthesizedLambdaExpressionSyntax) base.VisitParenthesizedLambdaExpression(node);
            return this.ProcessAnonymousFunction(node, convertedType);
        }

        public override unsafe SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            SyntaxList<AccessorDeclarationSyntax> list;
            if (node.ExpressionBody == null)
            {
                return base.VisitPropertyDeclaration(node);
            }
            node = (PropertyDeclarationSyntax) base.VisitPropertyDeclaration(node);
            SyntaxList<AccessorDeclarationSyntax>* listPtr1 = &list;
            listPtr1 = (SyntaxList<AccessorDeclarationSyntax>*) new SyntaxList<AccessorDeclarationSyntax>();
            return node.WithExpressionBody(null).WithAccessorList(SyntaxFactory.AccessorList(listPtr1.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, this.CreateDelegateMethodBody(node.ExpressionBody.Expression, true)))));
        }

        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            CancellationToken token = new CancellationToken();
            INamedTypeSymbol convertedType = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetTypeInfo(this.m_semanticModel, node, token).ConvertedType as INamedTypeSymbol;
            node = (SimpleLambdaExpressionSyntax) base.VisitSimpleLambdaExpression(node);
            return this.ProcessAnonymousFunction(node, convertedType);
        }

        public override SyntaxNode VisitSwitchSection(SwitchSectionSyntax node)
        {
            node = (SwitchSectionSyntax) base.VisitSwitchSection(node);
            if (node.Statements.Count > 0)
            {
                node = node.WithStatements(node.Statements.Insert(0, this.InstructionCounterCall()));
            }
            return node;
        }

        public override unsafe SyntaxNode VisitUsingStatement(UsingStatementSyntax node)
        {
            SeparatedSyntaxList<VariableDeclaratorSyntax> variables;
            FileLinePositionSpan blockResumeLocation = this.GetBlockResumeLocation(node);
            ExpressionSyntax expression = null;
            StatementSyntax syntax2 = null;
            if (node.Declaration != null)
            {
                variables = node.Declaration.Variables;
                if (variables.Count > 0)
                {
                    syntax2 = SyntaxFactory.LocalDeclarationStatement(node.Declaration);
                    expression = SyntaxFactory.IdentifierName(node.Declaration.Variables[0].Identifier);
                    goto TR_0006;
                }
            }
            if (node.Expression != null)
            {
                expression = SyntaxFactory.IdentifierName(GenerateUniqueIdentifier(blockResumeLocation));
                SeparatedSyntaxList<VariableDeclaratorSyntax>* listPtr1 = &variables;
                listPtr1 = (SeparatedSyntaxList<VariableDeclaratorSyntax>*) new SeparatedSyntaxList<VariableDeclaratorSyntax>();
                syntax2 = SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"), listPtr1.Add(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(GenerateUniqueIdentifier(blockResumeLocation)), null, SyntaxFactory.EqualsValueClause(node.Expression)))));
            }
        TR_0006:
            if (((expression == null) || (node.Statement.IsMissing || (node.UsingKeyword.IsMissing || node.OpenParenToken.IsMissing))) || node.CloseParenToken.IsMissing)
            {
                return base.VisitUsingStatement(node);
            }
            node = (UsingStatementSyntax) base.VisitUsingStatement(node);
            StatementSyntax[] statements = new StatementSyntax[2];
            statements[0] = syntax2;
            SyntaxList<CatchClauseSyntax> catches = new SyntaxList<CatchClauseSyntax>();
            StatementSyntax[] syntaxArray2 = new StatementSyntax[] { SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, SyntaxFactory.IdentifierName("Dispose")))) };
            statements[1] = SyntaxFactory.TryStatement(this.InjectedBlock(node.Statement, null), catches, this.InjectedFinally(SyntaxFactory.Block(syntaxArray2)));
            return SyntaxFactory.Block(statements);
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            node = (WhileStatementSyntax) base.VisitWhileStatement(node);
            node = node.WithStatement(this.InjectedBlock(node.Statement, null));
            return node;
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
            StatementSyntax[] statements = new StatementSyntax[] { node.WithExpression(this.YieldGuard(node.Expression, syntax2)), this.EnterMethodCall() };
            return SyntaxFactory.Block(statements);
        }

        private ExpressionSyntax YieldGuard(ExpressionSyntax expression, TypeSyntax genericAttribute = null)
        {
            SyntaxToken identifier = SyntaxFactory.Identifier("YieldGuard");
            SimpleNameSyntax node = (genericAttribute != null) ? ((SimpleNameSyntax) SyntaxFactory.GenericName(identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(genericAttribute)))) : ((SimpleNameSyntax) SyntaxFactory.IdentifierName(identifier));
            return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AnnotatedIdentifier(typeof(IlInjector).FullName), Annotated<SimpleNameSyntax>(node))).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(SyntaxFactory.Argument(expression.WithoutTrivia<ExpressionSyntax>())))).WithTriviaFrom<InvocationExpressionSyntax>(expression);
        }
    }
}

