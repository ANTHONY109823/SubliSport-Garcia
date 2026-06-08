namespace SubliSport.Domain.Ui;

public class UiFieldSetting
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
}

public class PanelSectionSettings
{
    public List<UiFieldSetting> Fields { get; set; } = [];
    public List<string> GarmentTypes { get; set; } = [];
    public List<string> Sports { get; set; } = [];
}

public class PanelUiSettingsData
{
    public PanelSectionSettings AdminCreateOrder { get; set; } = new();
    public PanelSectionSettings AdminOrderDetail { get; set; } = new();
    public PanelSectionSettings DesignerOrderDetail { get; set; } = new();
    public PanelSectionSettings ProductionOrderDetail { get; set; } = new();

    public static PanelUiSettingsData CreateDefault() => new()
    {
        AdminCreateOrder = new PanelSectionSettings
        {
            GarmentTypes = ["Conjunto completo", "Camiseta", "Short / Pantaloneta", "Medias", "Otro"],
            Sports = ["Fútbol", "Vóley", "Básquet", "Atletismo", "Otro"],
            Fields =
            [
                new UiFieldSetting { Key = "clientName", Label = "Nombre del cliente" },
                new UiFieldSetting { Key = "clientPhone", Label = "Teléfono / WhatsApp" },
                new UiFieldSetting { Key = "garmentType", Label = "Tipo de prenda" },
                new UiFieldSetting { Key = "sport", Label = "Deporte" },
                new UiFieldSetting { Key = "quantity", Label = "Cantidad (aprox.)" },
                new UiFieldSetting { Key = "agreedDeliveryDate", Label = "Fecha de entrega" },
                new UiFieldSetting { Key = "priority", Label = "Prioridad" },
                new UiFieldSetting { Key = "notes", Label = "Nota breve (opcional)" }
            ]
        },
        AdminOrderDetail = new PanelSectionSettings
        {
            Fields =
            [
                new UiFieldSetting { Key = "clientName", Label = "Cliente" },
                new UiFieldSetting { Key = "clientPhone", Label = "WhatsApp / Teléfono" },
                new UiFieldSetting { Key = "garmentType", Label = "Tipo de prenda" },
                new UiFieldSetting { Key = "sport", Label = "Deporte" },
                new UiFieldSetting { Key = "quantity", Label = "Cantidad" },
                new UiFieldSetting { Key = "sizeRange", Label = "Tallas" },
                new UiFieldSetting { Key = "agreedDeliveryDate", Label = "Entrega acordada" },
                new UiFieldSetting { Key = "notes", Label = "Notas admin" },
                new UiFieldSetting { Key = "assignedDesigner", Label = "Diseñador" }
            ]
        },
        DesignerOrderDetail = new PanelSectionSettings
        {
            Fields =
            [
                new UiFieldSetting { Key = "clientName", Label = "Cliente" },
                new UiFieldSetting { Key = "clientPhone", Label = "WhatsApp / Teléfono" },
                new UiFieldSetting { Key = "garmentType", Label = "Tipo de prenda" },
                new UiFieldSetting { Key = "sport", Label = "Deporte" },
                new UiFieldSetting { Key = "quantity", Label = "Cantidad" },
                new UiFieldSetting { Key = "sizeRange", Label = "Tallas" },
                new UiFieldSetting { Key = "agreedDeliveryDate", Label = "Entrega acordada" },
                new UiFieldSetting { Key = "notes", Label = "Notas admin" }
            ]
        },
        ProductionOrderDetail = new PanelSectionSettings
        {
            Fields =
            [
                new UiFieldSetting { Key = "clientName", Label = "Cliente" },
                new UiFieldSetting { Key = "clientPhone", Label = "WhatsApp cliente" },
                new UiFieldSetting { Key = "garmentType", Label = "Prenda" },
                new UiFieldSetting { Key = "sport", Label = "Deporte" },
                new UiFieldSetting { Key = "quantity", Label = "Cantidad" },
                new UiFieldSetting { Key = "sizeRange", Label = "Tallas" },
                new UiFieldSetting { Key = "agreedDeliveryDate", Label = "Entrega" },
                new UiFieldSetting { Key = "assignedDesigner", Label = "Diseñador" },
                new UiFieldSetting { Key = "notes", Label = "Notas" }
            ]
        }
    };
}
