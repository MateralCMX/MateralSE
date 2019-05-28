namespace VRage.Game.VisualScripting.Utils
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Runtime.CompilerServices;

    public static class MyRoslynExtensions
    {
        public static ClassDeclarationSyntax DeclaringClass(this MethodDeclarationSyntax methodSyntax) => 
            (!(methodSyntax.Parent is ClassDeclarationSyntax) ? null : (methodSyntax.Parent as ClassDeclarationSyntax));

        public static bool IsBool(this ITypeSymbol symbol) => 
            ((symbol != null) ? (symbol.MetadataName == "Boolean") : false);

        public static bool IsDerivedTypeOf(this ITypeSymbol derivedType, ITypeSymbol type) => 
            IsDerivedTypeRecursive(derivedType, type);

        private static bool IsDerivedTypeRecursive(ITypeSymbol derivedType, ITypeSymbol type) => 
            (!ReferenceEquals(derivedType, type) ? ((derivedType.BaseType != null) ? IsDerivedTypeRecursive(derivedType.BaseType, type) : false) : true);

        public static bool IsFloat(this ITypeSymbol symbol) => 
            ((symbol != null) ? (symbol.MetadataName == "Single") : false);

        public static bool IsInt(this ITypeSymbol symbol) => 
            ((symbol != null) ? ((symbol.MetadataName == "Int32") || (symbol.MetadataName == "Int64")) : false);

        public static bool IsOut(this ParameterSyntax paramSyntax) => 
            paramSyntax.Modifiers.Any(SyntaxKind.OutKeyword);

        public static bool IsSequenceDependent(this MethodDeclarationSyntax methodSyntax)
        {
            if (methodSyntax.AttributeLists.Count > 0)
            {
                SeparatedSyntaxList<AttributeSyntax>.Enumerator enumerator = methodSyntax.AttributeLists.First().Attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    AttributeSyntax current = enumerator.Current;
                    if (current.Name.ToString() == "VisualScriptingMember")
                    {
                        if (current.ArgumentList == null)
                        {
                            return false;
                        }
                        return (current.ArgumentList.Arguments.First().Expression.Kind() == SyntaxKind.TrueLiteralExpression);
                    }
                }
            }
            return false;
        }

        public static bool IsStatic(this MethodDeclarationSyntax methodSyntax) => 
            methodSyntax.Modifiers.Any(SyntaxKind.StaticKeyword);

        public static bool IsString(this ITypeSymbol symbol) => 
            ((symbol != null) ? (symbol.MetadataName == "String") : false);

        public static bool LiteComparator(this ITypeSymbol current, ITypeSymbol another) => 
            (current.Name == another.Name);

        public static string SerializeToObjectBuilder(this MethodDeclarationSyntax syntax)
        {
            ClassDeclarationSyntax syntax2 = syntax.DeclaringClass();
            NamespaceDeclarationSyntax parent = syntax2.Parent as NamespaceDeclarationSyntax;
            object[] objArray1 = new object[] { parent.Name, ".", syntax2.Identifier.Text, ".", syntax.Identifier.Text, syntax.ParameterList.ToFullString() };
            return string.Concat(objArray1);
        }

        public static string SerializeToObjectBuilder(this ITypeSymbol typeSymbol) => 
            (typeSymbol.ContainingNamespace.Name + "." + typeSymbol.MetadataName);
    }
}

