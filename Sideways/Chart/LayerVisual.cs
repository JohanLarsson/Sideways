namespace Sideways
{
    using System.Windows.Media;

    public class LayerVisual : DrawingVisual
    {
        protected override GeometryHitTestResult? HitTestCore(GeometryHitTestParameters hitTestParameters) => null;

        protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters) => null;
    }
}
