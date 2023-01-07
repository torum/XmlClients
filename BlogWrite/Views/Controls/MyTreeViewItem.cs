using BlogWrite.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml;
using BlogWrite.Models;
using DataPackageOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation;

namespace BlogWrite.Views
{

    class MyTreeViewItem : TreeViewItem
    {
        protected override void OnDragEnter(DragEventArgs e)
        {
            var draggedItem = FeedsPage.DraggedItems[0];
            var draggedOverItem = DataContext as NodeTree;

            if ((draggedItem is NodeFolder) && (draggedOverItem is NodeFolder) || (draggedItem is NodeFeed) && (draggedOverItem is NodeFolder))
            {
                e.Handled = true;
            }

            base.OnDragEnter(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            var draggedItem = FeedsPage.DraggedItems[0];
            var draggedOverItem = DataContext as NodeTree;
            /*
            if (draggedOverItem != null)
                if (draggedOverItem.Parent != null)
                    Debug.WriteLine(draggedOverItem.Parent.ToString());
            */
            /*
            if (draggedOverItem?.Parent is ServiceTreeBuilder)
            {
                // Accept

                base.OnDragOver(e);
                e.AcceptedOperation = DataPackageOperation.Move;

                return;
            }
            */
            //

            // only allow a feed move into a folder
            if ((draggedItem is NodeFeed) && (draggedOverItem is NodeFolder))
            {
                // same place is meaningless
                if (draggedItem.Parent == draggedOverItem)
                {
                    // Deny

                    e.Handled = true;
                    base.OnDragOver(e);
                    e.AcceptedOperation = DataPackageOperation.None;
                }
                else
                {
                    // Accept

                    base.OnDragOver(e);
                    e.AcceptedOperation = DataPackageOperation.Move;
                }
            }
            else
            {
                // Deny

                e.Handled = true;
                base.OnDragOver(e);
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }
        protected override void OnDrop(DragEventArgs e)
        {
            var draggedItem = FeedsPage.DraggedItems[0];
            var draggedOverItem = DataContext as NodeTree;

            if ((draggedItem is NodeFeed) && (draggedOverItem is NodeFolder))
            {
                // ok
            }
            else
            {
                // no
                e.Handled = true;
            }

            base.OnDrop(e);
        }
    }
}