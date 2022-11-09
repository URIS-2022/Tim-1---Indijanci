using System.ComponentModel.DataAnnotations;

namespace Bit.Api.Models.Request.Accounts;

public class VerifyOtpRequestModel
{
    [Required]
    public string OTP { get; set; }
}
