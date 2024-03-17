using Nice.Product.Domain.Data;
using Nice.Product.Domain.Services;

namespace Nice.Product.Domain.Tests;

public static class TestData
{
    public static ProductModel Product =>
        new(
            Id: 1,
            ShortName: "Smartwatch",
            Description: "Biggest smartwatch Pro",
            Properties: new Dictionary<string, string>
            {
                { "Waterproof", "Yes" },
                { "Battery", "24h" },
                { "Size", "44mm" }
            },
            Price: 19900);

    public static Models.ProductDbModel ProductDbModel =>
        new(
            Id: 1,
            ShortName: "Smartwatch",
            Description: "Biggest smartwatch Pro",
            Properties: new Dictionary<string, string>
            {
                { "Waterproof", "Yes" },
                { "Battery", "24h" },
                { "Size", "44mm" }
            },
            Price: 19900);
}