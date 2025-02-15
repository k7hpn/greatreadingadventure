﻿using System.Collections.Generic;
using GRA.Controllers.ViewModel.Shared;

namespace GRA.Controllers.ViewModel.MissionControl.Triggers
{
    public class TriggersListViewModel
    {
        public IEnumerable<GRA.Domain.Model.Trigger> Triggers { get; set; }
        public PaginateViewModel PaginateModel { get; set; }
        public string Search { get; set; }
        public int? SystemId { get; set; }
        public int? BranchId { get; set; }
        public int? ProgramId { get; set; }
        public bool? HideLowPoint { get; set; }
        public bool? Mine { get; set; }
        public bool? LowPoints { get; set; }
        public string ActiveNav { get; set; }
        public string SystemName { get; set; }
        public string BranchName { get; set; }
        public string ProgramName { get; set; }

        public IEnumerable<GRA.Domain.Model.Branch> BranchList { get; set; }
        public IEnumerable<GRA.Domain.Model.System> SystemList { get; set; }
        public IEnumerable<GRA.Domain.Model.Program> ProgramList { get; set; }
    }
}
