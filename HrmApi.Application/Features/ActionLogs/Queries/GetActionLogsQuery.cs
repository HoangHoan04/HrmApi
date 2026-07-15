using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HrmApi.Application.Features.ActionLogs.Queries
{
    public class ActionLogDto
    {
        public Guid Id { get; set; }
        public Guid CreatedById { get; set; }
        public string CreatedByCode { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string? CreatedNote { get; set; }
        public string? ActionType { get; set; }
        public Guid? EntityId { get; set; }
        public string? EntityName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class GetActionLogsQuery : PagedRequest, IRequest<PagedResult<ActionLogDto>>
    {
        public string? EntityName { get; set; }
        public Guid? EntityId { get; set; }
    }

    public class GetActionLogsQueryHandler : IRequestHandler<GetActionLogsQuery, PagedResult<ActionLogDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetActionLogsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ActionLogDto>> Handle(GetActionLogsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ActionLogEntities.AsNoTracking();

            if (!string.IsNullOrEmpty(request.EntityName))
            {
                query = query.Where(x => x.EntityName == request.EntityName);
            }

            if (request.EntityId.HasValue && request.EntityId.Value != Guid.Empty)
            {
                query = query.Where(x => x.EntityId == request.EntityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.Trim().ToLower();
                query = query.Where(x => x.CreatedByName.ToLower().Contains(search) || 
                                         x.CreatedByCode.ToLower().Contains(search) || 
                                         (x.CreatedNote != null && x.CreatedNote.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = query.OrderByDescending(x => x.CreatedAt);

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ActionLogDto
                {
                    Id = x.Id,
                    CreatedById = x.CreatedById,
                    CreatedByCode = x.CreatedByCode,
                    CreatedByName = x.CreatedByName,
                    CreatedNote = x.CreatedNote,
                    ActionType = x.ActionType,
                    EntityId = x.EntityId,
                    EntityName = x.EntityName,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    Location = x.Location,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ActionLogDto>(items, totalCount, request.PageIndex, request.PageSize);
        }
    }
}
