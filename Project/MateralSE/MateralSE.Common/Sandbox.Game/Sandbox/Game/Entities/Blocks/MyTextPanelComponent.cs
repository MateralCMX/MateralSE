﻿System.ArgumentException: 已添加项。字典中的关键字:“T {184}x”所添加的关键字:“T {184}x”
   在 System.Collections.Hashtable.Insert(Object key, Object nvalue, Boolean add)
   在 Reflector.CodeModel.Visitor.Cloner.TransformVariableDeclaration(IVariableDeclaration value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformVariableDeclarationCollection(IVariableDeclarationCollection value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformLambdaExpression(ILambdaExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpression(IExpression value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformAssignExpression(IAssignExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpression(IExpression value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformNullCoalescingExpression(INullCoalescingExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpression(IExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpressionCollection(ExpressionCollection expressions)
   在 Reflector.CodeModel.Visitor.Cloner.TransformObjectCreateExpression(IObjectCreateExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpression(IExpression value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformAssignExpression(IAssignExpression value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformExpression(IExpression value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformExpressionStatement(IExpressionStatement value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformStatement(IStatement value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformStatementCollection(StatementCollection value)
   在 Reflector.CodeModel.Visitor.Cloner.TransformBlockStatement(IBlockStatement value)
   在 Reflector.Disassembler.Optimizer.TransformMethodDeclaration(IMethodDeclaration value)
   在 Reflector.Disassembler.Disassembler.TransformMethodDeclaration(IMethodDeclaration value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformMethodDeclarationCollection(IMethodDeclarationCollection methods)
   在 Reflector.Disassembler.Disassembler.TransformTypeDeclaration(ITypeDeclaration value)
   在 Reflector.Application.Translator.TranslateTypeDeclaration(ITypeDeclaration value, Boolean memberDeclarationList, Boolean methodDeclarationBody)
   在 Reflector.Application.FileDisassembler.WriteTypeDeclaration(ITypeDeclaration typeDeclaration, String sourceFile, ILanguageWriterConfiguration languageWriterConfiguration)
namespace Sandbox.Game.Entities.Blocks
{
}

