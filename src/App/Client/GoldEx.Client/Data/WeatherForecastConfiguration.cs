using GoldEx.Client.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Data;

public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecast>
{
    public void Configure(EntityTypeBuilder<WeatherForecast> builder)
    {
        builder.HasIndex(w => w.TemperatureC);

        builder
            .HasData(new WeatherForecast
            {
                Id = 1,
                Date = new DateTimeOffset(2024, 1, 1, 10, 10, 10, TimeSpan.Zero),
                TemperatureC = 30,
                Summary = "Hot"
            }, new WeatherForecast
            {
                Id = 2,
                Date = new DateTimeOffset(2024, 1, 2, 10, 10, 10, TimeSpan.Zero),
                TemperatureC = 20,
                Summary = "Normal"
            }, new WeatherForecast
            {
                Id = 3,
                Date = new DateTimeOffset(2024, 1, 3, 10, 10, 10, TimeSpan.Zero),
                TemperatureC = 10,
                Summary = "Cold"
            });
    }
}
