namespace RoslynPaymentSystems;

public class SolutionInitializer
{
    private static readonly string SolutionPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\PaymentProvider.sln";
    private static readonly string PaymentSystemsPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\src\PaymentSystems";
    
    public async Task<IEnumerable<Document>> GetAllPaymentSystemsDocuments()
    {
        MSBuildLocator.RegisterDefaults();
        var workspace = MSBuildWorkspace.Create();
        
        var solution = await workspace.OpenSolutionAsync(SolutionPath);
        var projects = solution.Projects.Where(p => p.Name.Contains("PaymentSystem."));
        return projects.SelectMany(p => p.Documents);
    }
}