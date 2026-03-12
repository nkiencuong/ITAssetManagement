using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Request.RepairTickets; // Nhớ dòng này để dùng RepairItemDto
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IRepairService
    {
        Task<List<RepairTicket>> GetAllTicketsAsync();
        Task<RepairTicket?> GetTicketByIdAsync(int id);

        // Hàm tạo phiếu trả về RepairTicket để Client nhận được ID mới tạo
        Task<RepairTicket> CreateTicketAsync(RepairTicket ticket, int actionUserId);

        Task<bool> CancelTicketAsync(int ticketId, string reason);

        
        Task<bool> CompleteRepairAsync(int ticketId, string damageStatus, string solution, List<RepairItemDto> parts, int userId);
        Task<bool> ClaimTicketAsync(int ticketId, int userId);
        Task<bool> AssignTicketAsync(int ticketId, int assignToUserId, int actionUserId);
    }
}