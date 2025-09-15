using Microsoft.EntityFrameworkCore;

namespace WebApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
        public DbSet<Booking> Bookings => Set<Booking>();

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<MeetingRoom>().HasData(
        //        new MeetingRoom { Id = 1, Name = "Room A", Capacity = 5 },
        //        new MeetingRoom { Id = 2, Name = "Room B", Capacity = 10 }
        //    );


        //}
    }
}
