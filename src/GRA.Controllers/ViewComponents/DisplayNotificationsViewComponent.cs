﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GRA.Abstract;
using GRA.Controllers.ViewModel.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace GRA.Controllers.ViewComponents
{
    [ViewComponent(Name = "DisplayNotifications")]
    public class DisplayNotificationsViewComponent : ViewComponent
    {
        private const int MaxNotifications = 3;
        private readonly IPathResolver _pathResolver;
        private readonly IHtmlLocalizer<Resources.Shared> _sharedHtmlLocalizer;
        private readonly IStringLocalizer<Resources.Shared> _sharedLocalizer;

        public DisplayNotificationsViewComponent(IPathResolver pathResolver,
            IStringLocalizer<Resources.Shared> sharedLocalizer,
            IHtmlLocalizer<Resources.Shared> sharedHtmlLocalizer)
        {
            ArgumentNullException.ThrowIfNull(pathResolver);
            ArgumentNullException.ThrowIfNull(sharedHtmlLocalizer);
            ArgumentNullException.ThrowIfNull(sharedLocalizer);

            _pathResolver = pathResolver;
            _sharedHtmlLocalizer = sharedHtmlLocalizer;
            _sharedLocalizer = sharedLocalizer;
        }

        public IViewComponentResult Invoke()
        {
            var notifications =
                (List<Domain.Model.Notification>)HttpContext.Items[ItemKey.NotificationsList];
            var totalNotifications = notifications.Count;

            var notificationDisplayList = new List<GRA.Domain.Model.Notification>();
            int? totalPointsEarned = 0;
            var earnedBadge = false;

            foreach (var notification in notifications.Where(m => m.IsAchiever).ToList())
            {
                totalPointsEarned += notification.PointsEarned;
                if (notificationDisplayList.Count < MaxNotifications)
                {
                    if (!string.IsNullOrWhiteSpace(notification.BadgeFilename))
                    {
                        notification.BadgeFilename
                            = _pathResolver.ResolveContentPath(notification.BadgeFilename);
                        earnedBadge = true;
                    }
                    if (!string.IsNullOrWhiteSpace(notification.AttachmentFilename))
                    {
                        notification.AttachmentFilename
                            = _pathResolver.ResolveContentPath(notification.AttachmentFilename);
                    }
                    notificationDisplayList.Add(notification);
                }
                notifications.Remove(notification);
            }

            foreach (var notification in notifications.Where(m => m.IsJoiner).ToList())
            {
                totalPointsEarned += notification.PointsEarned;
                if (notificationDisplayList.Count < MaxNotifications)
                {
                    if (!string.IsNullOrWhiteSpace(notification.BadgeFilename))
                    {
                        notification.BadgeFilename
                            = _pathResolver.ResolveContentPath(notification.BadgeFilename);
                        earnedBadge = true;
                    }
                    if (!string.IsNullOrWhiteSpace(notification.AttachmentFilename))
                    {
                        notification.AttachmentFilename
                            = _pathResolver.ResolveContentPath(notification.AttachmentFilename);
                    }
                    notification.LocalizedText
                        = _sharedHtmlLocalizer[Annotations.Info.SuccessfullyJoined,
                            HttpContext.Items[ItemKey.SiteName]];
                    notification.DisplayIcon = "far fa-thumbs-up fa-fw";
                    notificationDisplayList.Add(notification);
                }
                notifications.Remove(notification);
            }

            foreach (var notification in notifications.Where(m => m.AvatarBundleId != null).ToList())
            {
                totalPointsEarned += notification.PointsEarned;
                if (notificationDisplayList.Count < MaxNotifications)
                {
                    if (!string.IsNullOrWhiteSpace(notification.BadgeFilename))
                    {
                        notification.BadgeFilename
                            = _pathResolver.ResolveContentPath(notification.BadgeFilename);
                        earnedBadge = true;
                    }
                    if (!string.IsNullOrWhiteSpace(notification.AttachmentFilename))
                    {
                        notification.AttachmentFilename
                            = _pathResolver.ResolveContentPath(notification.AttachmentFilename);
                    }
                    notification.DisplayIcon = "far fa-thumbs-up fa-fw";
                    notification.Text = new StringBuilder(notification.Text)
                        .AppendFormat(CultureInfo.InvariantCulture,
                            " <a href=\"{0}\">Check out your new avatar options!</a>",
                            Url.Action(nameof(AvatarController.Index),
                                AvatarController.Name,
                                new { bundle = notification.AvatarBundleId }))
                        .ToString();

                    notificationDisplayList.Add(notification);
                }
                notifications.Remove(notification);
            }

            var profileLink = Url.Action(nameof(ProfileController.History), ProfileController.Name);

            foreach (var notification in notifications
                .Where(m => !string.IsNullOrWhiteSpace(m.BadgeFilename))
                .OrderByDescending(m => m.PointsEarned)
                .ThenByDescending(m => m.CreatedAt).ToList())
            {
                totalPointsEarned += notification.PointsEarned;
                if (notificationDisplayList.Count < MaxNotifications)
                {
                    notification.BadgeFilename
                        = _pathResolver.ResolveContentPath(notification.BadgeFilename);
                    earnedBadge = true;
                    if (!string.IsNullOrWhiteSpace(notification.AttachmentFilename))
                    {
                        notification.AttachmentFilename
                            = _pathResolver.ResolveContentPath(notification.AttachmentFilename);
                    }

                    notification.Text = notification.Text.Replace("and a badge",
                        $"<a href=\"{profileLink}\">and a badge</a>",
                        StringComparison.OrdinalIgnoreCase);

                    notificationDisplayList.Add(notification);
                }
                notifications.Remove(notification);
            }

            foreach (var notification in notifications
                .OrderByDescending(m => m.PointsEarned)
                .ThenByDescending(m => m.CreatedAt))
            {
                totalPointsEarned += notification.PointsEarned;
                if (notificationDisplayList.Count < MaxNotifications)
                {
                    if (!string.IsNullOrWhiteSpace(notification.AttachmentFilename))
                    {
                        notification.AttachmentFilename
                            = _pathResolver.ResolveContentPath(notification.AttachmentFilename);
                    }
                    notificationDisplayList.Add(notification);
                }
            }

            string summaryText = "";
            if (notificationDisplayList.Count > 1 && totalNotifications > MaxNotifications)
            {
                summaryText = string.Format(CultureInfo.InvariantCulture,
                    "<a href=\"{0}\">{1}</a>",
                    Url.Action(nameof(ProfileController.History), ProfileController.Name),
                    _sharedLocalizer[Annotations.Interface.AndOtherActivities]);
            }

            var viewModel = new DisplayNotificationsViewModel
            {
                Notifications = notificationDisplayList,
                SummaryText = summaryText
            };

            HttpContext.Items[ItemKey.NotificationsDisplayed] = true;
            if (earnedBadge)
            {
                HttpContext.Items[ItemKey.NotificationsModal] = true;
                return View("Modal", viewModel);
            }
            else
            {
                return View("Alert", viewModel);
            }
        }
    }
}
