namespace SocietyManagementAPI.DTO
{
    public class GenerateBillRequest
    {
    }
    // Dtos/GenerateRequest.cs
    public class MaintenanceGenerateRequest
    {
        public int SocietyId { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public List<int> HeadIds { get; set; } = new();
        public bool Finalize { get; set; } = true; // if false -> generate but keep draft (optional)
    }

    // Dtos/PreviewBillDto.cs
    public class PreviewBillDto
    {
        public int ResidentId { get; set; }
        public int UserId { get; set; }
        public int ResidentFlatId { get; set; }
        public string FlatNo { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PreviewLineDto> Lines { get; set; } = new();
    }



    public class PreviewLineDto
    {
        public int MaintenanceHeadId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }

        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal LineTotal { get; set; }
    }



    // Dtos/GenerateResultDto.cs
    public class GenerateResultDto
    {
        public int BillRunId { get; set; }
        public int CreatedBillsCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MaintenanceBillRunDto
    {
        public int Id { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public string Frequency { get; set; }
        public string Status { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int GeneratedBy { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MaintenanceBillDto
    {
        public int BillId { get; set; }
        public string BillNo { get; set; }

        public int ResidentId { get; set; }
        public int ResidentFlatId { get; set; }

        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public DateTime DueDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalPayable { get; set; }

        public string Status { get; set; }

        public List<MaintenanceBillLineDto> Lines { get; set; } = new();
    }

    public class MaintenanceBillLineDto
    {
        public int BillLineId { get; set; }
        public int BillId { get; set; }

        public int HeadId { get; set; }
        public string HeadName { get; set; }

        public decimal Amount { get; set; }       // Base amount
        public decimal TaxAmount { get; set; }    // GST, if any
        public decimal LineTotal { get; set; }    // Amount + Tax
    }


}
