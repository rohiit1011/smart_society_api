using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TokenDocsAPI.Models
{
    public class SuccessMessageModel
    {
        public enum ExecustionStatusEnum
        {
            Success = 200, Error = 100 , Exception=500
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string successMessage { get; set; }

        public string successMessageId { get; set; }

      /*  public object Data { get; set; }*/

        public dynamic? responseData { get; set; }

        public ExecustionStatusEnum successMessageStatus { get; set; }
    }
}
