using ExpenseFlow.Application.ExpenseReports.Reporting;
using ExpenseFlow.Domain.ExpenseReports;

namespace ExpenseFlow.Application.Abstractions;

public interface IExpenseReportGenerator
{
    ExpenseReport Generate(ExpenseReportGenerationInput input);
}
