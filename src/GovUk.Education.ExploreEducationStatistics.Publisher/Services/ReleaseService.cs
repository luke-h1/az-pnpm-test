#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Extensions.PublisherExtensions;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IMethodologyService _methodologyService;

        public ReleaseService(ContentDbContext contentDbContext,
            IMethodologyService methodologyService)
        {
            _contentDbContext = contentDbContext;
            _methodologyService = methodologyService;
        }

        public async Task<Release> Get(Guid id)
        {
            return await _contentDbContext.Releases
                .SingleAsync(release => release.Id == id);
        }

        public async Task<IEnumerable<Release>> List(IEnumerable<Guid> ids)
        {
            return await _contentDbContext.Releases
                .AsQueryable()
                .Where(release => ids.Contains(release.Id))
                .Include(release => release.Publication)
                .Include(release => release.PreviousVersion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Release>> GetAmendedReleases(IEnumerable<Guid> releaseIds)
        {
            return await _contentDbContext.Releases
                .Include(r => r.PreviousVersion)
                .Include(r => r.Publication)
                .Where(r => releaseIds.Contains(r.Id) && r.PreviousVersionId != null)
                .ToListAsync();
        }

        public async Task<Release> GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds)
        {
            var releases = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .Where(release => release.PublicationId == publicationId)
                .ToListAsync();

            return releases
                .Where(release => release.IsReleasePublished(includedReleaseIds))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimePeriodCoverage)
                .Last();
        }

        public async Task SetPublishedDates(Guid id, DateTime published)
        {
            var release = await _contentDbContext.Releases
                .SingleOrDefaultAsync(r => r.Id == id);

            if (release == null)
            {
                throw new ArgumentException("Content Release does not exist", nameof(id));
            }

            if (release.Amendment)
            {
                var previousVersion = await _contentDbContext.Releases.AsNoTracking()
                    .SingleOrDefaultAsync(r => r.Id == release.PreviousVersionId);

                if (previousVersion?.Published == null)
                {
                    throw new ArgumentException("Previous version of release does not exist or is not live",
                        nameof(release.PreviousVersionId));
                }

                published = previousVersion.Published.Value;
            }

            _contentDbContext.Releases.Update(release);
            release.Published ??= published;

            // Set the published date on any methodologies used by this publication that are now publicly accessible
            // as a result of this release being published
            await _methodologyService.SetPublishedDatesByPublication(release.PublicationId, published);

            await _contentDbContext.SaveChangesAsync();
        }

        public async Task<List<File>> GetFiles(Guid releaseId, params FileType[] types)
        {
            return await _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf => rf.ReleaseId == releaseId)
                .Select(rf => rf.File)
                .Where(file => types.Contains(file.Type))
                .ToListAsync();
        }
    }
}
