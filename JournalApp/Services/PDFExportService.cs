using JournalApp.Data;
using JournalApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;
using Colors = QuestPDF.Helpers.Colors; // Explicitly use QuestPDF Colors

namespace JournalApp.Services
{
    public class PdfExportService
    {
        private readonly AppDatabase _database;

        public PdfExportService(AppDatabase database)
        {
            _database = database;
            // Set license type (Community license is free for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> ExportEntriesToPdfAsync(DateTime startDate, DateTime endDate)
        {
            // Get entries in the date range
            var entries = await _database.GetEntriesFilteredAsync(startDate, endDate);

            if (entries.Count == 0)
            {
                throw new Exception("No entries found in the selected date range.");
            }

            // Create filename with date range
            var fileName = $"Journal_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // Generate PDF synchronously
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        page.Header()
                            .Text($"Journal Entries: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken3);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                foreach (var entry in entries.OrderBy(e => e.EntryDate))
                                {
                                    RenderEntry(column, entry);
                                    column.Item().PaddingTop(20);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf(filePath);
            });

            return filePath;
        }

        private void RenderEntry(ColumnDescriptor column, JournalEntry entry)
        {
            // Entry Header
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(headerColumn =>
                {
                    headerColumn.Item().Text(entry.Title)
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Darken2);

                    headerColumn.Item().Text($"{entry.EntryDate:dddd, MMMM dd, yyyy}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingTop(10);

            // Moods
            var moods = _database.GetMoodsForEntryAsync(entry.Id).Result;
            if (moods.Any())
            {
                column.Item().Row(row =>
                {
                    row.AutoItem().Text("Moods: ").SemiBold().FontSize(10);
                    row.AutoItem().Text(string.Join(", ", moods.Select(m => m.Name)))
                        .FontSize(10).FontColor(GetMoodColor(moods.First().Category));
                });
            }

            // Tags
            var tags = _database.GetTagsForEntryAsync(entry.Id).Result;
            if (tags.Any())
            {
                column.Item().Row(row =>
                {
                    row.AutoItem().Text("Tags: ").SemiBold().FontSize(10);
                    row.AutoItem().Text(string.Join(", ", tags.Select(t => t.Name)))
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                });
            }

            column.Item().PaddingTop(10);

            // Content (strip HTML tags for PDF)
            var cleanContent = StripHtmlTags(entry.Content);
            column.Item().Text(cleanContent)
                .FontSize(11).LineHeight(1.5f);

            // Separator
            column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
        }

        private string GetMoodColor(string category)
        {
            return category switch
            {
                "Positive" => Colors.Green.Darken2,
                "Neutral" => Colors.Blue.Darken2,
                "Negative" => Colors.Red.Darken2,
                _ => Colors.Grey.Darken2
            };
        }

        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Remove HTML tags
            var stripped = Regex.Replace(html, "<.*?>", string.Empty);

            // Decode HTML entities
            stripped = System.Net.WebUtility.HtmlDecode(stripped);

            return stripped.Trim();
        }
    }
}