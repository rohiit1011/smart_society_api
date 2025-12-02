using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokenDocsAPI.Models
{
    public class ResponseStatusModel
    {
        public enum ResponseStatusEnum
        {
            Success = 1, Error = 2
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public ErrorMessageModel? errorMessageResponse { get; set; }


        public SuccessMessageModel? successMessageResponse { get; set; }

        [Required]
        public ResponseStatusEnum responseStatus { get; set; }

      

    }
}
