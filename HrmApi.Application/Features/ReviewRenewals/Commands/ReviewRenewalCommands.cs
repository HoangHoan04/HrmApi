using HrmApi.Application.Common.Interfaces;
using HrmApi.Application.Features.Contracts.Commands;
using HrmApi.Application.Mappings;
using HrmApi.Domain.Entities.Contract;
using HrmApi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrmApi.Application.Features.ReviewRenewals.Commands
{
    public class CreateReviewRenewalCommand : ReviewRenewalCommandFields, IRequest<Guid> { }

    public class CreateReviewRenewalCommandHandler : IRequestHandler<CreateReviewRenewalCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public CreateReviewRenewalCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateReviewRenewalCommand request, CancellationToken cancellationToken)
        {
            if (!request.ContractId.HasValue || request.ContractId == Guid.Empty)
            {
                throw new InvalidOperationException("Hợp đồng là bắt buộc.");
            }

            ContractEntity contract = await _context.ContractEntities
                .FirstOrDefaultAsync(x => x.Id == request.ContractId && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Hợp đồng không tồn tại.");

            if (contract.Status is not (ContractStatus.Active or ContractStatus.ExpiringSoon or ContractStatus.Expired))
            {
                throw new InvalidOperationException("Chỉ tạo đánh giá cho hợp đồng đang hiệu lực / sắp hết hạn / đã hết hạn.");
            }

            bool pendingExists = await _context.ReviewRenewalEntities.AnyAsync(x =>
                !x.IsDeleted
                && x.ContractId == contract.Id
                && (x.Status == ReviewRenewalStatus.PendingReview
                    || x.Status == ReviewRenewalStatus.PendingApproval
                    || x.Status == ReviewRenewalStatus.Approved),
                cancellationToken);
            if (pendingExists)
            {
                throw new InvalidOperationException("Hợp đồng này đã có đợt đánh giá đang xử lý.");
            }

            if (request.ProposedContractTypeId.HasValue && request.ProposedContractTypeId != Guid.Empty)
            {
                bool typeOk = await _context.ContractTypeEntities
                    .AnyAsync(x => x.Id == request.ProposedContractTypeId && !x.IsDeleted && x.IsActive, cancellationToken);
                if (!typeOk)
                {
                    throw new InvalidOperationException("Loại hợp đồng đề xuất không tồn tại.");
                }
            }

            string status = string.IsNullOrWhiteSpace(request.Status)
                ? (string.IsNullOrWhiteSpace(request.Recommendation)
                    ? ReviewRenewalStatus.PendingReview
                    : ReviewRenewalStatus.PendingApproval)
                : request.Status.Trim();

            ReviewRenewalEntity entity = new()
            {
                ContractId = contract.Id,
                EmployeeId = contract.EmployeeId,
                ReviewDate = request.ReviewDate ?? DateTime.UtcNow.Date,
                ReviewedBy = string.IsNullOrWhiteSpace(request.ReviewedBy)
                    ? (_currentUser.Username ?? _currentUser.UserCode)
                    : request.ReviewedBy.Trim(),
                PerformanceScore = request.PerformanceScore,
                ReviewResult = string.IsNullOrWhiteSpace(request.ReviewResult) ? null : request.ReviewResult.Trim(),
                ReviewComment = string.IsNullOrWhiteSpace(request.ReviewComment) ? null : request.ReviewComment.Trim(),
                Recommendation = string.IsNullOrWhiteSpace(request.Recommendation) ? null : request.Recommendation.Trim(),
                ProposedContractTypeId = request.ProposedContractTypeId,
                ProposedStartDate = request.ProposedStartDate,
                ProposedEndDate = request.ProposedEndDate,
                ProposedBasicSalary = request.ProposedBasicSalary,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                Status = status,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };

            _ = _context.ReviewRenewalEntities.Add(entity);
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.CREATE,
                "ReviewRenewalEntity",
                entity.Id,
                null,
                ReviewRenewalMapper.ToLogObject(entity),
                "Tạo đánh giá gia hạn hợp đồng");

            return entity.Id;
        }
    }

    public class UpdateReviewRenewalCommand : ReviewRenewalCommandFields, IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class UpdateReviewRenewalCommandHandler : IRequestHandler<UpdateReviewRenewalCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;

        public UpdateReviewRenewalCommandHandler(IApplicationDbContext context, IActionLogService actionLog)
        {
            _context = context;
            _actionLog = actionLog;
        }

        public async Task<bool> Handle(UpdateReviewRenewalCommand request, CancellationToken cancellationToken)
        {
            ReviewRenewalEntity? entity = await _context.ReviewRenewalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is ReviewRenewalStatus.Applied or ReviewRenewalStatus.Rejected)
            {
                throw new InvalidOperationException("Không thể cập nhật đánh giá đã áp dụng hoặc đã từ chối.");
            }

            object oldValue = ReviewRenewalMapper.ToLogObject(entity);
            ReviewRenewalMapper.ApplyCommandFields(entity, request);

            if (!string.IsNullOrWhiteSpace(request.Recommendation)
                && entity.Status == ReviewRenewalStatus.PendingReview)
            {
                entity.Status = ReviewRenewalStatus.PendingApproval;
            }

            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ReviewRenewalEntity",
                entity.Id,
                oldValue,
                ReviewRenewalMapper.ToLogObject(entity),
                "Cập nhật đánh giá gia hạn hợp đồng");

            return true;
        }
    }

    public class ApproveReviewRenewalCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class ApproveReviewRenewalCommandHandler : IRequestHandler<ApproveReviewRenewalCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public ApproveReviewRenewalCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(ApproveReviewRenewalCommand request, CancellationToken cancellationToken)
        {
            ReviewRenewalEntity? entity = await _context.ReviewRenewalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (ReviewRenewalStatus.PendingReview or ReviewRenewalStatus.PendingApproval))
            {
                throw new InvalidOperationException("Chỉ duyệt đánh giá đang chờ xử lý.");
            }

            if (string.IsNullOrWhiteSpace(entity.Recommendation))
            {
                throw new InvalidOperationException("Chưa có phương án đề xuất để duyệt.");
            }

            object oldValue = ReviewRenewalMapper.ToLogObject(entity);
            entity.Status = ReviewRenewalStatus.Approved;
            entity.ApprovedBy = _currentUser.Username ?? _currentUser.UserCode ?? "System";
            entity.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.APPROVE,
                "ReviewRenewalEntity",
                entity.Id,
                oldValue,
                ReviewRenewalMapper.ToLogObject(entity),
                "Duyệt đánh giá gia hạn hợp đồng");

            return true;
        }
    }

    public class RejectReviewRenewalCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string? Note { get; set; }
    }

    public class RejectReviewRenewalCommandHandler : IRequestHandler<RejectReviewRenewalCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly ICurrentUserService _currentUser;

        public RejectReviewRenewalCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            ICurrentUserService currentUser)
        {
            _context = context;
            _actionLog = actionLog;
            _currentUser = currentUser;
        }

        public async Task<bool> Handle(RejectReviewRenewalCommand request, CancellationToken cancellationToken)
        {
            ReviewRenewalEntity? entity = await _context.ReviewRenewalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);
            if (entity == null)
            {
                return false;
            }

            if (entity.Status is not (ReviewRenewalStatus.PendingReview or ReviewRenewalStatus.PendingApproval))
            {
                throw new InvalidOperationException("Chỉ từ chối đánh giá đang chờ xử lý.");
            }

            object oldValue = ReviewRenewalMapper.ToLogObject(entity);
            entity.Status = ReviewRenewalStatus.Rejected;
            entity.ApprovedBy = _currentUser.Username ?? _currentUser.UserCode ?? "System";
            entity.ApprovedDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                entity.Note = request.Note.Trim();
            }
            entity.UpdatedAt = DateTime.UtcNow;

            _ = await _context.SaveChangesAsync(cancellationToken);
            await _actionLog.LogActionAsync(
                ActionType.REJECT,
                "ReviewRenewalEntity",
                entity.Id,
                oldValue,
                ReviewRenewalMapper.ToLogObject(entity),
                "Từ chối đánh giá gia hạn hợp đồng");

            return true;
        }
    }

    public class ApplyReviewRenewalCommand : IRequest<Guid?>
    {
        public Guid Id { get; set; }
        public string? NewContractCode { get; set; }
    }

    public class ApplyReviewRenewalCommandHandler : IRequestHandler<ApplyReviewRenewalCommand, Guid?>
    {
        private readonly IApplicationDbContext _context;
        private readonly IActionLogService _actionLog;
        private readonly IMediator _mediator;

        public ApplyReviewRenewalCommandHandler(
            IApplicationDbContext context,
            IActionLogService actionLog,
            IMediator mediator)
        {
            _context = context;
            _actionLog = actionLog;
            _mediator = mediator;
        }

        public async Task<Guid?> Handle(ApplyReviewRenewalCommand request, CancellationToken cancellationToken)
        {
            ReviewRenewalEntity entity = await _context.ReviewRenewalEntities
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
                ?? throw new InvalidOperationException("Đánh giá không tồn tại.");

            if (entity.Status != ReviewRenewalStatus.Approved)
            {
                throw new InvalidOperationException("Chỉ áp dụng đánh giá đã được duyệt.");
            }

            if (string.IsNullOrWhiteSpace(entity.Recommendation))
            {
                throw new InvalidOperationException("Không có phương án đề xuất để áp dụng.");
            }

            object oldValue = ReviewRenewalMapper.ToLogObject(entity);
            Guid? newContractId = null;

            string recommendation = entity.Recommendation.Trim().ToUpperInvariant();
            if (recommendation is ReviewRecommendation.Renew or ReviewRecommendation.Convert or ReviewRecommendation.IncreaseSalary)
            {
                newContractId = await _mediator.Send(new RenewContractCommand
                {
                    Id = entity.ContractId,
                    Code = request.NewContractCode,
                    ContractTypeId = entity.ProposedContractTypeId,
                    StartDate = entity.ProposedStartDate,
                    EndDate = entity.ProposedEndDate,
                    BasicSalary = entity.ProposedBasicSalary,
                    SignImmediately = true,
                    Note = entity.Note
                }, cancellationToken);
                entity.NewContractId = newContractId;
            }
            else if (recommendation == ReviewRecommendation.Terminate)
            {
                _ = await _mediator.Send(new TerminateContractCommand
                {
                    Id = entity.ContractId,
                    TerminationDate = entity.ProposedStartDate ?? DateTime.UtcNow.Date,
                    TerminationReason = entity.ReviewComment ?? entity.Note ?? "Chấm dứt theo kết quả đánh giá gia hạn",
                    SyncEmployeeResignation = true
                }, cancellationToken);
            }
            else if (recommendation == ReviewRecommendation.NoChange)
            {
                // Không tạo HĐ mới
            }
            else
            {
                throw new InvalidOperationException("Phương án đề xuất không hợp lệ.");
            }

            entity.Status = ReviewRenewalStatus.Applied;
            entity.UpdatedAt = DateTime.UtcNow;
            _ = await _context.SaveChangesAsync(cancellationToken);

            await _actionLog.LogActionAsync(
                ActionType.UPDATE,
                "ReviewRenewalEntity",
                entity.Id,
                oldValue,
                ReviewRenewalMapper.ToLogObject(entity),
                "Áp dụng đánh giá gia hạn hợp đồng");

            return newContractId;
        }
    }
}
