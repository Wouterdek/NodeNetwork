using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NodeNetwork.Views.Controls
{
    /// <summary>
    /// Simple panel that stretches its children to fill the panel.
    /// </summary>
    public class FillPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size maxSize = new Size(0, 0);

            foreach (UIElement e in InternalChildren)
            {
                e.Measure(availableSize);
                maxSize = new Size(
                    Math.Max(maxSize.Width, e.DesiredSize.Width), 
                    Math.Max(maxSize.Height, e.DesiredSize.Height)
                );
            }

            return maxSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect size = new Rect(new Point(), finalSize);
            foreach (UIElement e in InternalChildren)
            {
                e.Arrange(size);
            }

            return finalSize;
        }
    }
}
