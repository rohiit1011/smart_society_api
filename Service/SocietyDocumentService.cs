using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyManagementAPI.Data;
using SocietyManagementAPI.DTO;
using SocietyManagementAPI.Model;

namespace SocietyManagementAPI.Service
{
    public interface ISocietyDocumentService
    {
        Task<object> UploadDocumentAsync(int societyId, int documentTypeId, IFormFile file, int uploadedBy, string userRole);
        Task<List<SocietyDocument>> GetDocumentsBySocietyAsync(int societyId);
        Task<List<ResidentDocuments>> GetDocumentsByResidentAsync(int residentId);
        Task<(byte[] fileBytes, string fileName)> DownloadDocumentAsync(int documentId);
        Task DeleteDocumentAsync(int documentId);
        Task<List<DocumentType>> GetAllDocumentTypesAsync(int roleId);

    }

    public class SocietyDocumentService : ISocietyDocumentService
    {
        private readonly SocietyContext _context;
        private readonly IWebHostEnvironment _env;

        public SocietyDocumentService(SocietyContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<object> UploadDocumentAsync(int societyId, int documentTypeId, IFormFile file, int uploadedBy, string userRole)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new Exception("No file uploaded.");

                string relativeFolder = string.Empty;

                if (userRole == "society")
                    relativeFolder = Path.Combine("uploads", "society_documents", societyId.ToString());

                if (userRole == "resident")
                    relativeFolder = Path.Combine("uploads", "resident_documents", societyId.ToString());

                if (userRole == "project")
                    relativeFolder = Path.Combine("uploads", "project_documents", societyId.ToString());

                if (userRole == "office")
                    relativeFolder = Path.Combine("uploads", "office_documents", societyId.ToString());

                if (string.IsNullOrEmpty(relativeFolder))
                    throw new Exception($"Unknown user role: {userRole}");

                // Ensure folder exists
                var absoluteFolderPath = Path.Combine(_env.ContentRootPath, relativeFolder);
                Directory.CreateDirectory(absoluteFolderPath);

                var fileName = file.FileName;
                var relativeFilePath = Path.Combine(relativeFolder, fileName);
                var absoluteFilePath = Path.Combine(absoluteFolderPath, fileName);

                using (var fs = new FileStream(absoluteFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                object doc = null;

                // Role-based document saving
                if (userRole == "society")
                {
                    var societyDoc = new SocietyDocument
                    {
                        society_id = societyId,
                        document_type_id = documentTypeId,
                        file_name = fileName,
                        file_path = relativeFilePath,
                        file_type = Path.GetExtension(fileName).TrimStart('.'),
                        uploaded_by = uploadedBy,
                        updated_by = uploadedBy,
                        upload_date = DateTime.UtcNow,
                        updated_at = DateTime.UtcNow,
                        is_active = true
                    };
                    _context.societyDocuments.Add(societyDoc);
                    doc = societyDoc;
                }
                else if (userRole == "resident")
                {
                    var residentDoc = new ResidentDocuments
                    {
                        resident_id = societyId,
                        document_type_id = documentTypeId,
                        file_name = fileName,
                        file_path = relativeFilePath,
                        file_type = Path.GetExtension(fileName).TrimStart('.'),
                        uploaded_by = uploadedBy,
                        updated_by = uploadedBy,
                        upload_date = DateTime.UtcNow,
                        updated_at = DateTime.UtcNow,
                        is_active = true
                    };
                    _context.residentDocuments.Add(residentDoc);
                    doc = residentDoc;
                }
                else if (userRole == "project")
                {
                    // TODO: Handle project documents if applicable
                }
                else if (userRole == "office")
                {
                    // TODO: Handle office documents if applicable
                }

                if (doc == null)
                    throw new Exception($"No document handler defined for user role: {userRole}");

                await _context.SaveChangesAsync();

                return doc;
            }
            catch (Exception ex)
            {
                // Optionally log the error here using your logger
                Console.WriteLine($"Error uploading document: {ex.Message}");

                // Re-throw as a clean error or return custom response
                throw new Exception($"File upload failed: {ex.Message}");
            }
        }

        public async Task<List<SocietyDocument>> GetDocumentsBySocietyAsync(int societyId)
        {
            return await _context.societyDocuments
               .Where(d => d.society_id == societyId)
               .Include(d => d.DocumentType) // optional, only if you need DocumentType loaded
               .ToListAsync();
        }

        public async Task<List<ResidentDocuments>> GetDocumentsByResidentAsync(int residentid)
        {
            return await _context.residentDocuments
               .Where(d => d.resident_id == residentid)
               .Include(d => d.DocumentType) // optional, only if you need DocumentType loaded
               .ToListAsync();
        }

        public async Task<(byte[] fileBytes, string fileName)> DownloadDocumentAsync(int documentId)
        {
            var doc = await _context.societyDocuments.FindAsync(documentId);
            if (doc == null) throw new Exception("Document not found.");

            var bytes = await System.IO.File.ReadAllBytesAsync(doc.file_path);
            return (bytes, doc.file_name);
        }


        public async Task DeleteDocumentAsync(int documentId)
        {
            var doc = await _context.societyDocuments.FindAsync(documentId);
            if (doc == null) throw new Exception("Document not found.");

            if (File.Exists(doc.file_path))
                File.Delete(doc.file_path);

            _context.societyDocuments.Remove(doc);
            await _context.SaveChangesAsync();
        }

        public async Task<List<DocumentType>> GetAllDocumentTypesAsync(int roleId)
        {
            var documentTypes = await (from dt in _context.documentTypes
                                       join dc in _context.documentCategories
                                       on dt.category_id equals dc.category_id into dcJoin
                                       from dc in dcJoin.DefaultIfEmpty()
                                       where dt.is_active && dt.role_id==roleId
                                       select new DocumentType
                                       {
                                           document_type_id = dt.document_type_id,
                                           name = dt.name,
                                           mandatory = dt.mandatory,
                                           category_id = dt.category_id,
                                           category_name = dc != null ? dc.category_name : "",
                                           role_id=roleId
                                       }).ToListAsync();

            return documentTypes;
        }
 
    }

}
