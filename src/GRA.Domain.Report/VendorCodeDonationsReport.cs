﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GRA.Domain.Model;
using GRA.Domain.Report.Abstract;
using GRA.Domain.Report.Attribute;
using GRA.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace GRA.Domain.Report
{
    [ReportInformation(ReportId,
        "Vendor Code Donations Report",
        "Vendor prize donations filterable by system.",
        "Participants")]
    public class VendorCodeDonationsReport : BaseReport
    {
        public const int ReportId = 15;

        private readonly IBranchRepository _branchRepository;
        private readonly IProgramRepository _programRepository;
        private readonly ISystemRepository _systemRepository;
        private readonly IUserRepository _userRepository;

        public VendorCodeDonationsReport(ILogger<VendorCodeDonationsReport> logger,
            ServiceFacade.Report serviceFacade,
            IBranchRepository branchRepository,
            IProgramRepository programRepository,
            ISystemRepository systemRepository,
            IUserRepository userRepository) : base(logger, serviceFacade)
        {
            ArgumentNullException.ThrowIfNull(branchRepository);
            ArgumentNullException.ThrowIfNull(programRepository);
            ArgumentNullException.ThrowIfNull(systemRepository);
            ArgumentNullException.ThrowIfNull(userRepository);

            _branchRepository = branchRepository;
            _programRepository = programRepository;
            _systemRepository = systemRepository;
            _userRepository = userRepository;
        }

        public override async Task ExecuteAsync(ReportRequest request,
            CancellationToken token,
            IProgress<JobStatus> progress = null)
        {
            #region Reporting initialization

            ArgumentNullException.ThrowIfNull(request);

            request = await StartRequestAsync(request);

            var criterion = await _serviceFacade.ReportCriterionRepository
                    .GetByIdAsync(request.ReportCriteriaId)
                ?? throw new GraException($"Report criteria {request.ReportCriteriaId} for report request id {request.Id} could not be found.");

            if (criterion.SiteId == null)
            {
                throw new ArgumentException(nameof(criterion.SiteId));
            }

            string title = null;

            if (criterion.SystemId.HasValue)
            {
                title = (await _systemRepository.GetByIdAsync(criterion.SystemId.Value)).Name;
            }

            var report = new StoredReport(title ?? _reportInformation.Name,
                _serviceFacade.DateTimeProvider.Now);
            var reportData = new List<object[]>();

            #endregion Reporting initialization

            #region Collect data

            UpdateProgress(progress, 1, "Starting report...", request.Name);

            // header row
            report.HeaderRow = new object[] {
                criterion.SystemId.HasValue ? "Branch Name" : "System Name"
            };

            var programs = await _programRepository.GetAllAsync(criterion.SiteId.Value);
            foreach (var program in programs)
            {
                report.HeaderRow = report.HeaderRow.Append(program.Name);
            }

            report.HeaderRow = report.HeaderRow.Append("Total");

            int count = 0;

            var users = await _userRepository.GetUsersByCriterionAsync(criterion);

            if (criterion.SystemId.HasValue)
            {
                users = users.Where(_ => _.SystemId == criterion.SystemId);
                var branches = await _branchRepository.GetBySystemAsync(criterion.SystemId.Value);
                foreach (var branch in branches)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    UpdateProgress(progress,
                        ++count * 100 / branches.Count(),
                        $"Processing: {branch.SystemName} - {branch.Name}",
                        request.Name);

                    var branchUsers = users.Where(_ => _.BranchId == branch.Id);
                    IEnumerable<object> row = new object[]
                    {
                        branch.Name
                    };
                    foreach (var program in programs)
                    {
                        row = row.Append(branchUsers.Count(_ => _.ProgramId == program.Id));
                    }
                    row = row.Append(branchUsers.Count());
                    reportData.Add(row.ToArray());
                }
            }
            else
            {
                var systems = await _systemRepository.GetAllAsync(criterion.SiteId.Value);
                foreach (var system in systems)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    UpdateProgress(progress,
                        ++count * 100 / systems.Count(),
                        $"Processing: {system.Name}",
                        request.Name);

                    var systemUsers = users.Where(_ => _.SystemId == system.Id);
                    IEnumerable<object> row = new object[]
                    {
                        system.Name
                    };
                    foreach (var program in programs)
                    {
                        row = row.Append(systemUsers.Count(_ => _.ProgramId == program.Id));
                    }
                    row = row.Append(systemUsers.Count());
                    reportData.Add(row.ToArray());
                }
            }

            report.Data = reportData.ToArray();

            IEnumerable<object> footerRow = new object[]
            {
                "Total"
            };
            foreach (var program in programs)
            {
                footerRow = footerRow.Append(users.Count(_ => _.ProgramId == program.Id));
            }
            footerRow = footerRow.Append(users.Count());

            report.FooterRow = footerRow.ToArray();

            #endregion Collect data

            #region Finish up reporting

            if (!token.IsCancellationRequested)
            {
                ReportSet.Reports.Add(report);
            }
            await FinishRequestAsync(request, !token.IsCancellationRequested);

            #endregion Finish up reporting
        }
    }
}
