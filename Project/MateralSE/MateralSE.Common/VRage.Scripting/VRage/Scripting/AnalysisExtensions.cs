namespace VRage.Scripting
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Immutable;
    using System.Runtime.CompilerServices;

    internal static class AnalysisExtensions
    {
        public static ISymbol GetOverriddenSymbol(this ISymbol symbol)
        {
            if (!symbol.IsOverride)
            {
                return null;
            }
            ITypeSymbol symbol2 = symbol as ITypeSymbol;
            if (symbol2 != null)
            {
                return symbol2.BaseType;
            }
            IEventSymbol symbol3 = symbol as IEventSymbol;
            if (symbol3 != null)
            {
                return symbol3.OverriddenEvent;
            }
            IPropertySymbol symbol4 = symbol as IPropertySymbol;
            if (symbol4 != null)
            {
                return symbol4.OverriddenProperty;
            }
            IMethodSymbol symbol5 = symbol as IMethodSymbol;
            return symbol5?.OverriddenMethod;
        }

        public static bool IsInSource(this ISymbol symbol)
        {
            int num = 0;
            while (true)
            {
                System.Collections.Immutable.ImmutableArray<Location> locations = symbol.Locations;
                if (num >= locations.Length)
                {
                    return true;
                }
                if (!symbol.Locations[num].IsInSource)
                {
                    return false;
                }
                num++;
            }
        }

        public static bool IsMemberSymbol(this ISymbol symbol) => 
            ((symbol is IEventSymbol) || ((symbol is IFieldSymbol) || ((symbol is IPropertySymbol) || (symbol is IMethodSymbol))));

        public static AnonymousFunctionExpressionSyntax WithBody(this AnonymousFunctionExpressionSyntax item, CSharpSyntaxNode body)
        {
            AnonymousMethodExpressionSyntax syntax = item as AnonymousMethodExpressionSyntax;
            if (syntax != null)
            {
                return syntax.WithBody(body);
            }
            ParenthesizedLambdaExpressionSyntax syntax2 = item as ParenthesizedLambdaExpressionSyntax;
            if (syntax2 != null)
            {
                return syntax2.WithBody(body);
            }
            SimpleLambdaExpressionSyntax syntax3 = item as SimpleLambdaExpressionSyntax;
            if (syntax3 == null)
            {
                throw new ArgumentException("Unknown " + typeof(AnonymousFunctionExpressionSyntax).FullName, "item");
            }
            return syntax3.WithBody(body);
        }

        public static BaseMethodDeclarationSyntax WithBody(this BaseMethodDeclarationSyntax item, BlockSyntax body)
        {
            ConstructorDeclarationSyntax syntax = item as ConstructorDeclarationSyntax;
            if (syntax != null)
            {
                return syntax.WithBody(body);
            }
            ConversionOperatorDeclarationSyntax syntax2 = item as ConversionOperatorDeclarationSyntax;
            if (syntax2 != null)
            {
                return syntax2.WithBody(body);
            }
            DestructorDeclarationSyntax syntax3 = item as DestructorDeclarationSyntax;
            if (syntax3 != null)
            {
                return syntax3.WithBody(body);
            }
            MethodDeclarationSyntax syntax4 = item as MethodDeclarationSyntax;
            if (syntax4 != null)
            {
                return syntax4.WithBody(body);
            }
            OperatorDeclarationSyntax syntax5 = item as OperatorDeclarationSyntax;
            if (syntax5 == null)
            {
                throw new ArgumentException("Unknown " + typeof(BaseMethodDeclarationSyntax).FullName, "item");
            }
            return syntax5.WithBody(body);
        }
    }
}

