﻿using GoldEx.Client.Pages.Calculate.Components;
using GoldEx.Client.Pages.Home.Components;
using GoldEx.Client.Pages.Products.Components;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Home;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new BreadcrumbItem("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home)
    ];
    private RenderFragment? _productListContent;
    private RenderFragment? _priceBoardContent;
    private RenderFragment? _calculatorContent;

    private async Task OnProductsListExpandedChanged(bool newVal)
    {
        if (newVal)
        {
            await Task.Delay(0);
            _productListContent = builder =>
            {
                builder.OpenComponent(0, typeof(ProductsList));
                builder.CloseComponent();
            };
            StateHasChanged();
        }
        else
        {
            Task.Delay(350).ContinueWith(t => { _productListContent = null; StateHasChanged(); }).CatchAndLog();
            StateHasChanged();
        }
    }

    private async Task OnPriceBoardExpandedChanged(bool newVal)
    {
        if (newVal)
        {
            await Task.Delay(0);
            _priceBoardContent = builder =>
            {
                builder.OpenComponent(0, typeof(PriceBoard));
                builder.CloseComponent();
            };
            StateHasChanged();
        }
        else
        {
            Task.Delay(350).ContinueWith(t => { _priceBoardContent = null; StateHasChanged(); }).CatchAndLog();
            StateHasChanged();
        }
    }

    private async Task OnCalculatorExpandedChanged(bool newVal)
    {
        if (newVal)
        {
            await Task.Delay(0);
            _calculatorContent = builder =>
            {
                builder.OpenComponent(0, typeof(Calculator));
                builder.CloseComponent();
            };
            StateHasChanged();
        }
        else
        {
            Task.Delay(350).ContinueWith(t => { _calculatorContent = null; StateHasChanged(); }).CatchAndLog();
            StateHasChanged();
        }
    }
}