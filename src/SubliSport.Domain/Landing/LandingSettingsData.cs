namespace SubliSport.Domain.Landing;

public class LandingCatalogItem
{
    public int SortOrder { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
}

public class LandingGarmentOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string IconClass { get; set; } = "fas fa-tshirt";
}

public class LandingSportOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class LandingSizeOption
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class LandingQuoteSettings
{
    public string WhatsAppPhone { get; set; } = "51960840874";
    public string ResponseNote { get; set; } = "Respuesta en menos de 2 horas · La Victoria, Lima";
    public string QuantityPlaceholder { get; set; } = "Cantidad de prendas";
    public string NamePlaceholder { get; set; } = "Tu nombre o club";
    public string ExtraPlaceholder { get; set; } = "Colores, diseño, fecha de entrega, escudo...";
    public List<LandingGarmentOption> Garments { get; set; } = [];
    public List<LandingSportOption> Sports { get; set; } = [];
    public List<LandingSizeOption> Sizes { get; set; } = [];
}

public class LandingSettingsData
{
    public List<LandingCatalogItem> Catalog { get; set; } = [];
    public LandingQuoteSettings Quote { get; set; } = new();

    public static LandingSettingsData CreateDefault() => new()
    {
        Catalog =
        [
            new LandingCatalogItem
            {
                SortOrder = 1,
                ImageUrl = "https://i.pinimg.com/736x/bf/c5/89/bfc589c46649a1c6978872470d1a8850.jpg",
                Title = "Real Madrid",
                Subtitle = "La Liga · España"
            },
            new LandingCatalogItem
            {
                SortOrder = 2,
                ImageUrl = "https://i1-c.pinimg.com/1200x/8f/32/5a/8f325a52478201b47be4a79bbf12db1d.jpg",
                Title = "FC Barcelona",
                Subtitle = "La Liga · España"
            },
            new LandingCatalogItem
            {
                SortOrder = 3,
                ImageUrl = "https://i.pinimg.com/736x/82/20/a4/8220a40857ff8088154d5a3c87ff5c51.jpg",
                Title = "Estilo Europeo",
                Subtitle = "Sublimado Premium"
            },
            new LandingCatalogItem
            {
                SortOrder = 4,
                ImageUrl = "https://i1-c.pinimg.com/736x/ff/eb/1d/ffeb1db96f8bb8dc3728543407c493a5.jpg",
                Title = "Diseño Especial",
                Subtitle = "Full Color"
            },
            new LandingCatalogItem
            {
                SortOrder = 5,
                ImageUrl = "https://i.pinimg.com/originals/cb/56/71/cb5671893cf9c11517b2f14441822d0d.png",
                Title = "Alianza Lima",
                Subtitle = "Liga 1 · Perú"
            },
            new LandingCatalogItem
            {
                SortOrder = 6,
                ImageUrl = "https://i.pinimg.com/736x/ec/97/d9/ec97d997bd4a823f6076ad7cd5f6e2c2.jpg",
                Title = "Universitario",
                Subtitle = "Liga 1 · Perú"
            },
            new LandingCatalogItem
            {
                SortOrder = 7,
                ImageUrl = "https://i1-c.pinimg.com/1200x/e9/59/34/e9593407ed67eef379b5153c46f02cbc.jpg",
                Title = "Sporting Cristal",
                Subtitle = "Liga 1 · Perú"
            },
            new LandingCatalogItem
            {
                SortOrder = 8,
                ImageUrl = "https://i1-c.pinimg.com/1200x/fa/48/1a/fa481a192b539477798fc8614ca4a23b.jpg",
                Title = "Selección Perú",
                Subtitle = "Blanquirroja"
            }
        ],
        Quote = new LandingQuoteSettings
        {
            WhatsAppPhone = "51960840874",
            ResponseNote = "Respuesta en menos de 2 horas · La Victoria, Lima",
            Garments =
            [
                new LandingGarmentOption
                {
                    Label = "Conjunto Completo",
                    Value = "Conjunto completo (camiseta + short + medias)",
                    IconClass = "fas fa-tshirt"
                },
                new LandingGarmentOption
                {
                    Label = "Solo Camiseta",
                    Value = "Solo camiseta",
                    IconClass = "fas fa-circle-dot"
                },
                new LandingGarmentOption
                {
                    Label = "Solo Short",
                    Value = "Short deportivo",
                    IconClass = "fas fa-person-running"
                }
            ],
            Sports =
            [
                new LandingSportOption { Label = "⚽ Fútbol", Value = "Fútbol" },
                new LandingSportOption { Label = "🏐 Vóley", Value = "Vóley" },
                new LandingSportOption { Label = "🏀 Básquet", Value = "Básquet" },
                new LandingSportOption { Label = "🚴 Ciclismo", Value = "Ciclismo" },
                new LandingSportOption { Label = "🏅 Otro", Value = "Otro" }
            ],
            Sizes =
            [
                new LandingSizeOption { Label = "XS / S", Value = "XS / S" },
                new LandingSizeOption { Label = "M / L", Value = "M / L" },
                new LandingSizeOption { Label = "XL / XXL", Value = "XL / XXL" },
                new LandingSizeOption { Label = "Tallas mixtas", Value = "Tallas mixtas" }
            ]
        }
    };
}
