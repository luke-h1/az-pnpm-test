﻿#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IReleaseCacheService
{
    Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug = null);

    Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(bool staging,
        DateTime expectedPublishDate,
        Guid releaseId,
        string publicationSlug,
        string? releaseSlug = null);
}
