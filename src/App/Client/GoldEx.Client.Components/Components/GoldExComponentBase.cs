using GoldEx.Sdk.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Security.Claims;
using GoldEx.Sdk.Client.Abstractions;

namespace GoldEx.Client.Components.Components;

public class GoldExComponentBase : ComponentBase, IDisposable
{
    
    private CancellationTokenSource? _cancellationTokenSource;
    private IServiceScope? _serviceScope;
    private int _busyCount;
    private bool _shouldRender = true;

    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar ToastService { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject] private IStringLocalizer<Resources.GoldExComponentBase> Localizer { get; set; } = default!;
    [Inject] private IBusyIndicator BusyIndicator { get; set; } = default!;

    public bool IsBusy => _busyCount > 0;
    protected ClaimsPrincipal? User { get; private set; }
    protected CancellationTokenSource CancellationTokenSource
    {
        get
        {
            _cancellationTokenSource ??= new CancellationTokenSource();

            return _cancellationTokenSource;
        }
    }

    protected Guid? UserId
    {
        get
        {
            var idClaim = User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (Guid.TryParse(idClaim?.Value, out var id))
                return id;

            return null;

        }
    }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        User = state.User;

        AuthenticationStateProvider.AuthenticationStateChanged += AuthenticationStateChanged;

        await base.OnInitializedAsync();
    }

    protected override bool ShouldRender()
    {
        // Check the flag, and if it is false, return false
        // this is a one-time flag, and will be reset to true, for future renders.
        if (!_shouldRender)
        {
            _shouldRender = true;
            return false;
        }

        return base.ShouldRender();
    }

    public virtual void Dispose()
    {
        CancelToken();

        _serviceScope?.Dispose();
        AuthenticationStateProvider.AuthenticationStateChanged -= AuthenticationStateChanged;
    }

    protected void ShouldNotRender()
    {
        _shouldRender = false;
    }

    protected void CancelToken()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();

            DestroyCancellationToken();
        }
    }

    protected void DestroyCancellationToken()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var state = await task;
        User = state.User;
    }

    protected void AddExceptionToast(Exception exception)
    {
        if (exception is TaskCanceledException)
        {
            return;
        }

        if (exception is FluentValidation.ValidationException fluentValidationException)
        {
            foreach (var error in fluentValidationException.Errors)
            {
                AddErrorToast(error.ErrorMessage);
            }
        }
        else switch (exception)
        {
            case HttpRequestValidationException requestValidationException:
            {
                if (requestValidationException.Errors.Any())
                {
                    foreach (var error in requestValidationException.Errors)
                    {
                        AddErrorToast(string.Join('\n', error.Value));
                    }
                }
                else
                {
                    AddErrorToast("خطای اعتبار سنجی");
                }

                break;
            }
            case HttpRequestAuthenticationFailedException or HttpRequestAuthorizationFailedException:
                AddErrorToast("شما فاقد مجوز کافی برای انجام این عملیات میباشید.");
                break;
            case HttpRequestFailedException or HttpRequestException:
                AddErrorToast("عدم امکان برقراری ارتباط با سرور");
                break;
            default:
                AddErrorToast($"{exception.GetType()}. {Localizer[exception.Message]}");
                break;
        }

#if !RELEASE
        throw exception;
#endif
    }

    protected void AddSuccessToast(string message) => ToastService.Add(message, Severity.Success);

    protected void AddErrorToast(string message) => ToastService.Add(message, Severity.Error);

    protected void AddWarningToast(string message) => ToastService.Add(message, Severity.Warning);

    protected void AddInfoToast(string message) => ToastService.Add(message, Severity.Info);

    protected async Task<bool> HasPolicyAsync(string policy)
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var result = await AuthorizationService.AuthorizeAsync(authState.User, policy);

        return result.Succeeded;
    }

    protected TService GetRequiredService<TService>() where TService : notnull
    {
        _serviceScope ??= CreateServiceScope();

        return GetRequiredService<TService>(_serviceScope);
    }

    protected TService GetRequiredService<TService>(IServiceScope scope) where TService : notnull
    {
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected void SetBusy(bool stateChanged = false)
    {
        Interlocked.Increment(ref _busyCount);
        BusyIndicator.SetBusy();

        if (stateChanged)
            StateHasChanged();
    }

    protected void SetIdeal(bool stateChanged = false)
    {
        Interlocked.Decrement(ref _busyCount);
        BusyIndicator.SetIdeal();

        if (stateChanged)
            StateHasChanged();
    }

    protected IServiceScope CreateServiceScope() => ServiceScopeFactory.CreateScope();
}