using Microsoft.EntityFrameworkCore;

namespace VfxApi.Models
{
    public class VfxContext : DbContext
    {
        public VfxContext(DbContextOptions<VfxContext> options)
        : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
    }
}
