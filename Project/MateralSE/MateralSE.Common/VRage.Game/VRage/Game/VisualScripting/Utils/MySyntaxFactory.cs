namespace VRage.Game.VisualScripting.Utils
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Game.VisualScripting;
    using VRageMath;

    public static class MySyntaxFactory
    {
        public static LocalDeclarationStatementSyntax ArithmeticStatement(string resultVariableName, string leftSide, string rightSide, string operation)
        {
            string[] textArray1 = new string[] { leftSide, " ", operation, " ", rightSide };
            ExpressionSyntax syntax2 = SyntaxFactory.ParseExpression(string.Concat(textArray1), 0, null, true);
            return SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(resultVariableName)).WithInitializer(SyntaxFactory.EqualsValueClause(syntax2)))));
        }

        public static LocalDeclarationStatementSyntax CastExpression(string castedVariableName, string type, string resultVariableName) => 
            SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var")).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(resultVariableName)).WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.CastExpression(SyntaxFactory.PredefinedType(SyntaxFactory.ParseToken(type, 0)), SyntaxFactory.IdentifierName(castedVariableName))))))).NormalizeWhitespace<LocalDeclarationStatementSyntax>("    ", "\r\n", false);

        public static ArgumentSyntax ConstantArgument(string typeSignature, string value)
        {
            Type type = MyVisualScriptingProxy.GetType(typeSignature);
            if ((type == typeof(Color)) || type.IsEnum)
            {
                return SyntaxFactory.Argument(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(typeSignature), SyntaxFactory.IdentifierName(value)));
            }
            return (!(type == typeof(Vector3D)) ? SyntaxFactory.Argument(Literal(typeSignature, value)) : SyntaxFactory.Argument(NewVector3D(value)));
        }

        public static ArgumentSyntax ConstantDefaultArgument(Type type)
        {
            if (type.IsClass)
            {
                return SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
            }
            if (((type == typeof(int)) || ((type == typeof(float)) || (type == typeof(long)))) || (type == typeof(double)))
            {
                return SyntaxFactory.Argument(Literal(type.Signature(), "0"));
            }
            SeparatedSyntaxList<ArgumentSyntax> arguments = new SeparatedSyntaxList<ArgumentSyntax>();
            return SyntaxFactory.Argument(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(type.Signature())).WithArgumentList(SyntaxFactory.ArgumentList(arguments)));
        }

        public static ConstructorDeclarationSyntax Constructor(ClassDeclarationSyntax classDeclaration)
        {
            SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();
            return SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(classDeclaration.Identifier.Text)).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithBody(SyntaxFactory.Block(statements)).NormalizeWhitespace<ConstructorDeclarationSyntax>("    ", "\r\n", false);
        }

        public static ArgumentSyntax CreateArgumentSyntax(string value) => 
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression(value, 0, null, true));

        public static ArgumentSyntax CreateOutArgumentSyntax(string value) => 
            SyntaxFactory.Argument(SyntaxFactory.ParseExpression(value, 0, null, true)).WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword));

        public static ExpressionStatementSyntax DelegateAssignment(string deletageIdentifier, string methodName) => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.AddAssignmentExpression, SyntaxFactory.IdentifierName(deletageIdentifier), SyntaxFactory.IdentifierName(methodName)));

        public static ExpressionStatementSyntax DelegateRemoval(string deletageIdentifier, string methodName) => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SubtractAssignmentExpression, SyntaxFactory.IdentifierName(deletageIdentifier), SyntaxFactory.IdentifierName(methodName)));

        public static FieldDeclarationSyntax GenericFieldDeclaration(Type type, string fieldVariableName, SyntaxTokenList? modifiers = new SyntaxTokenList?())
        {
            if (modifiers == null)
            {
                modifiers = new SyntaxTokenList?(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            }
            return SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(GenericTypeSyntax(type), SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(fieldVariableName))))).WithModifiers(modifiers.Value);
        }

        public static ObjectCreationExpressionSyntax GenericObjectCreation(Type type, IEnumerable<ExpressionSyntax> argumentExpressions = null)
        {
            List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
            Type[] genericArguments = type.GetGenericArguments();
            for (int i = 0; i < genericArguments.Length; i++)
            {
                IdentifierNameSyntax item = SyntaxFactory.IdentifierName(genericArguments[i].FullName);
                list.Add(item);
                if (i < (genericArguments.Length - 1))
                {
                    list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }
            }
            TypeSyntax syntax = GenericTypeSyntax(type);
            List<ArgumentSyntax> list2 = new List<ArgumentSyntax>();
            if (argumentExpressions != null)
            {
                using (IEnumerator<ExpressionSyntax> enumerator = argumentExpressions.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ArgumentSyntax item = SyntaxFactory.Argument(enumerator.Current);
                        list2.Add(item);
                    }
                }
            }
            return SyntaxFactory.ObjectCreationExpression(syntax).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list2)));
        }

        public static TypeSyntax GenericTypeSyntax(Type type)
        {
            if (!type.IsGenericType)
            {
                return (!(type == typeof(void)) ? ((TypeSyntax) SyntaxFactory.IdentifierName(type.FullName)) : ((TypeSyntax) SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword))));
            }
            List<TypeSyntax> list = new List<TypeSyntax>();
            foreach (Type type2 in type.GetGenericArguments())
            {
                list.Add(GenericTypeSyntax(type2));
            }
            return SyntaxFactory.GenericName(SyntaxFactory.Identifier(type.Name.Remove(type.Name.IndexOf('`'))), SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList<TypeSyntax>(list)));
        }

        public static IfStatementSyntax IfExpressionSyntax(ExpressionSyntax condition, List<StatementSyntax> statements, List<StatementSyntax> elseStatements = null)
        {
            if ((elseStatements == null) || (elseStatements.Count == 0))
            {
                return SyntaxFactory.IfStatement(condition, SyntaxFactory.Block(statements)).NormalizeWhitespace<IfStatementSyntax>("    ", "\r\n", false);
            }
            return SyntaxFactory.IfStatement(condition, SyntaxFactory.Block(statements)).WithElse(SyntaxFactory.ElseClause(SyntaxFactory.Block(elseStatements))).NormalizeWhitespace<IfStatementSyntax>("    ", "\r\n", false);
        }

        public static IfStatementSyntax IfExpressionSyntax(string conditionVariableName, List<StatementSyntax> statements, List<StatementSyntax> elseStatements) => 
            IfExpressionSyntax(SyntaxFactory.IdentifierName(conditionVariableName), statements, elseStatements);

        public static LiteralExpressionSyntax Literal(string typeSignature, string val)
        {
            Type type = MyVisualScriptingProxy.GetType(typeSignature);
            if (type != null)
            {
                if (type == typeof(float))
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(string.IsNullOrEmpty(val) ? 0f : float.Parse(val)));
                }
                if (type == typeof(int))
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(string.IsNullOrEmpty(val) ? 0 : int.Parse(val)));
                }
                if (type == typeof(long))
                {
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(string.IsNullOrEmpty(val) ? 0L : long.Parse(val)));
                }
                if (type == typeof(bool))
                {
                    return ((NormalizeBool(val) != "true") ? SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
                }
            }
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(val));
        }

        public static LocalDeclarationStatementSyntax LocalVariable(string typeData, string variableName, ExpressionSyntax initializer = null)
        {
            VariableDeclaratorSyntax node = SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variableName));
            if (initializer != null)
            {
                node = node.WithInitializer(SyntaxFactory.EqualsValueClause(initializer));
            }
            return SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName((typeData.Length > 0) ? typeData : "var")).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(node))).NormalizeWhitespace<LocalDeclarationStatementSyntax>("    ", "\r\n", false);
        }

        public static LocalDeclarationStatementSyntax LocalVariable(Type type, string varName, ExpressionSyntax initializerExpressionSyntax = null) => 
            SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(GenericTypeSyntax(type), SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(varName)).WithInitializer(SyntaxFactory.EqualsValueClause(initializerExpressionSyntax)))));

        public static MemberDeclarationSyntax MemberDeclaration(string memberName, string memberType) => 
            SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName(memberType)).WithVariables(SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(memberName))))).NormalizeWhitespace<FieldDeclarationSyntax>("    ", "\r\n", false);

        public static MethodDeclarationSyntax MethodDeclaration(MethodInfo method)
        {
            List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
            System.Reflection.ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                System.Reflection.ParameterInfo info = parameters[i];
                Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax item = SyntaxFactory.Parameter(SyntaxFactory.Identifier(info.Name)).WithType(GenericTypeSyntax(info.ParameterType));
                list.Add(item);
                if (i < (parameters.Length - 1))
                {
                    list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }
            }
            SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();
            return SyntaxFactory.MethodDeclaration(GenericTypeSyntax(method.ReturnType), SyntaxFactory.Identifier(method.Name)).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>(list))).WithBody(SyntaxFactory.Block(statements));
        }

        public static InvocationExpressionSyntax MethodInvocation(string methodName, IEnumerable<string> orderedVariableNames, string className = null)
        {
            List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
            if (orderedVariableNames != null)
            {
                foreach (string str in orderedVariableNames)
                {
                    list.Add(CreateArgumentSyntax(str));
                    list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }
            }
            if (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list));
            return (!string.IsNullOrEmpty(className) ? SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(className), SyntaxFactory.IdentifierName(methodName))).WithArgumentList(argumentList) : SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(methodName)).WithArgumentList(argumentList));
        }

        public static InvocationExpressionSyntax MethodInvocation(string methodName, IEnumerable<string> inputVariableNames, IEnumerable<string> outputVarNames, string className = null)
        {
            List<ArgumentSyntax> list = new List<ArgumentSyntax>();
            if (inputVariableNames != null)
            {
                foreach (string str in inputVariableNames)
                {
                    list.Add(CreateArgumentSyntax(str));
                }
            }
            if (outputVarNames != null)
            {
                foreach (string str2 in outputVarNames)
                {
                    list.Add(CreateOutArgumentSyntax(str2));
                }
            }
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list));
            return (!string.IsNullOrEmpty(className) ? SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(className), SyntaxFactory.IdentifierName(methodName))).WithArgumentList(argumentList) : SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(methodName)).WithArgumentList(argumentList));
        }

        public static InvocationExpressionSyntax MethodInvocationExpressionSyntax(IdentifierNameSyntax methodName, ArgumentListSyntax arguments, IdentifierNameSyntax instance = null)
        {
            InvocationExpressionSyntax syntax = null;
            syntax = (instance != null) ? SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, instance, methodName)) : SyntaxFactory.InvocationExpression(methodName);
            return syntax.WithArgumentList(arguments);
        }

        public static ObjectCreationExpressionSyntax NewVector3D(string vectorData)
        {
            char[] separator = new char[] { ' ' };
            string[] textArray1 = vectorData.Split(separator);
            double num = double.Parse(textArray1[0].Replace("X:", ""));
            double num2 = double.Parse(textArray1[1].Replace("Y:", ""));
            double num3 = double.Parse(textArray1[2].Replace("Z:", ""));
            SyntaxNodeOrToken[] tokenArray1 = new SyntaxNodeOrToken[] { SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(num))), SyntaxFactory.Token(SyntaxKind.CommaToken), SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(num2))), SyntaxFactory.Token(SyntaxKind.CommaToken), SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(num3))) };
            return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("VRageMath.Vector3D")).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(tokenArray1)));
        }

        public static ObjectCreationExpressionSyntax NewVector3D(Vector3D vector)
        {
            SyntaxNodeOrToken[] tokenArray1 = new SyntaxNodeOrToken[] { SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(vector.X))), SyntaxFactory.Token(SyntaxKind.CommaToken), SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(vector.Y))), SyntaxFactory.Token(SyntaxKind.CommaToken), SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(vector.Z))) };
            return SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName("VRageMath.Vector3D")).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(tokenArray1)));
        }

        public static string NormalizeBool(string value)
        {
            value = value.ToLower();
            return ((value != "0") ? ((value != "1") ? value : "true") : "false");
        }

        private static List<SyntaxNodeOrToken> Parameters(List<string> parameterNames, List<string> types, bool areOutputs = false)
        {
            List<SyntaxNodeOrToken> list = new List<SyntaxNodeOrToken>();
            for (int i = 0; i < parameterNames.Count; i++)
            {
                string name = parameterNames[i];
                Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax item = null;
                string typeFullName = types[i];
                Type type = MyVisualScriptingProxy.GetType(typeFullName);
                item = (type != null) ? ParameterSyntax(name, type) : ParameterSyntax(name, typeFullName);
                if (areOutputs)
                {
                    item = item.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.OutKeyword)));
                }
                list.Add(item);
                if (i < (parameterNames.Count - 1))
                {
                    list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }
            }
            return list;
        }

        public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax ParameterSyntax(string name, string typeIdentifier) => 
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(name)).WithType(SyntaxFactory.ParseTypeName(typeIdentifier, 0, true));

        public static Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax ParameterSyntax(string name, Type type) => 
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(name)).WithType(GenericTypeSyntax(type));

        public static ClassDeclarationSyntax PublicClass(string name) => 
            SyntaxFactory.ClassDeclaration(name).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).NormalizeWhitespace<ClassDeclarationSyntax>("    ", "\r\n", false);

        public static MethodDeclarationSyntax PublicMethodDeclaration(string methodName, SyntaxKind predefinedReturnType, List<string> inputParameterNames = null, List<string> inputParameterTypes = null, List<string> outputParameterNames = null, List<string> outputParameterTypes = null)
        {
            SyntaxList<StatementSyntax> list2;
            MethodDeclarationSyntax syntax = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(predefinedReturnType)), SyntaxFactory.Identifier(methodName)).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            List<SyntaxNodeOrToken> list = null;
            if (inputParameterNames == null)
            {
                if (outputParameterNames != null)
                {
                    list = Parameters(outputParameterNames, outputParameterTypes, true);
                }
            }
            else
            {
                list = Parameters(inputParameterNames, inputParameterTypes, false);
                if (((outputParameterNames != null) && (outputParameterNames.Count > 0)) && (inputParameterNames.Count > 0))
                {
                    list.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                    list.AddRange(Parameters(outputParameterNames, outputParameterTypes, true));
                }
            }
            if (list != null)
            {
                list2 = new SyntaxList<StatementSyntax>();
                return syntax.WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>(list))).WithBody(SyntaxFactory.Block(list2)).NormalizeWhitespace<MethodDeclarationSyntax>("    ", "\r\n", false);
            }
            list2 = new SyntaxList<StatementSyntax>();
            return syntax.WithBody(SyntaxFactory.Block(list2)).NormalizeWhitespace<MethodDeclarationSyntax>("    ", "\r\n", false);
        }

        public static LocalDeclarationStatementSyntax ReferenceTypeInstantiation(string variableName, string type, params LiteralExpressionSyntax[] values)
        {
            SyntaxNodeOrTokenList nodesAndTokens = SyntaxFactory.NodeOrTokenList();
            for (int i = 0; i < values.Length; i++)
            {
                LiteralExpressionSyntax expression = values[i];
                nodesAndTokens.Add(SyntaxFactory.Argument(expression));
                if ((i + 1) != values.Length)
                {
                    nodesAndTokens.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                }
            }
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(nodesAndTokens));
            ObjectCreationExpressionSyntax initializer = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(type)).WithArgumentList(argumentList);
            return LocalVariable(type, variableName, initializer);
        }

        public static ExpressionStatementSyntax SimpleAssignment(string variableName, ExpressionSyntax rightSide) => 
            SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), rightSide)).NormalizeWhitespace<ExpressionStatementSyntax>("    ", "\r\n", false);

        public static UsingDirectiveSyntax UsingStatementSyntax(string @namespace)
        {
            char[] separator = new char[] { '.' };
            string[] strArray = @namespace.Split(separator);
            if (strArray.Length < 2)
            {
                return SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(strArray[0]));
            }
            QualifiedNameSyntax left = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(strArray[0]), SyntaxFactory.IdentifierName(strArray[1]));
            for (int i = 2; i < strArray.Length; i++)
            {
                left = SyntaxFactory.QualifiedName(left, SyntaxFactory.IdentifierName(strArray[i]));
            }
            return SyntaxFactory.UsingDirective(left);
        }

        public static AssignmentExpressionSyntax VariableAssignment(string variableName, ExpressionSyntax rightSide) => 
            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), rightSide);

        public static ExpressionStatementSyntax VariableAssignmentExpression(string variableName, string value, SyntaxKind expressionKind)
        {
            SyntaxToken token;
            bool flag = false;
            if (expressionKind == SyntaxKind.StringLiteralExpression)
            {
                token = SyntaxFactory.Literal(value);
            }
            else
            {
                if ((expressionKind == SyntaxKind.TrueLiteralExpression) || (expressionKind == SyntaxKind.FalseLiteralExpression))
                {
                    return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), SyntaxFactory.LiteralExpression(expressionKind))).NormalizeWhitespace<ExpressionStatementSyntax>("    ", "\r\n", false);
                }
                if (value.Contains<char>('-'))
                {
                    flag = true;
                    value = value.Replace("-", "");
                }
                token = SyntaxFactory.ParseToken(value, 0);
            }
            LiteralExpressionSyntax right = SyntaxFactory.LiteralExpression(expressionKind, token);
            return (!flag ? SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), right)).NormalizeWhitespace<ExpressionStatementSyntax>("    ", "\r\n", false) : SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), SyntaxFactory.PrefixUnaryExpression(SyntaxKind.UnaryMinusExpression, right))).NormalizeWhitespace<ExpressionStatementSyntax>("    ", "\r\n", false));
        }

        private static ArgumentSyntax VectorArgumentSyntax(double value) => 
            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)));

        public static ExpressionStatementSyntax VectorAssignmentExpression(string variableName, string vectorType, double x, double y, double z)
        {
            ArgumentSyntax syntax = VectorArgumentSyntax(x);
            ArgumentSyntax syntax2 = VectorArgumentSyntax(y);
            ArgumentSyntax syntax3 = VectorArgumentSyntax(z);
            SyntaxNodeOrToken[] tokenArray1 = new SyntaxNodeOrToken[] { syntax, SyntaxFactory.Token(SyntaxKind.CommaToken), syntax2, SyntaxFactory.Token(SyntaxKind.CommaToken), syntax3 };
            ArgumentListSyntax argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(tokenArray1));
            ObjectCreationExpressionSyntax right = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(vectorType)).WithArgumentList(argumentList);
            return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(variableName), right)).NormalizeWhitespace<ExpressionStatementSyntax>("    ", "\r\n", false);
        }
    }
}

