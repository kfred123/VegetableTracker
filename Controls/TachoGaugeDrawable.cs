using Microsoft.Maui.Graphics;

namespace VegetableTracker.Controls;

/// <summary>
/// Draws a tachometer-style gauge (upper semicircular arc with needle).
/// </summary>
public class TachoGaugeDrawable : IDrawable
{
    /// <summary>Progress value between 0.0 and 1.0.</summary>
    public double Progress { get; set; }

    /// <summary>Current count displayed in the center.</summary>
    public int CurrentValue { get; set; }

    /// <summary>Maximum/goal value displayed in the center.</summary>
    public int MaxValue { get; set; } = 30;

    // Arc geometry — upper semicircle from 180° (left) to 0° (right)
    private const float StartAngleDeg = 180f;
    private const float EndAngleDeg = 360f;
    private const float SweepAngleDeg = 180f;

    // Colors for light-background rendering
    private static readonly Color TextColor = Color.FromArgb("#FF2D2D2D");
    private static readonly Color MinorTickColor = Color.FromArgb("#99888888");
    private static readonly Color HubOuterColor = Color.FromArgb("#FF4A4A4A");
    private static readonly Color HubInnerColor = Color.FromArgb("#FF707070");
    private static readonly Color NeedleColor = Color.FromArgb("#FFD32F2F");
    private static readonly Color NeedleShadowColor = Color.FromArgb("#30000000");
    private static readonly Color TrackColor = Color.FromArgb("#20808080");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var width = dirtyRect.Width;
        var height = dirtyRect.Height;

        // Center of the gauge arc — pushed down so the upper arc fills the view
        var cx = width / 2f;
        var cy = height * 0.78f;

        var radius = Math.Min(width, height * 1.5f) * 0.34f;
        var arcRect = new RectF(cx - radius, cy - radius, radius * 2, radius * 2);

        var trackWidth = radius * 0.16f;

        // Clip to only the upper half (above the gauge center line)
        // so no arc is drawn below
        canvas.SaveState();
        canvas.ClipRectangle(0, 0, width, cy + trackWidth / 2f);

        // ── 1. Background track arc (subtle grey, upper semicircle) ──
        canvas.StrokeColor = TrackColor;
        canvas.StrokeSize = trackWidth;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawArc(arcRect.X, arcRect.Y, arcRect.Width, arcRect.Height,
            StartAngleDeg, EndAngleDeg, true, false);

        // ── 2. Colored progress arc (gradient red→green) ──
        if (Progress > 0)
        {
            var progressSweep = (float)(SweepAngleDeg * Math.Min(Progress, 1.0));
            DrawGradientArc(canvas, arcRect, trackWidth, progressSweep);
        }

        // ── 3. Tick marks & labels ──
        DrawTicks(canvas, cx, cy, radius, trackWidth);

        // ── 4. Needle ──
        DrawNeedle(canvas, cx, cy, radius, trackWidth);

        canvas.RestoreState();

        // ── 5. Center hub (drawn outside clip so it's fully visible) ──
        var hubRadius = radius * 0.09f;
        canvas.FillColor = HubOuterColor;
        canvas.FillCircle(cx, cy, hubRadius);
        canvas.FillColor = HubInnerColor;
        canvas.FillCircle(cx, cy, hubRadius * 0.55f);

