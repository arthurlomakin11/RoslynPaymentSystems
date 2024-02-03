using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

class MainProject
{
    private static string _solutionPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\PaymentProvider.sln";
    private static string _projectPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\src\PaymentSystems\PaymentSystem.AdyenCreditCard\PaymentSystem.AdyenCreditCard.csproj";

    private static async Task GetProjects()
    {
        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();
        var currSolution = await workspace.OpenSolutionAsync(_solutionPath);
        var project = currSolution.Projects.First();
        foreach (var document in project.Documents)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var root = await syntaxTree?.GetRootAsync()!;
            var compRoot = root as CompilationUnitSyntax;
        }
        var projects = await project.GetCompilationAsync();
        //var currProject = await workspace.OpenProjectAsync(projectPath);
        //Console.WriteLine(string.Join('\n', projects.Select(p => p.Name)));
        //ProjectAnalysis(currProject);
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