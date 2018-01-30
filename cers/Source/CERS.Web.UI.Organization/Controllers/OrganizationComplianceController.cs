using CERS.Model;
using CERS.ViewModels.Enforcements;
using CERS.ViewModels.Inspections;
using CERS.ViewModels.Organizations;
using CERS.ViewModels.Violations;
using CERS.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CERS.Web.UI.Organization.Controllers
{
	[Authorize]
	public class OrganizationComplianceController : AppControllerBase
	{
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Enforcements( int organizationId, bool export = false )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			if ( export )
			{
				List<EnforcementGridViewModel> results = new List<EnforcementGridViewModel>();
				var facilityList = viewModel.Entity.Facilities.Where( p => !p.Voided );
				foreach ( Facility facility in facilityList )
				{
					results.AddRange( facility.Enforcements.Where( p => !p.Voided ).OrderByDescending( p => p.OccurredOn ).ToGridView() );
				}
				ExportToExcel( "OrganizationEnforcement_Export.xlsx", results );
			}
			return View( viewModel );
		}

		//
  // GET: /Compliance/
		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Index( int organizationId )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Inspections( int organizationId, bool export = false )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			if ( export )
			{
				List<InspectionGridViewModel> results = new List<InspectionGridViewModel>();
				var facilityList = viewModel.Entity.Facilities.Where( p => !p.Voided );
				foreach ( Facility facility in facilityList )
				{
					results.AddRange( facility.Inspections.Where( p => !p.Voided ).OrderByDescending( p => p.OccurredOn ).ToGridView() );
				}
				ExportToExcel( "OrganizationInspection_Export.xlsx", results );
			}
			return View( viewModel );
		}

		[EntityFilterAuthorization( Context.Organization, "organizationId", PermissionRole.OrgViewer )]
		public ActionResult Violations( int organizationId, bool export = false )
		{
			CurrentOrganizationID = organizationId;
			OrganizationViewModel viewModel = new OrganizationViewModel
			{
				Entity = Repository.Organizations.GetByID( organizationId )
			};
			if ( export )
			{
				List<ViolationGridViewModel> results = new List<ViolationGridViewModel>();
				var facilityList = viewModel.Entity.Facilities.Where( p => !p.Voided );
				foreach ( Facility facility in facilityList )
				{
					foreach ( Inspection inspection in facility.Inspections.Where( i => !i.Voided ) )
					{
						results.AddRange( inspection.Violations.Where( v => !v.Voided ).OrderByDescending( v => v.OccurredOn ).ToGridView() );
					}
				}
				ExportToExcel( "OrganizationViolation_Export.xlsx", results );
			}
			return View( viewModel );
		}
	}
}