using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; // Bắt buộc có để dùng LINQ (from... join...)
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. Hàm lấy danh sách (🔥 ĐÃ SỬA: Lấy thêm Tên Phòng Ban)
        public async Task<object> GetLogsAsync()
        {
            // A. Lấy dữ liệu thô từ 3 bảng
            var logs = await _unitOfWork.GetRepository<AuditLog>().GetAllAsync();
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var depts = await _unitOfWork.GetRepository<Department>().GetAllAsync(); // 👇 Lấy thêm bảng Department

            // B. Dùng LINQ để nối (JOIN) 3 bảng lại với nhau
            var query = from l in logs
                            // 1. Nối Log với User
                        join u in users on l.UserID equals u.UserID into uGroup
                        from u in uGroup.DefaultIfEmpty()

                            // 2. Nối User với Department (để lấy tên phòng)
                        join d in depts on u?.DepartmentID equals d.DepartmentID into dGroup
                        from d in dGroup.DefaultIfEmpty()

                        orderby l.ActionDate descending

                        // 3. Chọn dữ liệu trả về (Khớp với AuditLogResponse bên Client)
                        select new
                        {
                            LogID = l.LogID,
                            Action = l.Action,
                            TableName = l.TableName,

                            // Lấy tên User (nếu null thì hiện System)
                            UserName = u != null ? u.FullName : "System/Admin",

                            // 👇 QUAN TRỌNG: Lấy tên Phòng (nếu null thì hiện trống)
                            DepartmentName = d != null ? d.DeptName : "",

                            ActionDate = l.ActionDate,
                            Details = l.Details
                        };

            return query.ToList();
        }

        // 2. Hàm Ghi Log (Hàm cũ - giữ nguyên để tương thích ngược)
        public async Task LogAsync(string action, string tableName, int? recordId, int userId, string details)
        {
            await CreateLogAsync(action, tableName, recordId ?? 0, details, userId);
        }

        // 3. Hàm Ghi Log MỚI (RepairService gọi hàm này)
        public async Task CreateLogAsync(string action, string tableName, int recordId, string details, int userId)
        {
            try
            {
                var log = new AuditLog
                {
                    Action = action,
                    TableName = tableName,
                    RecordID = recordId,
                    UserID = userId,
                    Details = details,
                    ActionDate = DateTime.Now,
                    Timestamp = DateTime.Now
                };

                await _unitOfWork.GetRepository<AuditLog>().AddAsync(log);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // Bỏ qua lỗi log
            }
        }
    }
}