using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

class MainProject
{
    private static string _solutionPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\PaymentProvider.sln";
    private static readonly string _paymentSystemsPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\src\PaymentSystems";

    private static async Task GetProjects()
    {
        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();

        var files = Directory
            .GetFiles(_paymentSystemsPath, "*.csproj", SearchOption.AllDirectories)
            .OrderBy(file => file)
            .Where(file => !file.Contains("_Base"))
            .ToList();

        var catchStrings = new List<string>();
        var catchOrderedStrings = new List<string>();
        
        foreach (var filePath in files)
        {
            var project = await workspace.OpenProjectAsync(filePath);
            // var compilation = await project.GetCompilationAsync();
            // compilation.getSy
            foreach (var document in project.Documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var root = await syntaxTree?.GetRootAsync()!;
                var compRoot = root as CompilationUnitSyntax;
                var namespaceDeclarations = compRoot!.Members
                    .OfType<FileScopedNamespaceDeclarationSyntax>()
                    .ToList();
                namespaceDeclarations.ForEach(namespaceDeclaration =>
                {
                    namespaceDeclaration.Members
                        .OfType<ClassDeclarationSyntax>()
                        .ToList()
                        .ForEach(classDeclaration =>
                        {
                            classDeclaration.Members
                                .OfType<MethodDeclarationSyntax>()
                                .Where(methodDeclaration => methodDeclaration.Identifier.Text 
                                    is "CreatePayInRequestAsync" or "ProcessPayInCallbackAsync")
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
                                            catchStrings.Add(catchSyntaxString);
                                            
                                            
                                            var catchOrdered = catchSyntax.Block.Statements
                                                .OrderBy(s => s.ToFullString());
                                            var catchOrderedString = string.Join("\n", catchOrdered);
                                            
                                            catchOrderedStrings.Add(catchOrderedString);
                                        });
                                });
                        });
                });
            }
            // var compilation = await project.GetCompilationAsync();
            //ProjectAnalysis(currProject);
        }

        var catchStringHashSet = ProcessStrings(catchStrings);
        
        var catchOrderedStringHashSet = ProcessStrings(catchOrderedStrings);
        
        Console.WriteLine($"catchStrings.Count: {catchStrings.Count}");
        Console.WriteLine($"catchStringHashSet.Count: {catchStringHashSet.Count}");
    }

    private static List<string> ProcessStrings(IEnumerable<string> list)
    {
        return list
            .Select(el => el.Trim())
            .Select(el => el.Replace("\r", string.Empty))
            .Select(el => el.Replace("\n", string.Empty))
            .Select(el => el.Replace(";", ";\n"))
            .Select(el => el.Replace("processingResponse", "response"))
            .Select(el => el.Replace("paymentResponse", "response"))
            .Select(el =>
            {
                var splitString = el.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return string.Join(' ', splitString);
            })
            .ToHashSet()
            .OrderBy(el => el)
            .ToList();
    }

    private static string? Difference(string? str1, string? str2)
    {
        if (str1 == null)
        {
            return str2;
        }
        if (str2 == null)
        {
            return str1;
        }
    
        var set1 = str1.Split(' ').Distinct().ToList();
        var set2 = str2.Split(' ').Distinct().ToList();

        var diff = set2.Count > set1.Count ? set2.Except(set1).ToList() : set1.Except(set2).ToList();

        return string.Join("", diff);
    }

    // static async void ProjectAnalysis(Project project)
    // {
    //     var compilation = project.GetCompilationAsync().Result;
    //     foreach (var file in project.Documents)
    //     {
    //         var tree = file.GetSyntaxTreeAsync().Result;
    //         var model = compilation.GetSemanticModel(tree);
    //     
    //     
    //     }
    // }

    private static async Task Main()
    {
        await GetProjects();
    }
}