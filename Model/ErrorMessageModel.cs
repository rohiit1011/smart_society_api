using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static TokenDocsAPI.Models.SuccessMessageModel;

namespace TokenDocsAPI.Models
{
    public class ErrorMessageModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string errorMessage { get; set; }
        public string? errorMessageId { get; set; } = "0";

        public ExecustionStatusEnum errorMessageStatus { get; set; }



    }
}
