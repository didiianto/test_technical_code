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
    context.Database.EnsureDeleted();
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
    return await context.MeetingRooms.Include(r => r.bookings).ToListAsync();
});

//post
app.MapPost("/api/meetingrooms", async (AppDbContext context, BookingDto dto) =>
{
    if (dto.peeople < 2)
        return Results.BadRequest("Meeting must have at least 2 participants");

    var selectedRoom = await context.MeetingRooms
        .Include(r => r.bookings)
        .FirstOrDefaultAsync(r => r.Id == dto.meetingRoomId);

    if (selectedRoom == null)
        return Results.NotFound("Meeting room not found");

    if (dto.peeople > selectedRoom.Capacity)
        return Results.BadRequest($"Room capacity is only {selectedRoom.Capacity} people");

    var start = dto.Start;
    var end = dto.Start.AddMinutes(dto.DurationMinutes);

    bool conflict = selectedRoom.bookings.Any(x => x.Start < end && x.End > start);
    if (!conflict)
    {
        var booking = new Booking { Start = start, End = end, peeople = dto.peeople };
        selectedRoom.bookings.Add(booking);
        await context.SaveChangesAsync();
        return Results.Created($"/api/meetingrooms/{selectedRoom.Id}", selectedRoom);
    }

    var candidateRooms = await context.MeetingRooms
        .Include(r => r.bookings)
        .Where(r => r.Capacity >= dto.peeople)
        .ToListAsync();

    var suggestions = new List<object>();
    foreach (var room in candidateRooms)
    {
        var nextFree = FindNextAvailableSlot(room, start, dto.DurationMinutes);
        if (nextFree != null)
        {
            suggestions.Add(new
            {
                RoomId = room.Id,
                RoomName = room.Name,
                SuggestedStart = nextFree.Value,
                SuggestedEnd = nextFree.Value.AddMinutes(dto.DurationMinutes)
            });
        }
    }

    return Results.BadRequest(new
    {
        Message = "Requested slot is not available",
        Suggestions = suggestions
    });
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
            new Booking { Start = today.AddHours(9).AddMinutes(45),
                          End   = today.AddHours(10).AddMinutes(30),
                          peeople = 2 }
        });

        roomB.bookings.AddRange(new List<Booking>
        {
            new Booking { Start = today.AddHours(7), End = today.AddHours(9), peeople = 5 },
            new Booking { Start = today.AddHours(9).AddMinutes(45),
                          End   = today.AddHours(10).AddMinutes(30),
                          peeople = 4 }
        });

        context.MeetingRooms.AddRange(roomA, roomB);
        context.SaveChanges();
    }
}

DateTime? FindNextAvailableSlot(MeetingRoom room, DateTime desiredStart, int durationMinutes)
{
    var desiredEnd = desiredStart.AddMinutes(durationMinutes);
    var bookings = room.bookings.OrderBy(b => b.Start).ToList();

    DateTime pointer = desiredStart;

    foreach (var b in bookings)
    {
        if (pointer < b.Start && pointer.AddMinutes(durationMinutes) <= b.Start)
            return pointer;

        if (pointer < b.End)
            pointer = b.End;
    }

    var workDayEnd = desiredStart.Date.AddHours(16).AddMinutes(30);
    if (pointer.AddMinutes(durationMinutes) <= workDayEnd)
        return pointer;

    return null;
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