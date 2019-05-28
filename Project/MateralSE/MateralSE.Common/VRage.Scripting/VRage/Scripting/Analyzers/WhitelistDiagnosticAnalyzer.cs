namespace VRage.Scripting.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using VRage.Scripting;

    internal class WhitelistDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly SyntaxAnnotation INJECTED_ANNOTATION = new SyntaxAnnotation("Injected");
        internal static readonly DiagnosticDescriptor PROHIBITED_MEMBER_RULE = new DiagnosticDescriptor("ProhibitedMemberRule", "Prohibited Type Or Member", "The type or member '{0}' is prohibited", "Whitelist", DiagnosticSeverity.Error, true, null, null, Array.Empty<string>());
        internal static readonly DiagnosticDescriptor PROHIBITED_LANGUAGE_ELEMENT_RULE = new DiagnosticDescriptor("ProhibitedLanguageElement", "Prohibited Language Element", "The language element '{0}' is prohibited", "Whitelist", DiagnosticSeverity.Error, true, null, null, Array.Empty<string>());
        private readonly MyScriptWhitelist m_whitelist;
        private readonly MyWhitelistTarget m_target;
        private readonly System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> m_supportedDiagnostics = System.Collections.Immutable.ImmutableArray.Create<DiagnosticDescriptor>(PROHIBITED_MEMBER_RULE, PROHIBITED_LANGUAGE_ELEMENT_RULE);

        public WhitelistDiagnosticAnalyzer(MyScriptWhitelist whitelist, MyWhitelistTarget target)
        {
            this.m_whitelist = whitelist;
            this.m_target = target;
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;
            if (!node.HasAnnotation(INJECTED_ANNOTATION))
            {
                if ((node.Kind() == SyntaxKind.DestructorDeclaration) && (this.m_target == MyWhitelistTarget.Ingame))
                {
                    object[] messageArgs = new object[] { "Finalizer" };
                    Diagnostic diagnostic = Diagnostic.Create(PROHIBITED_LANGUAGE_ELEMENT_RULE, node.GetLocation(), messageArgs);
                    context.ReportDiagnostic(diagnostic);
                }
                else if (!this.IsQualifiedName(node.Parent))
                {
                    CancellationToken token = new CancellationToken();
                    SymbolInfo info = ModelExtensions.GetSymbolInfo(context.SemanticModel, node, token);
                    if ((info.Symbol != null) && !info.Symbol.IsInSource())
                    {
                        if (!this.m_whitelist.IsWhitelisted(info.Symbol, this.m_target))
                        {
                            object[] messageArgs = new object[] { info.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) };
                            Diagnostic diagnostic = Diagnostic.Create(PROHIBITED_MEMBER_RULE, node.GetLocation(), messageArgs);
                            context.ReportDiagnostic(diagnostic);
                        }
                        else
                        {
                            string key = info.Symbol.ContainingNamespace.ToString();
                            SortedDictionary<string, int> usedNamespaces = MyScriptCompiler.UsedNamespaces;
                            lock (usedNamespaces)
                            {
                                if (!MyScriptCompiler.UsedNamespaces.ContainsKey(key))
                                {
                                    MyScriptCompiler.UsedNamespaces.Add(key, 1);
                                }
                                else
                                {
                                    string str2 = key;
                                    MyScriptCompiler.UsedNamespaces[str2] += 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction<SyntaxKind>(new Action<SyntaxNodeAnalysisContext>(this.Analyze), new SyntaxKind[] { SyntaxKind.DestructorDeclaration, SyntaxKind.AliasQualifiedName, SyntaxKind.QualifiedName, SyntaxKind.GenericName, SyntaxKind.IdentifierName });
        }

        private bool IsQualifiedName(SyntaxNode arg)
        {
            SyntaxKind kind = arg.Kind();
            return ((kind == SyntaxKind.QualifiedName) || (kind == SyntaxKind.AliasQualifiedName));
        }

        public override System.Collections.Immutable.ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            this.m_supportedDiagnostics;
    }
}

