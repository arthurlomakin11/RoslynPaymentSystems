using Microsoft.CodeAnalysis.CSharp;

namespace RoslynPaymentSystems;

public class FluentGenericsProcessor
{
    private readonly SolutionInitializer _solutionInitializer = new();
    
    public async Task ReplacePaymentSystemWithFluentGeneric()
    {
        var documents = await _solutionInitializer.GetAllPaymentSystemsDocuments();
        var classDeclarationsList = await Task.WhenAll(
            documents.Select(async document =>
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var root = await syntaxTree?.GetRootAsync()!;
                var compRoot = root as CompilationUnitSyntax;
                return compRoot!
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(declaration => declaration.BaseList != null 
                            && declaration.BaseList.Types.ToFullString().Contains("PaymentSystemBase<")
                    )
                    .Select(declaration => (declaration, document, root));
            })
        );
        var classDeclarations = classDeclarationsList.SelectMany(c => c).ToList();
        
        foreach (var (classDeclarationSyntax, document, root) in classDeclarations)
        {
            var declarationNode = classDeclarationSyntax.BaseList!.Types.First();
            
            // PaymentSystemBase<
            //  BasePayInPaymentSystemSettings,
            //  BasePayOutPaymentSystemSettings,
            //  BaseRefundPaymentSystemSettings,
            //  PaymentSystemBaseCashierContract,
            //  PaymentSystemBasePayOutContract
            // >
            var declarationString = declarationNode.ChildNodesAndTokens().First().ToFullString();
            var cleanGenericArgumentsString = declarationString
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(" ", "")
                .Replace("PaymentSystemBase<", "")
                .Replace(">", "");
            var paymentSystemBaseGenericArguments = cleanGenericArgumentsString.Split(',');
            
            var payInSettingsType = paymentSystemBaseGenericArguments[0];
            var payOutSettingsType = paymentSystemBaseGenericArguments[1];
            var refundSettingsType = paymentSystemBaseGenericArguments[2];
            var payInCashierContractType = paymentSystemBaseGenericArguments[3];
            var payOutCashierContractType = paymentSystemBaseGenericArguments[4];

            List<string> newDeclarationList =
            [
                payInSettingsType == "BasePayInPaymentSystemSettings"
                && payInCashierContractType == "PaymentSystemBaseCashierContract"
                    ? ".WithoutPayIn"
                    : $".WithPayIn<{payInSettingsType}, {payInCashierContractType}>",
                
                payOutSettingsType == "BasePayOutPaymentSystemSettings"
                && payOutCashierContractType == "PaymentSystemBasePayOutContract"
                    ? ".WithoutPayOut"
                    : $".WithPayOut<{payOutSettingsType}, {payOutCashierContractType}>",

                refundSettingsType == "BaseRefundPaymentSystemSettings"
                    ? ".WithoutRefund"
                    : $".WithRefund<{refundSettingsType}>"
            ];

            var newArgumentsListStringWithPadding = string.Join(null, newDeclarationList.Select(s => "\r\n    " + s));
            var newDeclarationString = "PaymentSystemBase" + newArgumentsListStringWithPadding + "\n";
            var newDeclaration = SyntaxFactory.SimpleBaseType(
                SyntaxFactory.ParseName(newDeclarationString)
            );
            
            var newRoot = root.ReplaceNode(declarationNode, newDeclaration);
            _solutionInitializer.WithDocumentRoot(document.Id, newRoot);
        }
        
        var saved = _solutionInitializer.SaveChanges();
    }
}