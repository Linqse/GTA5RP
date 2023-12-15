namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    private Rectangle CalculateTargetRectangle( float relativeX, float relativeY, int targetWidth, int targetHeight)
    {
        if (Win.ActiveWindow != null)
        {
            var dwm = Win.ActiveWindow.DwmWindowBounds;

            // Вычисление центра элемента относительно размера окна
            int centerX = (int)(dwm.Width * relativeX);
            int centerY = (int)(dwm.Height * relativeY);

            // Вычисление координат верхнего левого угла нового прямоугольника
            int newX = centerX - targetWidth / 2;
            int newY = centerY - targetHeight / 2;

            return new Rectangle(newX, newY, targetWidth, targetHeight);
        }

        return default;
    }
}