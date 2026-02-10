namespace ITAssetManagement.Response
{
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; } // Chuỗi JWT để xác thực
        public int? DepartmentID { get; set; }
        public string?DeptName { get; set; } // Thêm luôn tên khoa cho tiện hiển thị
    }
}