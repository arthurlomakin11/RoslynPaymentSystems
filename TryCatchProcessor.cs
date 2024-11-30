namespace RoslynPaymentSystems;

public class TryCatchProcessor
{
    public static async Task GetProjects()
    {
        var solutionInitializer = new SolutionInitializer();
        var documents = await solutionInitializer.GetAllPaymentSystemsDocuments();
        
        var catchOrderedStrings = new List<string>();
        var catchOrderedUnorderedStringsDictionary = new Dictionary<string, string>();
        
        foreach (var document in documents)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var root = await syntaxTree?.GetRootAsync()!;
            var compRoot = root as CompilationUnitSyntax;
            compRoot!
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(methodDeclaration => methodDeclaration.Identifier.Text
                    is "CreatePayInRequestAsync" 
                    or "ProcessPayInCallbackAsync" 
                    or "CreatePayOutRequestAsync"
                    or "ProcessPayOutCallbackAsync"
                    or "FetchPayInPreAuthDataAsync"
                )
                .ToList()
                .ForEach(methodDeclaration =>
                {
                    var methodDeclarationBody = methodDeclaration.Body;
                    if (methodDeclarationBody is null) return;
                        
                    var tryStatements = methodDeclarationBody.Statements.OfType<TryStatementSyntax>();
                    var tryStatement = tryStatements.FirstOrDefault();
                    tryStatement?.Catches
                        .ToList()
                        .ForEach(catchSyntax =>
                        {
                            var catchSyntaxString = catchSyntax.ToFullString();
                                
                            var catchOrdered = catchSyntax.Block.Statements
                                .OrderBy(s => s.ToFullString());
                            var catchOrderedString = string.Join("\n", catchOrdered);
                            var catchOrderedProcessedString = ProcessString(catchOrderedString);
                            catchOrderedStrings.Add(catchOrderedProcessedString);
                            
                            catchOrderedUnorderedStringsDictionary.TryAdd(catchOrderedProcessedString, catchSyntaxString);
                        });
                });
        }
        
        var catchOrderedStringDistinct = ProcessStringsList(catchOrderedStrings);

        var unorderedProcessedStringList = catchOrderedStringDistinct
            .Select(s => catchOrderedUnorderedStringsDictionary[s])
            .ToList();
    }

    private static string ProcessString(string @string)
    {
        var replacedSymbolsString = @string.Trim()
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(";", ";\n")
                .Replace("processingResponse", "response")
                .Replace("paymentResponse", "response");
            
        var splitString = replacedSymbolsString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var removedEmptySymbols = string.Join(' ', splitString);
        return removedEmptySymbols;
    }
    
    private static List<string> ProcessStringsList(IEnumerable<string> list) =>
        list
            .Select(ProcessString)
            .ToHashSet()
            .OrderBy(el => el)
            .ToList();
}