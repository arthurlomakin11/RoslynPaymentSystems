namespace RoslynPaymentSystems;

public class SolutionInitializer
{
    private static readonly string SolutionPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\PaymentProvider.sln";
    private static readonly string PaymentSystemsPath = @"C:\Users\Artur\RiderProjects\PaymentsProvider2\src\PaymentSystems";
    private MSBuildWorkspace? _workspace;
    private Solution? _solution;
    
    public async Task<IEnumerable<Document>> GetAllPaymentSystemsDocuments()
    {
        MSBuildLocator.RegisterDefaults();
        _workspace = MSBuildWorkspace.Create();
        
        _solution = await _workspace.OpenSolutionAsync(SolutionPath);
        var projects = _solution.Projects.Where(p => p.Name.Contains("PaymentSystem."));
        return projects.SelectMany(p => p.Documents);
    }
    
    public bool SaveChanges()
    {
        var saveChanges = _workspace!.TryApplyChanges(_solution!);
        return saveChanges;
    }
    
    public void WithDocumentRoot(DocumentId documentId, SyntaxNode root)
    {
        _solution = _solution!.WithDocumentSyntaxRoot(documentId, root);
    }
}