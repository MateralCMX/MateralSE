namespace VRage.Game.VisualScripting
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.Game.VisualScripting.ScriptBuilder;
    using VRage.Game.VisualScripting.ScriptBuilder.Nodes;
    using VRage.Game.VisualScripting.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyVisualScriptBuilder
    {
        private string m_scriptFilePath;
        private string m_scriptName;
        private MyObjectBuilder_VisualScript m_objectBuilder;
        private Type m_baseType;
        private CompilationUnitSyntax m_compilationUnit;
        private MyVisualScriptNavigator m_navigator;
        private ClassDeclarationSyntax m_scriptClassDeclaration;
        private ConstructorDeclarationSyntax m_constructor;
        private MethodDeclarationSyntax m_disposeMethod;
        private NamespaceDeclarationSyntax m_namespaceDeclaration;
        private readonly List<MemberDeclarationSyntax> m_fieldDeclarations = new List<MemberDeclarationSyntax>();
        private readonly List<MethodDeclarationSyntax> m_methodDeclarations = new List<MethodDeclarationSyntax>();
        private readonly List<StatementSyntax> m_helperStatementList = new List<StatementSyntax>();
        private readonly MyVisualSyntaxBuilderNode m_builderNode = new MyVisualSyntaxBuilderNode();

        private void AddMissingInterfaceMethods()
        {
            if ((this.m_baseType != null) && this.m_baseType.IsInterface)
            {
                foreach (MethodInfo info in this.m_baseType.GetMethods())
                {
                    bool flag = false;
                    using (List<MethodDeclarationSyntax>.Enumerator enumerator = this.m_methodDeclarations.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            SyntaxToken identifier = enumerator.Current.Identifier;
                            if (identifier.ToFullString() == info.Name)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        VisualScriptingMember customAttribute = info.GetCustomAttribute<VisualScriptingMember>();
                        if (((customAttribute != null) && !customAttribute.Reserved) && !info.IsSpecialName)
                        {
                            this.m_methodDeclarations.Add(MySyntaxFactory.MethodDeclaration(info));
                        }
                    }
                }
            }
        }

        private void AddMissionLogicScriptMethods()
        {
            MethodDeclarationSyntax item = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)), SyntaxFactory.Identifier("GetOwnerId")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("OwnerId")))));
            AccessorDeclarationSyntax[] syntaxArray1 = new AccessorDeclarationSyntax[] { SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)) };
            PropertyDeclarationSyntax syntax2 = SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)), SyntaxFactory.Identifier("OwnerId")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(syntaxArray1)));
            AccessorDeclarationSyntax[] syntaxArray2 = new AccessorDeclarationSyntax[] { SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, null).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)) };
            PropertyDeclarationSyntax syntax3 = SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), SyntaxFactory.Identifier("TransitionTo")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List<AccessorDeclarationSyntax>(syntaxArray2)));
            MethodDeclarationSyntax syntax4 = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), SyntaxFactory.Identifier("Complete")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList<ParameterSyntax>(SyntaxFactory.Parameter(SyntaxFactory.Identifier("transitionName")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))).WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Completed"))))))).WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("TransitionTo"), SyntaxFactory.IdentifierName("transitionName"))))));
            this.m_methodDeclarations.Add(syntax4);
            this.m_fieldDeclarations.Add(syntax3);
            this.m_fieldDeclarations.Add(syntax2);
            this.m_methodDeclarations.Add(item);
        }

        public bool Build()
        {
            if (string.IsNullOrEmpty(this.m_scriptFilePath))
            {
                return false;
            }
            try
            {
                this.Clear();
                this.CreateClassSyntax();
                this.CreateDisposeMethod();
                this.CreateVariablesAndConstructorSyntax();
                this.CreateScriptInstances();
                this.CreateMethods();
                this.CreateNamespaceDeclaration();
                this.FinalizeSyntax();
            }
            catch (Exception exception)
            {
                string msg = "Script: " + this.m_scriptName + " failed to build. Error message: " + exception.Message;
                MyLog.Default.WriteLine(msg);
                MyLog.Default.WriteLine(exception);
                this.ErrorMessage = msg;
                return false;
            }
            return true;
        }

        private void Clear()
        {
            this.m_fieldDeclarations.Clear();
            this.m_methodDeclarations.Clear();
        }

        private void CreateClassSyntax()
        {
            IdentifierNameSyntax type = SyntaxFactory.IdentifierName("IMyLevelScript");
            if (!(this.m_objectBuilder is MyObjectBuilder_VisualLevelScript))
            {
                type = string.IsNullOrEmpty(this.m_objectBuilder.Interface) ? null : SyntaxFactory.IdentifierName(this.m_baseType.Name);
            }
            this.m_scriptClassDeclaration = MySyntaxFactory.PublicClass(this.m_scriptName);
            if (type != null)
            {
                this.m_scriptClassDeclaration = this.m_scriptClassDeclaration.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(SyntaxFactory.SimpleBaseType(type))));
            }
        }

        private void CreateDisposeMethod()
        {
            SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();
            this.m_disposeMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), SyntaxFactory.Identifier("Dispose")).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))).WithBody(SyntaxFactory.Block(statements));
        }

        private void CreateMethods()
        {
            if (!string.IsNullOrEmpty(this.m_objectBuilder.Interface))
            {
                foreach (MyVisualSyntaxInterfaceMethodNode node in this.m_navigator.OfType<MyVisualSyntaxInterfaceMethodNode>())
                {
                    MethodDeclarationSyntax methodDeclaration = node.GetMethodDeclaration();
                    MyVisualSyntaxInterfaceMethodNode[] nodes = new MyVisualSyntaxInterfaceMethodNode[] { node };
                    this.ProcessNodes(nodes, ref methodDeclaration, null);
                    this.m_methodDeclarations.Add(methodDeclaration);
                }
            }
            List<MyVisualSyntaxEventNode> list = this.m_navigator.OfType<MyVisualSyntaxEventNode>();
            list.AddRange(this.m_navigator.OfType<MyVisualSyntaxKeyEventNode>());
            while (list.Count > 0)
            {
                MyVisualSyntaxEventNode firstEvent = list[0];
                IEnumerable<MyVisualSyntaxEventNode> eventsWithSameName = from @event in list
                    where @event.ObjectBuilder.Name == firstEvent.ObjectBuilder.Name
                    select @event;
                MethodDeclarationSyntax methodDeclaration = MySyntaxFactory.PublicMethodDeclaration(firstEvent.EventName, SyntaxKind.VoidKeyword, firstEvent.ObjectBuilder.OutputNames, firstEvent.ObjectBuilder.OuputTypes, null, null);
                this.ProcessNodes(eventsWithSameName, ref methodDeclaration, null);
                StatementSyntax[] items = new StatementSyntax[] { MySyntaxFactory.DelegateAssignment(firstEvent.ObjectBuilder.Name, methodDeclaration.Identifier.ToString()) };
                this.m_constructor = this.m_constructor.AddBodyStatements(items);
                SyntaxToken identifier = methodDeclaration.Identifier;
                StatementSyntax[] syntaxArray2 = new StatementSyntax[] { MySyntaxFactory.DelegateRemoval(firstEvent.ObjectBuilder.Name, identifier.ToString()) };
                this.m_disposeMethod = this.m_disposeMethod.AddBodyStatements(syntaxArray2);
                this.m_methodDeclarations.Add(methodDeclaration);
                list.RemoveAll(@event => eventsWithSameName.Contains<MyVisualSyntaxEventNode>(@event));
            }
            List<MyVisualSyntaxInputNode> list2 = this.m_navigator.OfType<MyVisualSyntaxInputNode>();
            List<MyVisualSyntaxOutputNode> list3 = this.m_navigator.OfType<MyVisualSyntaxOutputNode>();
            if (list2.Count > 0)
            {
                MyVisualSyntaxInputNode node2 = list2[0];
                MethodDeclarationSyntax methodDeclaration = null;
                if (list3.Count <= 0)
                {
                    methodDeclaration = MySyntaxFactory.PublicMethodDeclaration("RunScript", SyntaxKind.BoolKeyword, node2.ObjectBuilder.OutputNames, node2.ObjectBuilder.OuputTypes, null, null);
                }
                else
                {
                    List<string> outputParameterNames = new List<string>(list3[0].ObjectBuilder.Inputs.Count);
                    List<string> outputParameterTypes = new List<string>(list3[0].ObjectBuilder.Inputs.Count);
                    foreach (MyInputParameterSerializationData data in list3[0].ObjectBuilder.Inputs)
                    {
                        outputParameterNames.Add(data.Name);
                        outputParameterTypes.Add(data.Type);
                    }
                    methodDeclaration = MySyntaxFactory.PublicMethodDeclaration("RunScript", SyntaxKind.BoolKeyword, node2.ObjectBuilder.OutputNames, node2.ObjectBuilder.OuputTypes, outputParameterNames, outputParameterTypes);
                }
                MyVisualSyntaxInputNode[] nodes = new MyVisualSyntaxInputNode[] { node2 };
                ReturnStatementSyntax[] statementsToAppend = new ReturnStatementSyntax[] { SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)) };
                this.ProcessNodes(nodes, ref methodDeclaration, statementsToAppend);
                this.m_methodDeclarations.Add(methodDeclaration);
            }
        }

        private void CreateNamespaceDeclaration()
        {
            this.m_namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("VisualScripting.CustomScripts"));
        }

        private void CreateScriptInstances()
        {
            List<MyVisualSyntaxScriptNode> list = this.m_navigator.OfType<MyVisualSyntaxScriptNode>();
            if (list != null)
            {
                foreach (MyVisualSyntaxScriptNode node in list)
                {
                    this.m_fieldDeclarations.Add(node.InstanceDeclaration());
                    StatementSyntax[] items = new StatementSyntax[] { node.DisposeCallDeclaration() };
                    this.m_disposeMethod = this.m_disposeMethod.AddBodyStatements(items);
                }
            }
        }

        private void CreateVariablesAndConstructorSyntax()
        {
            this.m_constructor = MySyntaxFactory.Constructor(this.m_scriptClassDeclaration);
            foreach (MyVisualSyntaxVariableNode node in this.m_navigator.OfType<MyVisualSyntaxVariableNode>())
            {
                this.m_fieldDeclarations.Add(node.CreateFieldDeclaration());
                StatementSyntax[] items = new StatementSyntax[] { node.CreateInitializationSyntax() };
                this.m_constructor = this.m_constructor.AddBodyStatements(items);
            }
        }

        private void FinalizeSyntax()
        {
            bool flag = false;
            int num = 0;
            while (true)
            {
                if (num < this.m_methodDeclarations.Count)
                {
                    SyntaxToken identifier = this.m_disposeMethod.Identifier;
                    if (this.m_methodDeclarations[num].Identifier.ToString() != identifier.ToString())
                    {
                        num++;
                        continue;
                    }
                    if (this.m_disposeMethod.Body.Statements.Count > 0)
                    {
                        StatementSyntax[] syntaxArray1 = new StatementSyntax[] { this.m_disposeMethod.Body };
                        this.m_methodDeclarations[num] = this.m_methodDeclarations[num].AddBodyStatements(syntaxArray1);
                    }
                    flag = true;
                }
                if (!flag)
                {
                    this.m_methodDeclarations.Add(this.m_disposeMethod);
                }
                this.AddMissingInterfaceMethods();
                if (this.m_baseType == typeof(IMyStateMachineScript))
                {
                    this.AddMissionLogicScriptMethods();
                }
                this.m_scriptClassDeclaration = this.m_scriptClassDeclaration.AddMembers(this.m_fieldDeclarations.ToArray());
                MemberDeclarationSyntax[] items = new MemberDeclarationSyntax[] { this.m_constructor };
                this.m_scriptClassDeclaration = this.m_scriptClassDeclaration.AddMembers(items);
                this.m_scriptClassDeclaration = this.m_scriptClassDeclaration.AddMembers(this.m_methodDeclarations.ToArray());
                MemberDeclarationSyntax[] syntaxArray3 = new MemberDeclarationSyntax[] { this.m_scriptClassDeclaration };
                this.m_namespaceDeclaration = this.m_namespaceDeclaration.AddMembers(syntaxArray3);
                List<UsingDirectiveSyntax> list = new List<UsingDirectiveSyntax>();
                HashSet<string> set = new HashSet<string>();
                UsingDirectiveSyntax item = MySyntaxFactory.UsingStatementSyntax("VRage.Game.VisualScripting");
                UsingDirectiveSyntax syntax2 = MySyntaxFactory.UsingStatementSyntax("System.Collections.Generic");
                list.Add(item);
                list.Add(syntax2);
                set.Add(item.ToFullString());
                set.Add(syntax2.ToFullString());
                foreach (MyVisualSyntaxFunctionNode node in this.m_navigator.OfType<MyVisualSyntaxFunctionNode>())
                {
                    if (set.Add(node.Using.ToFullString()))
                    {
                        list.Add(node.Using);
                    }
                }
                foreach (MyVisualSyntaxVariableNode node2 in this.m_navigator.OfType<MyVisualSyntaxVariableNode>())
                {
                    if (set.Add(node2.Using.ToFullString()))
                    {
                        list.Add(node2.Using);
                    }
                }
                MemberDeclarationSyntax[] syntaxArray4 = new MemberDeclarationSyntax[] { this.m_namespaceDeclaration };
                this.m_compilationUnit = SyntaxFactory.CompilationUnit().WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>(list)).AddMembers(syntaxArray4).NormalizeWhitespace<CompilationUnitSyntax>("    ", "\r\n", false);
                return;
            }
        }

        public bool Load()
        {
            MyObjectBuilder_VSFiles files;
            bool flag;
            if (string.IsNullOrEmpty(this.m_scriptFilePath))
            {
                return false;
            }
            using (Stream stream = MyFileSystem.OpenRead(this.m_scriptFilePath))
            {
                if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(stream, out files))
                {
                    this.ErrorMessage = "Deserialization failed : " + this.m_scriptFilePath;
                    return false;
                }
            }
            try
            {
                this.ErrorMessage = string.Empty;
                if (files.LevelScript != null)
                {
                    this.m_objectBuilder = files.LevelScript;
                }
                else if (files.VisualScript != null)
                {
                    this.m_objectBuilder = files.VisualScript;
                }
                this.m_navigator = new MyVisualScriptNavigator(this.m_objectBuilder);
                this.m_scriptName = this.m_objectBuilder.Name;
                if (this.m_objectBuilder.Interface != null)
                {
                    this.m_baseType = MyVisualScriptingProxy.GetType(this.m_objectBuilder.Interface);
                }
                return true;
            }
            catch (Exception exception)
            {
                string msg = "Error occured during the graph reconstruction: " + exception;
                MyLog.Default.WriteLine(msg);
                MyLog.Default.WriteLine(exception);
                this.ErrorMessage = msg;
                flag = false;
            }
            return flag;
        }

        private void ProcessNodes(IEnumerable<MyVisualSyntaxNode> nodes, ref MethodDeclarationSyntax methodDeclaration, IEnumerable<StatementSyntax> statementsToAppend = null)
        {
            this.m_helperStatementList.Clear();
            this.m_navigator.ResetNodes();
            this.m_builderNode.Reset();
            this.m_builderNode.SequenceOutputs.AddRange(nodes);
            this.m_builderNode.Navigator = this.m_navigator;
            using (IEnumerator<MyVisualSyntaxNode> enumerator = nodes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ((IMyVisualSyntaxEntryPoint) enumerator.Current).AddSequenceInput(this.m_builderNode);
                }
            }
            this.m_builderNode.Preprocess();
            this.m_builderNode.CollectSequenceExpressions(this.m_helperStatementList);
            if (statementsToAppend != null)
            {
                this.m_helperStatementList.AddRange(statementsToAppend);
            }
            methodDeclaration = methodDeclaration.AddBodyStatements(this.m_helperStatementList.ToArray());
        }

        public string Syntax =>
            this.m_compilationUnit.ToFullString().Replace(@"\\n", @"\n");

        public string ScriptName =>
            this.m_scriptName;

        public List<string> Dependencies =>
            this.m_objectBuilder.DependencyFilePaths;

        public string ScriptFilePath
        {
            get => 
                this.m_scriptFilePath;
            set => 
                (this.m_scriptFilePath = value);
        }

        public string ErrorMessage { get; set; }
    }
}

