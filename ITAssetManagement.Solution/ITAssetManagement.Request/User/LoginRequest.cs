namespace ITAssetManagement.Request.User
{
    // Class hứng dữ liệu đăng nhập
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // Class trả về kết quả đăng nhập (Token + Thông tin)
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public bool MustChangePassword { get; set; } // Cờ đổi mật khẩu
    }
}