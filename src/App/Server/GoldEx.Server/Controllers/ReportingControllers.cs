using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.ReportDesigner;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

// This controller is required for the Document Viewer and Report Designer.
public class CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService)
    : WebDocumentViewerController(controllerService);

// This controller is required for the Report Designer.
public class CustomReportDesignerController(IReportDesignerMvcControllerService controllerService)
    : ReportDesignerController(controllerService)
{
    [HttpPost("[action]")]
    public object GetReportDesignerModel(
        [FromForm] string reportUrl,
        [FromForm] ReportDesignerSettingsBase designerModelSettings,
        [FromServices] IReportDesignerClientSideModelGenerator designerClientSideModelGenerator)
    {
        var dataSources = new Dictionary<string, object>();
        var ds = new SqlDataSource("GoldEx");
        //dataSources.Add("sqlDataSource1", ds);
        ReportDesignerModel model;
        model = string.IsNullOrEmpty(reportUrl)
            ? designerClientSideModelGenerator.GetModel(new XtraReport(),
                dataSources,
                "/DXXRD",
                "/DXXRDV",
                "/DXXQB")
            : designerClientSideModelGenerator.GetModel(reportUrl,
                dataSources,
                "/DXXRD",
                "/DXXRDV",
                "/DXXQB");
        //model.WizardSettings.EnableSqlDataSource = true;
        model.Assign(designerModelSettings);
        var modelJsonScript = designerClientSideModelGenerator.GetJsonModelScript(model);
        return Content(modelJsonScript, "application/json");
    }
}

// This controller is required for the Report Designer.
public class CustomQueryBuilderController(IQueryBuilderMvcControllerService controllerService)
    : QueryBuilderController(controllerService);