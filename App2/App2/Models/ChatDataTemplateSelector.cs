using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace App2.Models
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FromTemplate { get; set; }
        public DataTemplate ToTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((Chat)item).Status.ToUpper().Equals("SENT") ? ToTemplate : FromTemplate;
        }
    }
}
