using BLL.Helpers;
using BLL.Models;
using FluentAssertions;

namespace TechApp2FTest
{
    public class BllTests
    {
        [Fact]
        public void PartDescriptionBuilderTest()
        {
            PartDescriptionBuilder partDescriptionBuilder = new PartDescriptionBuilder();
            PartModel part = new PartModel
            {
                IsRound = true,
                IsSheetMetal = true,
                Length = 120,
                MaterialThickness = 6,
                Parttype = "Wasserstrahl Zuschnitt",
                Material = "Messing",
                Holesummary = "2xÿ10.5, 4xM5"
            };

            partDescriptionBuilder.BuildDescription(part).Should().Be("Zuschnitt Messing ÿ120 s=6 mit 2xÿ10.5, 4xM5");

        }
    }
}