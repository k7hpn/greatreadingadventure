﻿using System;
using GRA.Controllers.ViewModel.Shared;
using Microsoft.AspNetCore.Http;

namespace GRA.Controllers.ViewModel.MissionControl.Participants
{
    public class ParticipantPartialViewModel : SchoolSelectionViewModel
    {
        public ParticipantPartialViewModel()
        { }

        public ParticipantPartialViewModel(ParticipantPartialViewModel viewModel)
        {
            if (viewModel != null)
            {
                Id = viewModel.Id;
                HouseholdCount = viewModel.HouseholdCount;
                PrizeCount = viewModel.PrizeCount;
                RoleCount = viewModel.RoleCount;
                HasElevatedRole = viewModel.HasElevatedRole;
                HasAccount = viewModel.HasAccount;
                IsGroup = viewModel.IsGroup;
                EmailSubscriptionEnabled = viewModel.EmailSubscriptionEnabled;
                Action = viewModel.Action;
            }
        }

        public string Action { get; set; }
        public bool EmailSubscriptionEnabled { get; set; }
        public bool HasAccount { get; set; }
        public bool HasElevatedRole { get; set; }
        public int HouseholdCount { get; set; }
        public int Id { get; set; }
        public bool IsGroup { get; set; }
        public int PrizeCount { get; set; }
        public int RoleCount { get; set; }

        public string GetBadgeClass(Microsoft.AspNetCore.Mvc.Razor.RazorPage page, string action)
        {
            if (page == null || string.IsNullOrEmpty(action))
            {
                return "text-bg-primary";
            }

            return page
                .ViewContext
                .RouteData
                .Values["action"]
                .ToString()
                .Equals(action, StringComparison.OrdinalIgnoreCase)
                ? "text-bg-light" : "text-bg-primary";
        }
    }
}
