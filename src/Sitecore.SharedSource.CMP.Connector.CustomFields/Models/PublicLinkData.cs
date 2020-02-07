namespace Sitecore.SharedSource.CMP.Connector.CustomFields.Models
{
    public class PublicLinkData
    {
        public string AssetId { get; set; }

        public string URL { get; set; }

        public string AltText { get; set; }

        public string Width { get; set; }

        public string Height { get; set; }

        public Stylelabs.M.SiteCore.Integration.Components.Models.MFileType ContentType = Stylelabs.M.SiteCore.Integration.Components.Models.MFileType.Image;
    }
}