using Microsoft.EntityFrameworkCore;
using SnookerGameManagementSystem.Data;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Services
{
    public class FrameService
    {
        private readonly SnookerDbContext _context;

        public FrameService(SnookerDbContext context)
        {
            _context = context;
        }

        public async Task<Frame> CreateFrameAsync(
            Guid sessionId,
            List<Guid> playerIds,
            decimal baseRate)
        {
            var frame = new Frame
            {
                SessionId = sessionId,
                StartedAt = DateTime.Now,
                BaseRatePk = baseRate,
                PayerMode = PayerMode.LOSER,
                PayStatus = PayStatus.UNPAID
            };

            _context.Frames.Add(frame);
            await _context.SaveChangesAsync();

            // Add participants
            foreach (var playerId in playerIds)
            {
                var participant = new FrameParticipant
                {
                    FrameId = frame.Id,
                    CustomerId = playerId
                };
                _context.FrameParticipants.Add(participant);
            }

            await _context.SaveChangesAsync();

            return frame;
        }

        public async Task<Frame> EndFrameAsync(
            Guid frameId,
            Guid winnerId,
            Guid? loserId,
            int overtimeMinutes = 0,
            decimal overtimeAmount = 0,
            decimal discount = 0)
        {
            var frame = await _context.Frames
                .Include(f => f.Participants)
                .FirstOrDefaultAsync(f => f.Id == frameId);

            if (frame == null)
                throw new Exception("Frame not found");

            frame.EndedAt = DateTime.Now;
            frame.WinnerCustomerId = winnerId;
            frame.LoserCustomerId = loserId;
            frame.OvertimeMinutes = overtimeMinutes;
            frame.OvertimeAmountPk = overtimeAmount;
            frame.DiscountPk = discount;
            frame.TotalAmountPk = frame.BaseRatePk + overtimeAmount - discount;

            // Update participants
            foreach (var participant in frame.Participants)
            {
                participant.IsWinner = participant.CustomerId == winnerId;
            }

            await _context.SaveChangesAsync();

            return frame;
        }

        public async Task<List<Frame>> GetSessionFramesAsync(Guid sessionId)
        {
            return await _context.Frames
                .Include(f => f.Participants)
                    .ThenInclude(p => p.Customer)
                .Where(f => f.SessionId == sessionId)
                .OrderBy(f => f.StartedAt)
                .ToListAsync();
        }
    }
}
