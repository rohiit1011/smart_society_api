
// Controllers/MembershipController.cs
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.Helpers;
using SocietyManagementAPI.Model;
using System.Text.Json;

namespace SocietyManagementAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class MembershipController : ControllerBase
    {
        private readonly SocietyContext _context;
        public MembershipController(SocietyContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] MembershipApplication model)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid data");

            _context.MembershipApplications.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Application submitted", id = model.ApplicationId });
        }

        [HttpGet("generate-pdf/{id}")]
        public async Task<IActionResult> GeneratePDF(int id)
        {
            var app = await _context.MembershipApplications.FindAsync(id);
            if (app == null) return NotFound();

            var pdfBytes = PdfGenerator.GenerateMembershipForm(app);
            return File(pdfBytes, "application/pdf", $"MembershipForm_{id}.pdf");
        }
        [HttpGet("generate-pdf-demo")]
        public IActionResult GeneratePDFDemo()
        {
            // Hardcoded properties for demo
            var applicantName = "Rohit Kamble";
            var age = 30;
            var occupation = "Software Engineer";
            var monthlyIncome = 120000;
            var officeAddress = "123, Tech Park, Pune";
            var residentialAddress = "456, Residency Rd, Pune";
            var flatDetails = "Flat No. 101, Building A, 75 sq.m";
            var propertyList = new List<PropertyOwnership>
    {
        new PropertyOwnership
        {
            OwnerName = "Rohit Kamble",
            PropertyDetails = "Plot No. 23, Pune",
            Reason = "Primary residence"
        }
    };

            // Generate PDF
            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                   // page.Header().Text("APPENDIX – 2").SemiBold().FontSize(16).AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"To,\nThe Chief Promoter/Secretary\nCo-operative Housing Society Ltd.\n");
                        col.Item().Text($"I, Shri/Smt. {applicantName}, aged {age} years, occupation {occupation}, " +
                                        $"hereby make an application for membership of the society.");
                        col.Item().Text($"Monthly Income: ₹ {monthlyIncome}");
                        col.Item().Text($"Office Address: {officeAddress}");
                        col.Item().Text($"Residential Address: {residentialAddress}");
                        col.Item().Text($"Flat Details: {flatDetails}");
                        col.Item().Text("\nProperty Owned (if any):");

                        if (propertyList.Count > 0)
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
                                foreach (var prop in propertyList)
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
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "MembershipForm_Demo.pdf");
        }

        public class PropertyOwnership
        {
            public string OwnerName { get; set; } = "";
            public string PropertyDetails { get; set; } = "";
            public string Reason { get; set; } = "";
        }


    }

}
