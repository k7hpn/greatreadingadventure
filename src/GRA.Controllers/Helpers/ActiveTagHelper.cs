﻿using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Baseline.Controllers.TagHelpers
{
    /// <summary>
    /// Add the CSS class "active" to anchor elements if the asp-action, asp-area, and
    /// asp-controller properties match those of the current page.
    /// </summary>
    [HtmlTargetElement("a", Attributes = "add-active")]
    public class ActiveTagHelper : TagHelper
    {
        private const string ActiveClass = "active";
        private const string AddActiveTag = "add-active";
        private const string RouteAction = "action";
        private const string RouteArea = "area";
        private const string RouteController = "controller";
        private const string TagAspAction = "asp-action";
        private const string TagAspArea = "asp-area";
        private const string TagAspController = "asp-controller";
        private const string TagClass = "class";
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ActiveTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory
                ?? throw new ArgumentNullException(nameof(urlHelperFactory));
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; } = null!;

        private string AspAction { get; set; } = string.Empty;
        private string AspArea { get; set; } = string.Empty;
        private string AspController { get; set; } = string.Empty;

        public override void Init(TagHelperContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // store this information before the Microsoft tag helper turns it into a link
            AspArea = context.AllAttributes.TryGetAttribute(TagAspArea, out var areaItem)
                ? areaItem?.Value as string ?? string.Empty
                : string.Empty;

            AspAction = context.AllAttributes.TryGetAttribute(TagAspAction, out var actionItem)
                ? actionItem?.Value as string ?? string.Empty
                : string.Empty;

            AspController = context.AllAttributes.TryGetAttribute(TagAspController,
                    out var controllerItem)
                ? controllerItem?.Value as string ?? string.Empty
                : string.Empty;

            base.Init(context);
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(output);

            output.Attributes.Remove(new TagHelperAttribute(AddActiveTag));

            var url = _urlHelperFactory.GetUrlHelper(ViewContextData);
            var routeData = url.ActionContext.RouteData.Values;

            // check if the route has an action and if it matches - or if it's blank in both
            var actionMatch = routeData.TryGetValue(RouteAction, out var action)
                ? action as string == AspAction
                : string.IsNullOrEmpty(AspAction);

            // route has an area and it matches or the a tag does not specify
            var areaMatch = routeData.TryGetValue(RouteArea, out var area)
                ? area as string == AspArea || string.IsNullOrEmpty(AspArea)
                : string.IsNullOrEmpty(AspArea);

            // route has a controller and it matches or the a tag does not specify
            var controllerMatch = routeData.TryGetValue(RouteController, out var controller)
                ? controller as string == AspController || string.IsNullOrEmpty(AspController)
                : string.IsNullOrEmpty(AspController);

            // if there's a match, perform the addition of the class
            if (actionMatch && areaMatch && controllerMatch)
            {
                var existingClass = output.Attributes.FirstOrDefault(_ => _.Name == TagClass);
                var cssClass = existingClass != null
                    ? $"{existingClass.Value} {ActiveClass}"
                    : ActiveClass;

                if (existingClass != null)
                {
                    output.Attributes.Remove(existingClass);
                }

                output.Attributes.Add(new TagHelperAttribute(TagClass, cssClass.Trim()));
            }
        }
    }
}
