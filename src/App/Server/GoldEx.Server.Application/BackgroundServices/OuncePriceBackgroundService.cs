﻿using Microsoft.Extensions.Hosting;

namespace GoldEx.Server.Application.BackgroundServices;

public class OuncePriceBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}