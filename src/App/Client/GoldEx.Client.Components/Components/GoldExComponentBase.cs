using FluentValidation;
using GoldEx.Sdk.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Security.Claims;
using Severity = MudBlazor.Severity;

namespace GoldEx.Client.Components.Components;

public class GoldExComponentBase : ComponentBase, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;
    private int _busyCount;
    private bool _shouldRender = true;
    private IServiceScope? _currentScope;
    private IServiceScope CurrentScope
    {
        get
        {
            _currentScope ??= CreateServiceScope();
            return _currentScope;
        }
    }
    protected CancellationToken CancellationToken
    {
        get
        {
            _cancellationTokenSource ??= new CancellationTokenSource();

            return _cancellationTokenSource.Token;
        }
    }

    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar ToastService { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject] private IStringLocalizer<Resources.GoldExComponentBase> Localizer { get; set; } = default!;

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



    #region Render

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

    protected void ShouldNotRender()
    {
        _shouldRender = false;
    }

    #endregion

    #region Authentication

    private async void AuthenticationStateChanged(Task<AuthenticationState> task)
    {
        var state = await task;
        User = state.User;
    }

    protected async Task<bool> HasPolicyAsync(string policy)
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var result = await AuthorizationService.AuthorizeAsync(authState.User, policy);

        return result.Succeeded;
    }

    #endregion

    #region Toasts

    protected void AddSuccessToast(string message) => ToastService.Add(message, Severity.Success);

    protected void AddErrorToast(string message) => ToastService.Add(message, Severity.Error);

    protected void AddWarningToast(string message) => ToastService.Add(message, Severity.Warning);

    protected void AddInfoToast(string message) => ToastService.Add(message, Severity.Info);

    #endregion

    #region Service Scope

    protected TService GetRequiredService<TService>() where TService : notnull
    {
        return GetRequiredService<TService>(CurrentScope);
    }

    protected IEnumerable<TService> GetServices<TService>() where TService : notnull
    {
        return CurrentScope.ServiceProvider.GetServices<TService>();
    }

    protected TService GetRequiredService<TService>(IServiceScope scope) where TService : notnull
    {
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected IServiceScope CreateServiceScope() => ServiceScopeFactory.CreateScope();

    #endregion

    #region Busy State

    protected void SetBusy(bool stateChanged = false)
    {
        Interlocked.Increment(ref _busyCount);

        if (stateChanged)
            StateHasChanged();
    }

    protected void SetIdeal(bool stateChanged = false)
    {
        Interlocked.Decrement(ref _busyCount);

        if (stateChanged)
            StateHasChanged();
    }

    #endregion

    #region Cancellation Token

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

    #endregion

    #region Send Request

    protected async Task<TResult> SendRequestAsync<TService, TResult, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
                                                                                 Func<TResponse, Task<TResult>> afterSend,
                                                                                 Func<Task>? onFailure = null)
        where TService : notnull
    {
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>();

            var response = await action.Invoke(service, CancellationToken);
            return await afterSend.Invoke(response);
        }
        catch (Exception ex)
        {
            if (onFailure != null)
            {
                await onFailure.Invoke();
            }
            HandleRequestException(ex);
            return default!;
        }
        finally
        {
            SetIdeal();
        }
    }

    protected async Task<TResult> SendRequestAsync<TService, TResult, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
                                                                                 Func<TResponse, TResult> afterSend,
                                                                                 Action? onFailure = null)
        where TService : notnull
    {
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>();

            var response = await action.Invoke(service, CancellationToken);
            return afterSend.Invoke(response);
        }
        catch (Exception ex)
        {
            if (onFailure != null)
            {
                onFailure.Invoke();
            }
            HandleRequestException(ex);
            return default!;
        }
        finally
        {
            SetIdeal();
        }
    }

    protected async Task SendRequestAsync<TService, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
                                                               Func<TResponse, Task> afterSend,
                                                               Func<Task>? onFailure = null)
        where TService : notnull
    {
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>();

            var response = await action.Invoke(service, CancellationToken);
            await afterSend.Invoke(response);
        }
        catch (Exception ex)
        {
            if (onFailure != null)
            {
                await onFailure.Invoke();
            }
            HandleRequestException(ex);
        }
        finally
        {
            SetIdeal();
        }
    }

    protected async Task SendRequestAsync<TService, TResponse>(Func<TService, CancellationToken, Task<TResponse>> action,
                                                               Action<TResponse> afterSend,
                                                               Action? onFailure = null,
                                                               bool createScope = false)
        where TService : notnull
    {
        var scope = createScope ? CreateServiceScope() : CurrentScope;
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>(scope);

            var response = await action.Invoke(service, CancellationToken);
            afterSend.Invoke(response);
        }
        catch (Exception ex)
        {
            onFailure?.Invoke();
            HandleRequestException(ex);
        }
        finally
        {
            if (createScope)
                scope.Dispose();

            SetIdeal();
        }
    }

    protected async Task<TResult?> SendRequestAsync<TService, TResult>(Func<TService, CancellationToken, Task<TResult>> action, bool createScope = false)
        where TService : notnull
    {
        var scope = createScope ? CreateServiceScope() : CurrentScope;
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>(scope);

            return await action.Invoke(service, CancellationToken);
        }
        catch (Exception ex)
        {
            HandleRequestException(ex);

            return default(TResult);
        }
        finally
        {
            if (createScope)
                scope.Dispose();

            SetIdeal();
        }
    }

    protected async Task<bool> SendRequestAsync<TService>(Func<TService, CancellationToken, Task> action,
                                                    Func<Task>? afterSend = null,
                                                    Func<Task>? onFailure = null)
        where TService : notnull
    {
        try
        {
            CancelToken();
            SetBusy();
            var service = GetRequiredService<TService>();

            await action.Invoke(service, CancellationToken);
            if (afterSend != null)
            {
                await afterSend.Invoke();
            }

            return true;
        }
        catch (Exception ex)
        {
            if (onFailure != null)
            {
                await onFailure.Invoke();
            }

            HandleRequestException(ex);

            return false;
        }
        finally
        {
            SetIdeal();
        }
    }

    #endregion

    #region Exception handling

    private void HandleRequestException(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException notFoundException:
               AddErrorToast(string.IsNullOrEmpty(notFoundException.Message) ? "هیچ اطلاعاتی یافت نشد" : notFoundException.Message);
                break;

            case ValidationException validationException:
                foreach (var error in validationException.Errors)
                {
                    AddErrorToast(error.ErrorMessage);
                }
                break;
            case HttpRequestValidationException validationException:
                if (validationException.Errors.Any())
                {
                    foreach (var error in validationException.Errors)
                    {
                        AddErrorToast(string.Join('\n', error.Value));
                    }
                }
                else
                {
                    AddErrorToast("خطای اعتبار سنجی");
                }
                break;
            case OperationCanceledException:
                // Operation was canceled, do nothing
                break;
            case HttpRequestFailedException or HttpRequestException:
                AddErrorToast("عدم امکان برقراری ارتباط با سرور");
                break;
            default:
                AddErrorToast($"{ex.GetType()}. {Localizer[ex.Message]}");
                break;
        }

//#if DEBUG
//        throw ex;
//#endif
    }

    #endregion

    public virtual void Dispose()
    {
        CancelToken();

        CurrentScope.Dispose();
        AuthenticationStateProvider.AuthenticationStateChanged -= AuthenticationStateChanged;
    }
}