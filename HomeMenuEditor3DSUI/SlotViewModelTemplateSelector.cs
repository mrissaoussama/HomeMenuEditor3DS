using System.Windows;
using System.Windows.Controls;

namespace HomeMenuEditor3DSUI
{
    public class SlotViewModelTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TitleTemplate { get; set; }
        public DataTemplate FolderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var slotViewModel = item as SlotViewModel;
            if (slotViewModel != null)
            {
                if (slotViewModel.Folder != null)
                    return FolderTemplate;
                else
                    return TitleTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
