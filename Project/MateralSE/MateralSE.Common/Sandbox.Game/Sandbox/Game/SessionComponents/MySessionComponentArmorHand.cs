System.InvalidOperationException: Inner optimization dead-lock.
   在 Reflector.Disassembler.Optimizer.TransformMethodDeclaration(IMethodDeclaration value)
   在 Reflector.Disassembler.Disassembler.TransformMethodDeclaration(IMethodDeclaration value)
   在 Reflector.CodeModel.Visitor.Transformer.TransformMethodDeclarationCollection(IMethodDeclarationCollection methods)
   在 Reflector.Disassembler.Disassembler.TransformTypeDeclaration(ITypeDeclaration value)
   在 Reflector.Application.Translator.TranslateTypeDeclaration(ITypeDeclaration value, Boolean memberDeclarationList, Boolean methodDeclarationBody)
   在 Reflector.Application.FileDisassembler.WriteTypeDeclaration(ITypeDeclaration typeDeclaration, String sourceFile, ILanguageWriterConfiguration languageWriterConfiguration)
namespace Sandbox.Game.SessionComponents
{
}

