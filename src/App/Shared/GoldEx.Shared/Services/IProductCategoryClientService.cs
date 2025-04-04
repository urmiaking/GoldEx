﻿using GoldEx.Shared.DTOs.Categories;

namespace GoldEx.Shared.Services;

public interface IProductCategoryClientService
{
    Task<List<GetCategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GetCategoryResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<List<GetPendingCategoryResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}