using Sitecore.Abstractions;
using Sitecore.Connector.CMP.Pipelines.ImportEntity;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.SharedSource.CMP.Connector.CustomFields.Helpers;
using Sitecore.SharedSource.CMP.Connector.CustomFields.Models;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Authentication;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sitecore.SharedSource.CMP.Connector.CustomFields.Pipelines
{

    public class SaveImageFieldValues : ImportEntityProcessor
    {
        private ImportEntityPipelineArgs Args
        {
            get;
            set;
        }

        private IWebMClient _webMClient;        
        private IWebMClient WebMClient
        {
            get
            {
                if(_webMClient == null)
                {
                    OAuthPasswordGrant oAuth = new OAuthPasswordGrant
                    {
                        ClientId = Args.ConfigItem[Sitecore.Connector.CMP.Constants.MClientIdFieldId],
                        ClientSecret = Args.ConfigItem[Sitecore.Connector.CMP.Constants.MClientSecretFieldId],
                        UserName = Args.ConfigItem[Sitecore.Connector.CMP.Constants.MUserNameFieldId],
                        Password = Args.ConfigItem[Sitecore.Connector.CMP.Constants.MPasswordFieldId]
                    };

                    _webMClient = MClientFactory.CreateMClient(new Uri(Args.ConfigItem[Sitecore.Connector.CMP.Constants.MUriFieldId]), oAuth);
                }
                return _webMClient;
            }
        }

        public override void Process(ImportEntityPipelineArgs args, BaseLog logger)
        {
            Assert.IsNotNull(args.Item, "The item is null.");
            Assert.IsNotNull(args.Language, "The language is null.");
            Args = args;
            var contentHubHost = args.ConfigItem[Sitecore.Connector.CMP.Constants.MUriFieldId];
            using (new SecurityDisabler())
            {
                using (new LanguageSwitcher(args.Language))
                {
                    try
                    {
                        int assetIndexField = 1;
                        args.Item.Editing.BeginEdit();                        
                        foreach (Item item in from i in args.EntityMappingItem.Children
                                              where i.TemplateID == Constants.ImageFieldMappingTypeID
                                              select i)
                        {
                            var cmpFieldName = item[Sitecore.Connector.CMP.Constants.FieldMappingCmpFieldNameFieldId];
                            var sitecoreFieldName = item[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId];
                            
                            if(!int.TryParse(item[Constants.FieldMappingSitecoreAssetIndexFieldID], out assetIndexField))
                            {
                                assetIndexField = 1;
                            }
                            
                            var renditionField = item[Constants.FieldMappingSitecoreRenditionFieldID];
                            var publicLink = GetPublicLinkData(contentHubHost, args.Entity, cmpFieldName, renditionField, assetIndexField).GetAwaiter().GetResult();

                            if(publicLink != null && !string.IsNullOrEmpty(publicLink.URL))
                            {
                                var imgElement = GetContentHubDamImageElement(contentHubHost, publicLink);
                                args.Item[item[Sitecore.Connector.CMP.Constants.FieldMappingSitecoreFieldNameFieldId]] = imgElement;
                            }
                        }
                    }
                    finally
                    {
                        args.Item.Editing.EndEdit();
                    }
                }
            }
        }

        private async Task<PublicLinkData> GetPublicLinkData(string host, IEntity entity, string relationName, string rendition, int assetIndex)
        {
            var asset = await GetAssetFromRelation(entity, relationName, assetIndex);            
            Assert.IsNotNull(asset, $"Could not load asset.");
            Assert.IsTrue(asset.Id.HasValue, $"Asset id is null.");

            var assetPublicLink = await PublicLinkHelper.GetorCreatePublicLink(WebMClient, rendition, asset.Id.Value);    
            Assert.IsNotNull(assetPublicLink, $"Could not get or create public link to asset id: {asset.Id.Value}.");
           
            var publicLink = PublicLinkHelper.GetPublicLinkData(host, asset, assetPublicLink, rendition);
            Assert.IsNotNull(publicLink, $"public link data is null.");

            return publicLink;      
        }       

        private async Task<IEntity> GetAssetFromRelation(IEntity entity, string relationName, int index)
        {
            var relationWithAssets = entity.GetRelation<IParentToManyChildrenRelation>(relationName);
            Assert.IsNotNull(relationWithAssets, $"Failed to get relation: {relationName}.");

            var assetIds = relationWithAssets.GetIds();
            Assert.IsNotNull(assetIds[index-1], $"Asset with index {index - 1} not found.");
            
            var propertiesLoad = new PropertyLoadOption(new string[] { Constants.Asset.Properties.FileName, Constants.Asset.Properties.Title, Constants.Asset.Properties.Renditions });
            return await WebMClient.Entities.GetAsync(assetIds[index - 1], new EntityLoadConfiguration(CultureLoadOption.Default, propertiesLoad, RelationLoadOption.None)).ConfigureAwait(false);
        }

        private string GetContentHubDamImageElement(string host, PublicLinkData publicLink)
        {            
            return $"<image stylelabs-content-id=\"{publicLink.AssetId}\" thumbnailsrc=\"{host}/api/gateway/{publicLink.AssetId}/thumbnail\" src=\"{publicLink.URL}\" mediaid =\"\" stylelabs-content-type=\"{publicLink.ContentType.ToString()}\" alt=\"{publicLink.AltText}\" height=\"{publicLink.Height}\" width=\"{publicLink.Width}\" />";
        }

    }
}