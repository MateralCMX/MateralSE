namespace VRage.Scripting
{
    using Microsoft.CodeAnalysis;
    using System;
    using System.Runtime.CompilerServices;

    internal static class TypeKeyExtensions
    {
        public static string GetWhitelistKey(this INamespaceSymbol symbol, TypeKeyQuantity quantity)
        {
            if (quantity == TypeKeyQuantity.ThisOnly)
            {
                return (symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ", " + symbol.ContainingAssembly.Name);
            }
            if (quantity != TypeKeyQuantity.AllMembers)
            {
                throw new ArgumentOutOfRangeException("quantity", quantity, null);
            }
            return (symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ".*, " + symbol.ContainingAssembly.Name);
        }

        public static string GetWhitelistKey(this ISymbol symbol, TypeKeyQuantity quantity)
        {
            ISymbol symbol1;
            INamespaceSymbol symbol2 = symbol as INamespaceSymbol;
            if (symbol2 != null)
            {
                return symbol2.GetWhitelistKey(quantity);
            }
            ITypeSymbol symbol3 = symbol as ITypeSymbol;
            if (symbol3 != null)
            {
                return symbol3.GetWhitelistKey(quantity);
            }
            IMethodSymbol originalDefinition = symbol as IMethodSymbol;
            if (((originalDefinition != null) && originalDefinition.IsGenericMethod) && !originalDefinition.IsDefinition)
            {
                originalDefinition = originalDefinition.OriginalDefinition;
                if (originalDefinition.IsExtensionMethod && (originalDefinition.ReducedFrom != null))
                {
                    originalDefinition = originalDefinition.ReducedFrom;
                }
                return (originalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ", " + symbol.ContainingAssembly.Name);
            }
            if (((symbol is IEventSymbol) || ((symbol is IFieldSymbol) || (symbol is IPropertySymbol))) || (symbol is IMethodSymbol))
            {
                symbol1 = symbol;
            }
            else
            {
                symbol1 = null;
            }
            ISymbol local1 = symbol1;
            if (local1 == null)
            {
                throw new ArgumentException("Invalid symbol type: Expected namespace, type or type member", "symbol");
            }
            return (local1.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ", " + symbol.ContainingAssembly.Name);
        }

        public static string GetWhitelistKey(this ITypeSymbol symbol, TypeKeyQuantity quantity)
        {
            ITypeSymbol symbol1 = ResolveRootType(symbol);
            symbol = symbol1;
            if (quantity == TypeKeyQuantity.ThisOnly)
            {
                return (symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + ", " + symbol.ContainingAssembly.Name);
            }
            if (quantity != TypeKeyQuantity.AllMembers)
            {
                throw new ArgumentOutOfRangeException("quantity", quantity, null);
            }
            return (symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat) + "+*, " + symbol.ContainingAssembly.Name);
        }

        private static ITypeSymbol ResolveRootType(ITypeSymbol symbol)
        {
            INamedTypeSymbol symbol2 = symbol as INamedTypeSymbol;
            if (((symbol2 == null) || !symbol2.IsGenericType) || symbol2.IsDefinition)
            {
                IPointerTypeSymbol symbol3 = symbol as IPointerTypeSymbol;
                return ((symbol3 == null) ? symbol : symbol3.PointedAtType);
            }
            symbol = symbol2.OriginalDefinition;
            return symbol;
        }
    }
}

