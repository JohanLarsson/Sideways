namespace Sideways
{
    using System.Windows;
    using System.Windows.Documents;

    public class EarningsBar : CandleSeries
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            return new(25, TextElement.GetFontSize(this) * TextElement.GetFontFamily(this).LineSpacing);
        }
    }
}
