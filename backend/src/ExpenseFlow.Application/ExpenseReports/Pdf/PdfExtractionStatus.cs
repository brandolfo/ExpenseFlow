namespace ExpenseFlow.Application.ExpenseReports.Pdf;

public enum PdfExtractionStatus
{
    Succeeded,
    Partial,
    UnsupportedStatementShape,
    UnsupportedScannedImageOnly,
    UnsupportedEncrypted,
    Failed
}
