using System;
using System.Collections.Generic;

namespace GoldEx.Shared.DTOs.Stores;

public record AssignStoreUsersRequest(List<Guid> UserIds);
