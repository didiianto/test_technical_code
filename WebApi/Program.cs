using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Seed Data

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    SeedData(context);
}

//add cors
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseCors("OpenCors)");
//Get All
app.MapGet("/api/meetingrooms", async (AppDbContext context) =>
{
    return await context.MeetingRooms.ToListAsync();
});

//post
app.MapPost("/api/meetingrooms", async (AppDbContext context, BookingDto dto) =>
{
    var room = await context.MeetingRooms.Include(x => x.bookings).FirstOrDefaultAsync(x => x.Id == dto.meetingRoomId);

    if(room == null) return Results.NotFound();
    if (dto.peeople > room.Capacity) return Results.BadRequest("Not enough capacity for that many people");

    var start = dto.Start;
    var end = dto.Start.AddMinutes(dto.DurationMinutes);

    bool conflict = room.bookings.Any(x => x.Start < end && x.End > start);
    if (conflict) return Results.BadRequest("Room is already booked");

    var booking = new Booking { Start = start, End = end, peeople = dto.peeople };
    room.bookings.Add(booking);
    await context.SaveChangesAsync();
    return Results.Created($"/api/meetingrooms/{room.Id}", room);

});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void SeedData(AppDbContext context)
{
    if (!context.MeetingRooms.Any())
    {
        var today = DateTime.Today;

        var roomA = new MeetingRoom { Name = "Room A", Capacity = 5 };
        var roomB = new MeetingRoom { Name = "Room B", Capacity = 10 };

        roomA.bookings.AddRange(new List<Booking>

        {
            new Booking { Start = today.AddHours(7), End = today.AddHours(9), peeople = 3 },
            new Booking { Start = today.AddHours(9).AddMinutes(45), End = today.AddHours(10).AddMinutes(35), peeople = 2 }
        });

        roomB.bookings.AddRange(new List<Booking>
        {
            new Booking { Start = today.AddHours(7), End = today.AddHours(9), peeople = 3 },
            new Booking { Start = today.AddHours(9).AddMinutes(45), End = today.AddHours(10).AddMinutes(35), peeople = 2 }
        });

        context.MeetingRooms.AddRange(roomA, roomB);
        context.SaveChanges();
    }
}

public class MeetingRoom
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Capacity { get; set; }
    public List<Booking> bookings { get; set; } = new List<Booking>();
}
record BookingDto(int Id, int meetingRoomId, DateTime Start, int DurationMinutes, int peeople);
public class Booking
{
    public int Id { get; set; }
    public int meetingRoomId { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int peeople { get; set; }
    [JsonIgnore]
    public MeetingRoom? MeetingRoom { get; set; }
}