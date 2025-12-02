// Helpers/PdfGenerator.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SocietyManagementAPI.Model;
using System.Reflection.Metadata;
using System.Text.Json;


namespace SocietyManagementAPI.Helpers
{

    public static class PdfGenerator
    {
        public static byte[] GenerateMembershipForm(MembershipApplication app)
        {
            var properties = JsonSerializer.Deserialize<List<PropertyOwnership>>(app.PropertyDetailsJson) ?? new List<PropertyOwnership>();

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    //page.Header().Text("APPENDIX – 2").SemiBold().FontSize(16).AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"To,\nThe Chief Promoter/Secretary\nCo-operative Housing Society Ltd.\n");
                        col.Item().Text($"I, Shri/Smt. {app.ApplicantName}, aged {app.Age} years, occupation {app.Occupation}, " +
                            $"hereby make an application for membership of the society.");
                        col.Item().Text($"Monthly Income: ₹ {app.MonthlyIncome}");
                        col.Item().Text($"Office Address: {app.OfficeAddress}");
                        col.Item().Text($"Residential Address: {app.ResidentialAddress}");
                        col.Item().Text($"Flat Details: {app.FlatDetails}");
                        col.Item().Text("\nProperty Owned (if any):");

                        if (properties.Count > 0)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Sr. No").Bold();
                                    header.Cell().Text("Owner Name").Bold();
                                    header.Cell().Text("Property Details").Bold();
                                    header.Cell().Text("Reason").Bold();
                                });

                                int srNo = 1;
                                foreach (var prop in properties)
                                {
                                    table.Cell().Text(srNo.ToString());
                                    table.Cell().Text(prop.OwnerName);
                                    table.Cell().Text(prop.PropertyDetails);
                                    table.Cell().Text(prop.Reason);
                                    srNo++;
                                }
                            });
                        }
                        else
                        {
                            col.Item().Text("None");
                        }

                        col.Item().Text("\nI undertake to abide by the Bye-laws and discharge all liabilities.");
                        col.Item().Text($"\nDate: {DateTime.Now:dd-MM-yyyy}");
                        col.Item().Text("Signature: ___________________________");
                    });
                });
            });

            return pdf.GeneratePdf();
        }
    }

    public class PropertyOwnership
    {
        public string OwnerName { get; set; } = "";
        public string PropertyDetails { get; set; } = "";
        public string Reason { get; set; } = "";
    }

}
