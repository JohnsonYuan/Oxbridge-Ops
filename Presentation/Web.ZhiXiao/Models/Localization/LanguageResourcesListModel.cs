using Nop.Web.Framework;

namespace Nop.Admin.Models.Localization
{
    public class LanguageResourcesListModel
    {
        [NopResourceDisplayName("Admin.Configuration.Languages.Resources.SearchResourceName")]
        public string SearchResourceName { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Languages.Resources.SearchResourceValue")]
        public string SearchResourceValue { get; set; }
    }
}