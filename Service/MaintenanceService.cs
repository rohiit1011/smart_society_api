using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Interface;
using SocietyManagementAPI.Model;
using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementAPI.Service
{ 

    public class MaintenanceService : IMaintenanceService
    {
        private readonly SocietyContext _context;
        private readonly IEmailService _emailService;
        //private readonly INotificationService _notificationService;
        private readonly ILogger<MaintenanceService> _logger;

        public MaintenanceService(SocietyContext context,
                                  IEmailService emailService,
                                //  INotificationService notificationService,
                                  ILogger<MaintenanceService> logger)
        {
            _context = context;
            _emailService = emailService;
         //   _notification_service = notificationService;
            _logger = logger;
        }

        // ------------------------------
        // 1) Preview: compute bills in-memory
        // ------------------------------
        public async Task<List<PreviewBillDto>> PreviewGenerateAsync(MaintenanceGenerateRequest req)
        {
            if (req.PeriodFrom >= req.PeriodTo) throw new ArgumentException("PeriodFrom must be before PeriodTo");

            // Load requested heads (or all active if none specified)
            var headsQuery = _context.maintenanceHeads.AsQueryable();
            if (req.HeadIds != null && req.HeadIds.Any())
                headsQuery = headsQuery.Where(h => req.HeadIds.Contains(h.maintenance_head_id));
            var heads = await headsQuery.ToListAsync();

            // Bulk load residents for society via userRoles
            var residentUserIds = await _context.userRoles
                .Where(ur => ur.society_id == req.SocietyId)
                .Select(ur => ur.user_id)
                .Distinct()
                .ToListAsync();

            var residents = await _context.residentInfo
                .Where(r => residentUserIds.Contains(r.user_id))
                .ToListAsync();

            // Bulk load flats and vehicles
            var residentIds = residents.Select(r => r.resident_id).ToList();
            var allFlats = await _context.residentFlats
                .Where(f => residentIds.Contains(f.resident_id))
                .ToListAsync();

            var allVehicles = await _context.residentVehicles
                .Where(v => residentIds.Contains(v.resident_id))
                .ToListAsync();

            // Load society maintenance setting fallback (for defaults)
            var societyDefault = await _context.societyMaintenanceSettings
                .Where(s => s.society_id == req.SocietyId)
                .FirstOrDefaultAsync();

            var results = new List<PreviewBillDto>();

            // Group flats per resident to generate per-flat bills
            var flatsByResident = allFlats.GroupBy(f => f.resident_id)
                                          .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var r in residents)
            {
                if (!flatsByResident.TryGetValue(r.resident_id, out var flatsForResident) || flatsForResident.Count == 0)
                {
                    // Skip residents with no flats (optional)
                    continue;
                }

                foreach (var flat in flatsForResident)
                {
                    var preview = new PreviewBillDto
                    {
                        ResidentId = r.resident_id,
                        ResidentFlatId = flat.resident_flat_id,
                        FlatNo = (!string.IsNullOrEmpty(flat.flat_or_house_number) ? flat.flat_or_house_number : flat.flat_or_house_number) ?? $"Flat-{flat.resident_flat_id}",
                        Lines = new List<PreviewLineDto>()
                    };

                    // Determine base maintenance (owner/tenant) fallback
                    decimal baseMaintenance = 0m;
                    if (!string.IsNullOrEmpty(flat.ownership_type) && flat.ownership_type.Equals("Tenant", StringComparison.OrdinalIgnoreCase))
                        baseMaintenance = flat.tenant_maintenance ?? 0m;
                    else
                        baseMaintenance = flat.monthly_maintenance ?? 0m;

                    if (baseMaintenance == 0 && societyDefault != null)
                        baseMaintenance = societyDefault.amount;

                    decimal runningTotal = 0m;

                    // compute each head amount
                    foreach (var head in heads)
                    {
                        decimal amount = ComputeAmountForHead(head, flat, allVehicles.Where(v => v.resident_id == r.resident_id).ToList(), baseMaintenance);
                        decimal taxPercent = 0m; // if you store tax per head, read it here
                        decimal taxAmount = Math.Round(amount * taxPercent / 100m, 2);
                        var lineTotal = Math.Round(amount + taxAmount, 2);

                        preview.Lines.Add(new PreviewLineDto
                        {
                            MaintenanceHeadId = head.maintenance_head_id,
                            Description = head.name,
                            Amount = amount,
                            TaxPercent = taxPercent,
                            TaxAmount = taxAmount,
                            LineTotal = lineTotal
                        });

                        runningTotal += lineTotal;
                    }

                    preview.TotalAmount = Math.Round(runningTotal, 2);
                    results.Add(preview);
                }
            }

            return results;
        }

        private decimal ComputeAmountForHead(MaintenanceHeads head, ResidentFlat flat, List<ResidentVehicles> residentVehicles, decimal baseMaintenance)
        {
            if (head == null) return 0m;
            var calcType = (head.calc_type ?? "").ToLowerInvariant();
            decimal amount = 0m;
            //if(baseMaintenance!=0 || baseMaintenance != null)
            //{
            //    head.default_amount = baseMaintenance;
            //}
            switch (calcType)
            {
                case "fixed":
                    amount = (head.default_amount);
                    break;
                case "persqft":
                case "per_sqft":
                case "per_sq_ft":
                    amount = ((flat.carpet_area_sqft ?? 0m) * (head.default_amount));
                    break;
                case "perflat":
                case "per_flat":
                    amount = (head.default_amount);
                    break;
                case "pervehicle":
                case "per_vehicle":
                    amount = (residentVehicles?.Count ?? 0) * (head.default_amount);
                    break;
                case "percentage":
                    amount = ((head.default_percentage??0) / 100m) * baseMaintenance;
                    break;
                case "custom":
                default:
                    amount = (head.default_amount);
                    break;
            }

            return Math.Round(amount, 2);
        }

        // ------------------------------
        // 2) Generate: persist bill run + bills + lines
        // ------------------------------
        public async Task<GenerateResultDto> GenerateRunAsync(MaintenanceGenerateRequest req, int generatedByUserId)
        {
            // prevent double-generation for same society & period if required
            var existing = await _context.maintenanceBillRuns
                .FirstOrDefaultAsync(r => r.society_id == req.SocietyId && r.period_from == req.PeriodFrom && r.period_to == req.PeriodTo && r.status == "Finalized");

            if (existing != null)
                throw new InvalidOperationException("Bills for this period are already finalized.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
 
                // 1) Create bill_run
                var run = new MaintenanceBillRuns
                {
                    society_id = req.SocietyId,
                    period_from = req.PeriodFrom,
                    period_to = req.PeriodTo,
                    frequency = req.Frequency,
                    generated_at = DateTime.UtcNow,
                    generated_by = generatedByUserId,
                    status = "Generated",
                    notes="",
                    total_amount=0
                };
                _context.maintenanceBillRuns.Add(run);
                await _context.SaveChangesAsync();

                // 2) Compute preview in-memory (bulk)
                var preview = await PreviewGenerateAsync(req);

                // 3) Create bills
                var billEntities = preview.Select(pb => new MaintenanceBills
                {
                    bill_run_id = run.bill_run_id,
                    resident_flat_id = pb.ResidentFlatId,
                    resident_id = pb.ResidentId,
                    bill_no = GenerateBillNo(req.SocietyId, run.bill_run_id),
                    bill_date = DateTime.UtcNow.Date,
                    period_from = req.PeriodFrom,
                    period_to = req.PeriodTo,
                    due_date = req.PeriodTo.AddDays(7), // or society-specific
                    total_amount = pb.TotalAmount,
                    total_tax = pb.Lines.Sum(l => l.TaxAmount),
                    total_payable = pb.TotalAmount,
                    status = "Unpaid",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }).ToList();


                run.total_amount = billEntities.Sum(b => b.total_amount);

                await _context.maintenanceBills.AddRangeAsync(billEntities);
                await _context.SaveChangesAsync();

                // 4) Create lines (map preview lines to created bills by index)
                var lineEntities = new List<MaintenanceBillLines>();
                for (int i = 0; i < preview.Count; i++)
                {
                    var pb = preview[i];
                    var bill = billEntities[i];
                    foreach (var pl in pb.Lines)
                    {
                        var line = new MaintenanceBillLines
                        {
                            bill_id = bill.bill_id,
                            maintenance_head_id = pl.MaintenanceHeadId,
                            description = pl.Description,
                            amount = pl.Amount,
                            quantity = 1m,
                            unit_rate = pl.Amount,
                            tax_percent = pl.TaxPercent,
                            tax_amount = pl.TaxAmount,
                            line_total = pl.LineTotal,
                            created_at = DateTime.UtcNow
                        };
                        lineEntities.Add(line);
                    }
                }

                if (lineEntities.Any())
                {
                    await _context.maintenanceBillLines.AddRangeAsync(lineEntities);
                    await _context.SaveChangesAsync();
                }


                // 5) finalize run
                run.status = "Finalized";
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // 6) Fire-and-forget notifications (do not await for API latency)
                _ = Task.Run(() => NotifyResidentsAsync(run.bill_run_id));

                return new GenerateResultDto
                {
                    BillRunId = run.bill_run_id,
                    CreatedBillsCount = billEntities.Count,
                    TotalAmount = billEntities.Sum(b => b.total_amount)
                };
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger?.LogError(ex, "GenerateRunAsync failed");
                throw;
            }
        }

        private string GenerateBillNo(int societyId, int runId)
        {
            // Use runId to produce unique number per run; can be improved
            return $"S{societyId}-{DateTime.UtcNow:yyyyMM}-{runId:0000}";
        }

        // ------------------------------
        // 3) Fetch runs & bills & resident bills
        // ------------------------------
        public async Task<List<MaintenanceBillRunDto>> FetchBillRunsAsync(int societyId)
        {
            var runs = await _context.maintenanceBillRuns
                .Where(r => r.society_id == societyId)
                .OrderByDescending(r => r.generated_at)
                .Take(50)
                .ToListAsync();

            return runs.Select(r => new MaintenanceBillRunDto
            {
                Id = r.bill_run_id,
                PeriodFrom = r.period_from,
                PeriodTo = r.period_to,
                Frequency = r.frequency,
                Status = r.status,
                GeneratedAt = r.generated_at ,
                GeneratedBy = r.generated_by,
                TotalAmount=r.total_amount,

                
            }).ToList();
        }

        public async Task<List<MaintenanceBillDto>> FetchBillsForRunAsync(int runId)
        {
            var bills = await _context.maintenanceBills
                .Where(b => b.bill_run_id == runId)
               // .Include(b => b.maintenance_bill_lines)   // <-- REQUIRED
                .ToListAsync();

            var list = bills.Select(b => new MaintenanceBillDto
            {
                BillId = b.bill_id,
                BillNo = b.bill_no,
                ResidentId = b.resident_id,
                ResidentFlatId = b.resident_flat_id,
                PeriodFrom = b.period_from,
                PeriodTo = b.period_to,
                DueDate = b.due_date,
                TotalAmount = b.total_amount,
                TotalTax = b.total_tax,
                TotalPayable = b.total_payable,
                Status = b.status,

                //Lines = b.maintenance_bill_lines?.Select(l => new MaintenanceBillLineDto
                //{
                //    BillLineId = l.bill_line_id,
                //    //Description = l.description,
                //    Amount = l.amount,
                //    TaxAmount = l.tax_amount,
                //    LineTotal = l.line_total ?? 0m

                //}).ToList() ?? new List<MaintenanceBillLineDto>()

            }).ToList();

            return list;
        }

        // ------------------------------
        // 4) Send single bill notice (email + in-app)
        // ------------------------------
        public async Task<bool> SendBillNoticeAsync(int billId)
        {
            var bill = await _context.maintenanceBills
                .Include(b => b.resident_id) // optional if navigation exists
                .FirstOrDefaultAsync(b => b.bill_id == billId);

            if (bill == null) return false;

            var resident = await _context.residentInfo.FirstOrDefaultAsync(r => r.resident_id == bill.resident_id);
            if (resident == null || string.IsNullOrEmpty(resident.email)) return false;

            var subject = $"New maintenance bill: {bill.bill_no}";
            var body = $@"
                    <p>Dear {resident.first_name},</p>
                    <p>Your maintenance bill <b>{bill.bill_no}</b> for period {bill.period_from:yyyy-MM-dd} — {bill.period_to:yyyy-MM-dd} has been generated.</p>
                    <p>Total payable: <b>₹{bill.total_payable}</b>. Due date: <b>{bill.due_date:yyyy-MM-dd}</b></p>
                    <p>Login to your portal to view & pay the bill.</p>
                    <p>Regards,<br/>Smart Society</p>
                    ";

            try
            {
                await _emailService.SendEmailAsync(resident.email, subject, body);
                //await _notification_service.CreateAsync(bill.resident_id, $"New bill {bill.bill_no} of ₹{bill.total_payable} generated");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SendBillNoticeAsync failed");
                return false;
            }
        }

        // ------------------------------
        // 5) NotifyResidentsAsync (bulk)
        // ------------------------------
        private async Task NotifyResidentsAsync(int billRunId)
        {
            try
            {
                var bills = await _context.maintenanceBills
                    .Where(b => b.bill_run_id == billRunId)
                    .ToListAsync();

                foreach (var b in bills)
                {
                    try
                    {
                        var resident = await _context.residentInfo.FirstOrDefaultAsync(r => r.resident_id == b.resident_id);
                        if (resident == null || string.IsNullOrEmpty(resident.email)) continue;

                        var subject = $"Bill generated: {b.bill_no}";
                        var body = $@"
Dear {resident.first_name},
Your bill {b.bill_no} for period {b.period_from:yyyy-MM-dd} - {b.period_to:yyyy-MM-dd} is generated. Total: ₹{b.total_payable}.
Login to pay.
";
                        // send email (do not block)
                        await _emailService.SendEmailAsync(resident.email, subject, body);

                        // push in-app notification
                       // await _notification_service.CreateAsync(b.resident_id, $"New maintenance bill {b.bill_no} of ₹{b.total_payable} generated");
                    }
                    catch (Exception innerEx)
                    {
                        _logger?.LogError(innerEx, "NotifyResidentsAsync inner error (continuing)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "NotifyResidentsAsync failed");
            }
        }

        // ------------------------------
        // 6) Scheduled: apply late penalties (example)
        // ------------------------------
        public async Task<bool> ApplyLatePenaltiesForDueRunsAsync()
        {
            try
            {
                // find bills past due_date and unpaid
                var overdueBills = await _context.maintenanceBills
                    .Where(b => b.due_date < DateTime.UtcNow.Date && (b.status == "Unpaid" || b.status == "PartiallyPaid"))
                    .ToListAsync();

                foreach (var bill in overdueBills)
                {
                    // check if penalty already applied for this bill in adjustments, or add simple percentage
                    // For demo: add 2% of total_amount as penalty and create adjustment & bill line
                    decimal penaltyPercent = 2m;
                    decimal penaltyAmount = Math.Round((bill.total_amount ) * penaltyPercent / 100m, 2);

                    if (penaltyAmount <= 0) continue;

                    // create adjustment (maintenance_adjustments) if table exists
                    var adj = new MaintenanceAdjustments
                    {
                        bill_id = bill.bill_id,
                        adj_type = "LateFee",
                        amount = penaltyAmount,
                        reason = "Auto late fee",
                        created_by = 0,
                        created_at = DateTime.UtcNow
                    };
                    _context.maintenanceAdjustments.Add(adj);

                    // add bill line
                    var penaltyLine = new MaintenanceBillLines
                    {
                        bill_id = bill.bill_id,
                        maintenance_head_id = 0,
                        description = "Late Fee",
                        amount = penaltyAmount,
                        quantity = 1,
                        unit_rate = penaltyAmount,
                        tax_percent = 0,
                        tax_amount = 0,
                        line_total = penaltyAmount,
                        created_at = DateTime.UtcNow
                    };
                    _context.maintenanceBillLines.Add(penaltyLine);

                    // update totals
                    bill.total_amount = (bill.total_amount ) + penaltyAmount;
                    bill.total_payable = (bill.total_payable ) + penaltyAmount;
                    bill.updated_at = DateTime.UtcNow;
                    // keep status as Unpaid
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "ApplyLatePenaltiesForDueRunsAsync failed");
                return false;
            }
        }

        public async Task<List<MaintenanceHeads>> FetchAllHeadsAsync(int societyId)
        {
            try
            {
                return await _context.maintenanceHeads
                         .Where(h => h.society_id == societyId && h.is_active)
                         .OrderBy(h => h.maintenance_head_id)
                         .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching maintenance heads");
                throw; // Let controller handle the response formatting
            }
        }

        public async Task<MaintenanceHeads> AddHeadAsync(MaintenanceHeads dto)
        {
            try
            {
                // Ensure ID is zero for auto-increment (if applicable)
                dto.maintenance_head_id = 0;

                dto.is_active = true;
                dto.created_at = DateTime.UtcNow;

                await _context.maintenanceHeads.AddAsync(dto);
                await _context.SaveChangesAsync();

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddHeadAsync");
                throw;       // handled in controller
            }
        }


    }


}
