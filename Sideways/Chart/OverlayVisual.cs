namespace Sideways
{
    using System.Windows.Media;

    public class OverlayVisual : DrawingVisual
    {
        protected override GeometryHitTestResult? HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return null;
        }

        protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return null;
        }
    }
}
