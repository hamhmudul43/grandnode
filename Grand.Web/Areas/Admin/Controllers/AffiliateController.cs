﻿using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Affiliates;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Affiliates;
using Grand.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class AffiliateController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IAffiliateService _affiliateService;
        private readonly IPermissionService _permissionService;
        private readonly IAffiliateViewModelService _affiliateViewModelService;

        #endregion

        #region Constructors

        public AffiliateController(ILocalizationService localizationService,
            IAffiliateService affiliateService, IPermissionService permissionService, IAffiliateViewModelService affiliateViewModelService)
        {
            this._localizationService = localizationService;
            this._affiliateService = affiliateService;
            this._permissionService = permissionService;
            this._affiliateViewModelService = affiliateViewModelService;
        }

        #endregion

        #region Methods

        //list
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var model = new AffiliateListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command, AffiliateListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliatesModel = _affiliateViewModelService.PrepareAffiliateModelList(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = affiliatesModel.affiliateModels,
                Total = affiliatesModel.totalCount,
            };
            return Json(gridModel);
        }

        //create
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var model = new AffiliateModel();
            _affiliateViewModelService.PrepareAffiliateModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Create(AffiliateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var affiliate = _affiliateViewModelService.InsertAffiliateModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = affiliate.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _affiliateViewModelService.PrepareAffiliateModel(model, null, true);
            return View(model);

        }


        //edit
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            var model = new AffiliateModel();
            _affiliateViewModelService.PrepareAffiliateModel(model, affiliate, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(AffiliateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliate = _affiliateService.GetAffiliateById(model.Id);
            if (affiliate == null || affiliate.Deleted)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                affiliate = _affiliateViewModelService.UpdateAffiliateModel(model, affiliate);

                SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = affiliate.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            _affiliateViewModelService.PrepareAffiliateModel(model, affiliate, true);
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliate = _affiliateService.GetAffiliateById(id);
            if (affiliate == null)
                //No affiliate found with the specified id
                return RedirectToAction("List");

            _affiliateService.DeleteAffiliate(affiliate);
            SuccessNotification(_localizationService.GetResource("Admin.Affiliates.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult AffiliatedOrderList(DataSourceRequest command, AffiliatedOrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliate = _affiliateService.GetAffiliateById(model.AffliateId);
            if (affiliate == null)
                throw new ArgumentException("No affiliate found with the specified id");

            var affiliateOrders = _affiliateViewModelService.PrepareAffiliatedOrderList(affiliate, model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = affiliateOrders.affiliateOrderModels,
                Total = affiliateOrders.totalCount
            };

            return Json(gridModel);
        }


        [HttpPost]
        public IActionResult AffiliatedCustomerList(string affiliateId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageAffiliates))
                return AccessDeniedView();

            var affiliate = _affiliateService.GetAffiliateById(affiliateId);
            if (affiliate == null)
                throw new ArgumentException("No affiliate found with the specified id");

            var affiliateCustomers = _affiliateViewModelService.PrepareAffiliatedCustomerList(affiliate, command.Page, command.PageSize);

            var gridModel = new DataSourceResult
            {
                Data = affiliateCustomers.affiliateCustomerModels,
                Total = affiliateCustomers.totalCount
            };

            return Json(gridModel);
        }

        #endregion
    }
}
