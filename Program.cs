namespace RoslynPaymentSystems;

public class MainProject
{
    private static async Task Main()
    {
        var processor = new FluentGenericsProcessor();
        await processor.ReplacePaymentSystemWithFluentGeneric();
    }
}