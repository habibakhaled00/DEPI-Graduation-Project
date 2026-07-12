using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeighborHelp.Data;
using NeighborHelp.DTOs;
using NeighborHelp.Models;
using System.Security.Claims;

namespace NeighborHelp.Controllers
{
    [ApiController]
    [Route("api/HelpRequests")]
    public class HelpRequestsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HelpRequestsApiController(AppDbContext context)
        {
            _context = context;
        }

        private string? CurrentUID =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<HelpRequestResponseDto>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] RequestStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            var query = _context.HelpRequests
                .Include(h => h.Category)
                .Include(h => h.User)
                .Include(h => h.VolunteerRequests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(h =>
                    h.Title.ToLower().Contains(term) ||
                    h.Description.ToLower().Contains(term) ||
                    h.Address.ToLower().Contains(term));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(h => h.CategoryId == categoryId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(h => h.Status == status.Value);
            }

            query = query.OrderByDescending(h => h.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HelpRequestResponseDto
                {
                    RequestId = h.RequestId,
                    UserId = h.UserId,
                    RequesterName = h.User!.FullName ?? "Unknown",
                    CategoryId = h.CategoryId,
                    CategoryName = h.Category!.Name,
                    Title = h.Title,
                    Description = h.Description,
                    Address = h.Address,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    Status = h.Status,
                    CreatedAt = h.CreatedAt,
                    VolunteerCount = h.VolunteerRequests.Count
                })
                .ToListAsync();

            if (CurrentUID != null)
            {
                var requestIds = items.Select(i => i.RequestId).ToList();
                var currentUserStatuses = await _context.VolunteerRequests
                    .Where(v => requestIds.Contains(v.RequestId) && v.UserId == CurrentUID)
                    .Select(v => new { v.RequestId, v.Status })
                    .ToDictionaryAsync(v => v.RequestId, v => (VolunteerStatus?)v.Status);

                foreach (var item in items)
                {
                    if (currentUserStatuses.TryGetValue(item.RequestId, out var volunteerStatus))
                    {
                        item.CurrentUserVolunteerStatus = volunteerStatus;
                    }
                }
            }

            Response.Headers.Append("X-Total-Count", total.ToString());
            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<HelpRequestResponseDto>> GetById(int id)
        {
            var h = await _context.HelpRequests
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.VolunteerRequests)
                .FirstOrDefaultAsync(x => x.RequestId == id);

            if (h == null) return NotFound();

            VolunteerStatus? currentUserVolunteerStatus = null;
            if (CurrentUID != null)
            {
                currentUserVolunteerStatus = await _context.VolunteerRequests
                    .Where(v => v.RequestId == id && v.UserId == CurrentUID)
                    .Select(v => (VolunteerStatus?)v.Status)
                    .FirstOrDefaultAsync();
            }

            return Ok(new HelpRequestResponseDto
            {
                RequestId = h.RequestId,
                UserId = h.UserId,
                RequesterName = h.User!.FullName ?? "Unknown",
                CategoryId = h.CategoryId,
                CategoryName = h.Category!.Name,
                Title = h.Title,
                Description = h.Description,
                Address = h.Address,
                Latitude = h.Latitude,
                Longitude = h.Longitude,
                Status = h.Status,
                CreatedAt = h.CreatedAt,
                VolunteerCount = h.VolunteerRequests.Count,
                CurrentUserVolunteerStatus = currentUserVolunteerStatus
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HelpRequestResponseDto>> Create(CreateHelpRequestDto dto)
        {
            if (CurrentUID == null) return Unauthorized();

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!categoryExists) return BadRequest("Invalid category.");

            var entity = new HelpRequest
            {
                UserId = CurrentUID,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Description = dto.Description,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = RequestStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.HelpRequests.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.RequestId }, entity);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateHelpRequestDto dto)
        {
            var entity = await _context.HelpRequests.FindAsync(id);
            if (entity == null) return NotFound();

            if (entity.UserId != CurrentUID)
                return Forbid();

            entity.CategoryId = dto.CategoryId;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Address = dto.Address;
            entity.Latitude = dto.Latitude;
            entity.Longitude = dto.Longitude;
            entity.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.HelpRequests.FindAsync(id);
            if (entity == null) return NotFound();

            if (entity.UserId != CurrentUID)
                return Forbid();

            _context.HelpRequests.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
