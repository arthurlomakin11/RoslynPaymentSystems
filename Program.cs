using Buildalyzer;
using Buildalyzer.Workspaces;

class MainProject
{
    private static string _solutionPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\PaymentProvider.sln";
    private static string _projectPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\src\PaymentSystems\PaymentSystem.AdyenCreditCard\PaymentSystem.AdyenCreditCard.csproj";

    private static async Task GetProjects()
    {
        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(_projectPath);
        var result = analyzer.Build();
        var workspace = analyzer.GetWorkspace();
        // var workspace = MSBuildWorkspace.Create();
        // var currSolution = await workspace.OpenSolutionAsync(_solutionPath);
        // var projects = currSolution.Projects;
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