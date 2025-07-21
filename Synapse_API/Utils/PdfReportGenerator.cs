using DocumentFormat.OpenXml.Bibliography;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Synapse_API.Models.Dto.LearningAnalysisDTOs;
using Synapse_API.Models.Dto.LearningReportDto;

namespace Synapse_API.Utils
{
    public static class PdfReportGenerator
    {
        public static byte[] GenerateReportPdf(List<EnhancedLearningReportDto> reports)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    // Header with logo and title
                    page.Header().AlignCenter().Column(col =>
                    {
                        col.Item().Text("Learning Analytics Report")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().Text($"Generated on: {DateTime.Now:dddd, dd MMMM yyyy}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium);
                    });

                    // Footer with page numbers
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ").FontSize(10).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(10).FontColor(Colors.Grey.Medium);
                        text.Span(" of ").FontSize(10).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {


                        // User reports
                        foreach (var report in reports)
                        {
                            col.Item().PaddingTop(15).Column(userCol =>
                            {
                                // User header
                                userCol.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                                {
                                    row.RelativeItem().Column(innerCol =>
                                    {                
                                        innerCol.Item().Text($" Topic: {report.TopicName}")
                                            .FontSize(14);
                                    });

                                    row.RelativeItem().AlignRight().Column(innerCol =>
                                    {
                                        innerCol.Item().Text($"🎯 Target Score: {report.Goal.TargetScore}")
                                            .FontSize(12);

                                        innerCol.Item().Text($"📈 Average Score: {report.Performance.AverageScore}")
                                            .FontSize(12);
                                    });
                                });

                                // Performance highlights
                                userCol.Item().PaddingVertical(5).Row(row =>
                                {
                                    row.RelativeItem().Background(Colors.Green.Lighten5).Padding(5).Column(metricCol =>
                                    {
                                        metricCol.Item().AlignCenter().Text("Highest Score")
                                            .FontSize(12)
                                            .Bold();
                                        metricCol.Item().AlignCenter().Text(report.Performance.HighestScore.ToString())
                                            .FontSize(14)
                                            .Bold();
                                    });

                                    row.RelativeItem().Background(Colors.Blue.Lighten5).Padding(5).Column(metricCol =>
                                    {
                                        metricCol.Item().AlignCenter().Text("Attempts")
                                            .FontSize(12)
                                            .Bold();
                                        metricCol.Item().AlignCenter().Text(report.Attempts.Count.ToString())
                                            .FontSize(14)
                                            .Bold();
                                    });

                                    row.RelativeItem().Background(Colors.Orange.Lighten5).Padding(5).Column(metricCol =>
                                    {
                                        metricCol.Item().AlignCenter().Text("Improvement")
                                            .FontSize(12)
                                            .Bold();
                                        metricCol.Item().AlignCenter().Text(CalculateImprovement(report.Attempts) + "%")
                                            .FontSize(14)
                                            .Bold();
                                    });
                                });

                                // Attempts table
                                userCol.Item().PaddingTop(10).Text("📝 Quiz Attempts")
                                    .FontSize(14)
                                    .Bold();

                                userCol.Item().Table(table =>
                                {
                                    // Define columns
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Quiz Title
                                        columns.RelativeColumn(1.5f); // Date
                                        columns.RelativeColumn(1); // Score
                                        columns.RelativeColumn(2.5f); // Feedback
                                    });

                                    // Header row
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Quiz");
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date");
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("Score");
                                        header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Feedback");

                                        header.Cell().ColumnSpan(4).PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                                    });

                                    // Data rows
                                    foreach (var attempt in report.Attempts.OrderBy(a => a.AttemptDate))
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(attempt.QuizTitle);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(attempt.AttemptDate.ToString("dd/MM/yyyy"));
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter().Text(attempt.Score.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(attempt.Feedback);
                                    }
                                });

                                // Goal section
                                userCol.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(10).Column(goalCol =>
                                {
                                    goalCol.Item().Text("🎯 Learning Goal").FontSize(14).Bold();
                                    goalCol.Item().Text(report.Goal.Description);
                                    goalCol.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Current Progress: {CalculateProgress(report.Performance.AverageScore, report.Goal.TargetScore)}%");
                                        row.RelativeItem().AlignRight().Text($"Target: {report.Goal.TargetScore}");
                                    });
                                });

                                userCol.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                            });
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static string CalculateImprovement(List<AttemptDto> attempts)
        {
            if (attempts.Count < 2) return "N/A"; // Không đủ dữ liệu

            var ordered = attempts.OrderBy(a => a.AttemptDate).ToList();

            // Phương án 1: Nếu có từ 4 attempts → dùng trung bình 3 đầu/cuối
            if (attempts.Count >= 4)
            {
                var firstThreeAvg = ordered.Take(3).Average(a => a.Score);
                var lastThreeAvg = ordered.TakeLast(3).Average(a => a.Score);
                var improvement = ((lastThreeAvg - firstThreeAvg) / firstThreeAvg) * 100;
                return improvement.ToString("0.0") + "%";
            }
            // Phương án 2: Nếu có 2-3 attempts → so sánh đầu-cuối đơn giản
            else
            {
                var firstScore = ordered.First().Score;
                var lastScore = ordered.Last().Score;
                var improvement = ((lastScore - firstScore) / firstScore) * 100;
                return improvement.ToString("0.0") + "% (Tính theo đầu-cuối)";
            }
        }

        private static string CalculateProgress(double currentScore, double targetScore)
        {
            if (targetScore <= 0) return "0";
            var progress = (currentScore / targetScore) * 100;
            return progress > 100 ? "100" : progress.ToString("0.0");
        }
    }
}