        // ── 6. Center text ──
        DrawCenterText(canvas, cx, cy, radius);
    }

    /// <summary>
    /// Draws the progress arc with a color gradient (many tiny segments).
    /// </summary>
    private void DrawGradientArc(ICanvas canvas, RectF arcRect, float strokeWidth, float progressSweep)
    {
        const int segments = 60;
        var segSweep = progressSweep / segments;

        canvas.StrokeSize = strokeWidth;
        canvas.StrokeLineCap = LineCap.Butt;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            canvas.StrokeColor = GaugeColorAt(t);

            float segStart = StartAngleDeg + i * segSweep;
            float segEnd = segStart + segSweep + 0.5f; // tiny overlap
            canvas.DrawArc(arcRect.X, arcRect.Y, arcRect.Width, arcRect.Height,
                segStart, segEnd, true, false);
        }
    }

    /// <summary>
    /// Interpolates red → orange → yellow → green.
    /// </summary>
    private static Color GaugeColorAt(float t)
    {
        float r, g, b;
        if (t < 0.33f)
        {
            float p = t / 0.33f;
            r = 1f;
            g = 0.3f * p + 0.15f * (1 - p);
            b = 0.05f;
        }
        else if (t < 0.66f)
        {
            float p = (t - 0.33f) / 0.33f;
            r = 1f;
            g = 0.55f + 0.35f * p;
            b = 0.05f;
        }
        else
        {
            float p = (t - 0.66f) / 0.34f;
            r = 1f - 0.7f * p;
            g = 0.85f + 0.15f * p;
            b = 0.1f + 0.2f * p;
        }
        return new Color(r, g, b);
    }

    /// <summary>
    /// Draws major (0,5,10,…,30) and minor tick marks plus numeric labels.
    /// </summary>
    private void DrawTicks(ICanvas canvas, float cx, float cy, float radius, float trackWidth)
    {
        var majorTicks = new[] { 0, 5, 10, 15, 20, 25, 30 };
        var totalTicks = 30;

        float outerR = radius + trackWidth / 2f + 4f;
        float majorLen = radius * 0.12f;
        float minorLen = radius * 0.06f;
        float labelR = outerR + majorLen + radius * 0.10f;

        canvas.FontSize = radius * 0.13f;

        for (int i = 0; i <= totalTicks; i++)
        {
            float t = (float)i / totalTicks;
            float angleDeg = StartAngleDeg + SweepAngleDeg * t;
            float angleRad = angleDeg * MathF.PI / 180f;

            bool isMajor = Array.IndexOf(majorTicks, i) >= 0;
            float len = isMajor ? majorLen : minorLen;

            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            float x1 = cx + outerR * cos;
            float y1 = cy + outerR * sin;
            float x2 = cx + (outerR + len) * cos;
            float y2 = cy + (outerR + len) * sin;

            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeSize = isMajor ? 2.5f : 1.2f;
            canvas.StrokeColor = isMajor ? TextColor : MinorTickColor;
            canvas.DrawLine(x1, y1, x2, y2);

            if (isMajor)
            {
                float lx = cx + labelR * cos;
                float ly = cy + labelR * sin;

                var label = i.ToString();
                canvas.FontColor = TextColor;
                canvas.DrawString(label, lx - 12, ly - 8, 24, 16,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }

    /// <summary>
    /// Draws the needle pointing at the current progress.
    /// </summary>
    private void DrawNeedle(ICanvas canvas, float cx, float cy, float radius, float trackWidth)
    {
        float angleDeg = StartAngleDeg + SweepAngleDeg * (float)Math.Min(Progress, 1.0);
        float angleRad = angleDeg * MathF.PI / 180f;

        float needleLen = radius + trackWidth / 2f - 4f;
        float needleBase = radius * 0.06f;

        float tipX = cx + needleLen * MathF.Cos(angleRad);
        float tipY = cy + needleLen * MathF.Sin(angleRad);

        float perpX = -MathF.Sin(angleRad) * needleBase;
        float perpY = MathF.Cos(angleRad) * needleBase;

        var path = new PathF();
        path.MoveTo(tipX, tipY);
        path.LineTo(cx + perpX, cy + perpY);
        path.LineTo(cx - perpX, cy - perpY);
        path.Close();

        // Shadow
        canvas.FillColor = NeedleShadowColor;
        canvas.SaveState();
        canvas.Translate(2, 2);
        canvas.FillPath(path);
        canvas.RestoreState();

        // Needle body
        canvas.FillColor = NeedleColor;
        canvas.FillPath(path);
    }

    /// <summary>
    /// Draws percentage text centered below the hub.
    /// </summary>
    private void DrawCenterText(ICanvas canvas, float cx, float cy, float radius)
    {
        canvas.FontSize = radius * 0.28f;
        canvas.FontColor = TextColor;
        var percent = MaxValue > 0 ? (int)Math.Round(Progress * 100) : 0;
        var valueText = $"{percent}%";
        canvas.DrawString(valueText, cx - radius * 0.5f, cy + radius * 0.08f,
            radius, radius * 0.3f, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
