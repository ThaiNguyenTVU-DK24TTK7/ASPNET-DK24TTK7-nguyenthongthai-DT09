using Ecommerce.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services
{
    public class MoMoPaymentExpirationService : BackgroundService
    {
        private static readonly TimeSpan PaymentTimeout = TimeSpan.FromMinutes(5);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MoMoPaymentExpirationService> _logger;

        public MoMoPaymentExpirationService(
            IServiceScopeFactory scopeFactory,
            ILogger<MoMoPaymentExpirationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var expiryTime = DateTime.UtcNow - PaymentTimeout;

                    var expiredOrders = await context.DonHangs
                        .Where(order => order.TrangThai == "Chờ thanh toán" && order.NgayTao <= expiryTime)
                        .ToListAsync(stoppingToken);

                    if (expiredOrders.Count == 0)
                    {
                        continue;
                    }

                    foreach (var order in expiredOrders)
                    {
                        order.TrangThai = "Đã hủy";
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không thể cập nhật các đơn MoMo hết hạn.");
                }
            }
        }
    }
}