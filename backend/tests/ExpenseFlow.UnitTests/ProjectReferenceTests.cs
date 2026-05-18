using ExpenseFlow.Application.Common;
using ExpenseFlow.Domain.Common;

namespace ExpenseFlow.UnitTests;

public sealed class ProjectReferenceTests
{
    [Fact]
    public void UnitTestProjectCanReferenceApplicationAndDomainAssemblies()
    {
        Assert.Equal("ExpenseFlow.Application", typeof(ApplicationAssemblyMarker).Assembly.GetName().Name);
        Assert.Equal("ExpenseFlow.Domain", typeof(DomainAssemblyMarker).Assembly.GetName().Name);
    }
}
