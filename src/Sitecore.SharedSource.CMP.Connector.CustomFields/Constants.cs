using Sitecore.Data;

namespace Sitecore.SharedSource.CMP.Connector.CustomFields
{
    public static class Constants
    {        
        public static ID ImageFieldMappingTypeID = new ID("{40313FEB-8D8B-403D-BEB7-9ACB64302283}");
        public static ID FieldMappingSitecoreAssetIndexFieldID = new ID("{EBE8B49A-D744-4A66-9220-928B5B89A419}");
        public static ID FieldMappingSitecoreRenditionFieldID = new ID("{0DC3344C-F12E-47C4-A7CC-E9609DAFB25E}");
        
        public static class PublicLink
        {
            public static class Properties
            {
                public const string RelativeUrl = "RelativeUrl";
                public const string VersionHash = "VersionHash";
                public const string ConversionConfiguration = "ConversionConfiguration";
            }            
        }

        public static class Asset
        {
            public static class Properties
            {
                public const string Renditions = "Renditions";                
                public const string FileName = "FileName";                
                public const string Title = "Title";                
            }
        }
    }
}