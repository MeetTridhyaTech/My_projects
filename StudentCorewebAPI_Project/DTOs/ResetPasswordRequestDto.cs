namespace StudentCorewebAPI_Project.DTOs
{
    public class ResetPasswordRequestDto
    {
        //public string Email { get; set; }
        //public string OTP { get; set; }              // ✅ Add this line
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
