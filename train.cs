using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public class Train
{
    public int TrainID { get; set; }
    public string TrainNumber { get; set; }
    public string DepartureStation { get; set; }
    public string ArrivalStation { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
}

public class ApplicationContext : DbContext
{
    public DbSet<Train> Trains { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
    }
}

public class DatabaseService
{
    private DbContextOptions<ApplicationContext> GetConnectionOptions()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var config = builder.Build();
        string connectionString = config.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        return optionsBuilder.UseSqlServer(connectionString).Options;
    }

    public async Task EnsurePopulated()
    {
        using (var db = new ApplicationContext(GetConnectionOptions()))
        {
            await db.Database.EnsureCreatedAsync();

            if (!db.Trains.Any())
            {
                db.Trains.AddRange(Train.TestData);
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task AddTrain(Train train)
    {
        using (var db = new ApplicationContext(GetConnectionOptions()))
        {
            db.Trains.Add(train);
            await db.SaveChangesAsync();
        }
    }

    public async Task<Train> GetTrainById(int trainId)
    {
        using (var db = new ApplicationContext(GetConnectionOptions()))
        {
            return await db.Trains.FindAsync(trainId);
        }
    }

    public async Task UpdateTrain(Train updatedTrain)
    {
        using (var db = new ApplicationContext(GetConnectionOptions()))
        {
            var existingTrain = await db.Trains.FindAsync(updatedTrain.TrainID);
            if (existingTrain != null)
            {
                existingTrain.TrainNumber = updatedTrain.TrainNumber;
                existingTrain.DepartureStation = updatedTrain.DepartureStation;
                existingTrain.ArrivalStation = updatedTrain.ArrivalStation;
                existingTrain.DepartureTime = updatedTrain.DepartureTime;
                existingTrain.ArrivalTime = updatedTrain.ArrivalTime;

                await db.SaveChangesAsync();
            }
        }
    }

    public async Task DeleteTrain(int trainId)
    {
        using (var db = new ApplicationContext(GetConnectionOptions()))
        {
            var trainToDelete = await db.Trains.FindAsync(trainId);
            if (trainToDelete != null)
            {
                db.Trains.Remove(trainToDelete);
                await db.SaveChangesAsync();
            }
        }
    }
}

class Program
{
    static async Task Main()
    {
        var databaseService = new DatabaseService();
        await databaseService.EnsurePopulated();

        await databaseService.AddTrain(new Train
        {
            TrainNumber = "123",
            DepartureStation = "Station_A",
            ArrivalStation = "Station_B",
            DepartureTime = DateTime.Parse("2024-01-30 08:00:00"),
            ArrivalTime = DateTime.Parse("2024-01-30 12:00:00")
        });

        var train = await databaseService.GetTrainById(1);
        Console.WriteLine($"Train ID: {train.TrainID}, Number: {train.TrainNumber}, Departure: {train.DepartureStation}, Arrival: {train.ArrivalStation}");

        await databaseService.UpdateTrain(new Train
        {
            TrainID = 1,
            TrainNumber = "456",
            DepartureStation = "Station_C",
            ArrivalStation = "Station_D",
            DepartureTime = DateTime.Parse("2024-01-30 09:00:00"),
            ArrivalTime = DateTime.Parse("2024-01-30 13:00:00")
        });

        await databaseService.DeleteTrain(1);
    }
}
