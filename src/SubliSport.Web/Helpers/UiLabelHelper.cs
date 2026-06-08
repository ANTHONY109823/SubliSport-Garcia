using SubliSport.Domain.Ui;

namespace SubliSport.Web.Helpers;

public static class UiLabelHelper
{
    public static PanelSectionSettings GetSection(PanelUiSettingsData ui, string panelKey) =>
        panelKey switch
        {
            UiPanelKeys.AdminCreateOrder => ui.AdminCreateOrder,
            UiPanelKeys.AdminOrderDetail => ui.AdminOrderDetail,
            UiPanelKeys.DesignerOrderDetail => ui.DesignerOrderDetail,
            UiPanelKeys.ProductionOrderDetail => ui.ProductionOrderDetail,
            _ => new PanelSectionSettings()
        };

    public static string Label(PanelUiSettingsData ui, string panelKey, string fieldKey, string fallback)
    {
        var field = GetSection(ui, panelKey).Fields.FirstOrDefault(f => f.Key == fieldKey);
        return string.IsNullOrWhiteSpace(field?.Label) ? fallback : field.Label;
    }

    public static bool IsVisible(PanelUiSettingsData ui, string panelKey, string fieldKey)
    {
        var field = GetSection(ui, panelKey).Fields.FirstOrDefault(f => f.Key == fieldKey);
        return field?.Visible ?? true;
    }

    public static IReadOnlyList<string> GarmentTypes(PanelUiSettingsData ui) =>
        ui.AdminCreateOrder.GarmentTypes.Count > 0
            ? ui.AdminCreateOrder.GarmentTypes
            : PanelUiSettingsData.CreateDefault().AdminCreateOrder.GarmentTypes;

    public static IReadOnlyList<string> Sports(PanelUiSettingsData ui) =>
        ui.AdminCreateOrder.Sports.Count > 0
            ? ui.AdminCreateOrder.Sports
            : PanelUiSettingsData.CreateDefault().AdminCreateOrder.Sports;
}
