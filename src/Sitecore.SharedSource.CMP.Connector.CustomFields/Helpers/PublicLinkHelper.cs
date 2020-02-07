using Stylelabs.M.Base.Querying;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Base.Querying.Filters;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using MConstants = Stylelabs.M.Sdk.Constants;
using Stylelabs.M.Sdk.WebClient;
using Newtonsoft.Json.Linq;
using Sitecore.SharedSource.CMP.Connector.CustomFields.Models;

namespace Sitecore.SharedSource.CMP.Connector.CustomFields.Helpers
{
    public static class PublicLinkHelper
    {
        public static async Task<IEntity> GetorCreatePublicLink(IWebMClient webMClient, string rendition, long assetId)
        {
            var publicLink = await GetPublicLink(webMClient, rendition, assetId);
            if (publicLink == null)
            {
                webMClient.Logger.Debug("publicLink not found. Will create one");
                publicLink = await CreatePublicLink(webMClient, rendition, assetId);
            }

            return publicLink;
        }

        public static async Task<IEntity> GetPublicLink(IWebMClient webMClient, string rendition, long assetId)
        {
            var filters = new List<QueryFilter>
            {
                new DefinitionQueryFilter()
                {
                    Name = MConstants.PublicLink.DefinitionName
                },
                new PropertyQueryFilter()
                {
                    Property = MConstants.PublicLink.Resource,
                    Value = rendition
                },
                new RelationQueryFilter()
                {
                    ParentId = assetId,
                    Relation = MConstants.PublicLink.AssetToPublicLink
                }
            };

            var query = new Query()
            {
                Take = 1,
                Filter = new CompositeQueryFilter()
                {
                    Children = filters,
                    CombineMethod = CompositeFilterOperator.And
                }
            };

            return await webMClient.Querying.SingleAsync(query, PublicLinkLoadConfiguration()).ConfigureAwait(false);
        }

        public static async Task<IEntity> CreatePublicLink(IWebMClient webMClient, string rendition, long assetId)
        {
            var publicLink = await webMClient.EntityFactory.CreateAsync(MConstants.PublicLink.DefinitionName).ConfigureAwait(false);
            publicLink.SetPropertyValue(MConstants.PublicLink.Resource, rendition);

            var relation = publicLink.GetRelation<IChildToManyParentsRelation>(MConstants.PublicLink.AssetToPublicLink);
            if (relation == null)
            {
                webMClient.Logger.Error("Unable to create public link: no AssetToPublicLink relation found.");
                return null;
            }

            relation.Parents.Add(assetId);
            var id = await webMClient.Entities.SaveAsync(publicLink).ConfigureAwait(false);
            return await webMClient.Entities.GetAsync(id, PublicLinkLoadConfiguration()).ConfigureAwait(false);
        }

        public static PublicLinkData GetPublicLinkData(string host, IEntity asset, IEntity publicLink, string rendition)
        {
            var publicLinkData = new PublicLinkData() { AssetId = asset.Id.Value.ToString() };


            var relativeUrl = publicLink.GetPropertyValue(Constants.PublicLink.Properties.RelativeUrl);
            var version = publicLink.GetPropertyValue(Constants.PublicLink.Properties.VersionHash);
            publicLinkData.URL = $"{host}/api/public/content/{relativeUrl}?v={version}";

            publicLinkData.AltText = asset.GetPropertyValue<string>(Constants.Asset.Properties.Title) ?? asset.GetPropertyValue<string>(Constants.Asset.Properties.FileName);

            var publicLinkConversions = publicLink.GetPropertyValue<JToken>(Constants.PublicLink.Properties.ConversionConfiguration);
            JToken imageProperties = null;
            if (publicLinkConversions != null)
            {
                imageProperties = publicLinkConversions;
            }
            else //no custom dimensions set. get dimension from rendition
            {
                var renditions = asset.GetPropertyValue<JToken>(Constants.Asset.Properties.Renditions);
                if(renditions != null)
                {                   
                    var renditionProperties = renditions.SelectToken($"{rendition.ToLowerInvariant()}.properties");
                    if (renditionProperties != null)
                    {
                        imageProperties = renditionProperties;
                    }                    
                }
            }

            var width = imageProperties.SelectToken("width");
            if (width != null)
            {
                publicLinkData.Width = width.ToObject<string>();
            }

            var height = imageProperties.SelectToken("height");
            if (height != null)
            {
                publicLinkData.Height = height.ToObject<string>();
            }

            return publicLinkData;
        }

        private static IEntityLoadConfiguration PublicLinkLoadConfiguration()
        {
            var propertyOptions = new PropertyLoadOption(new string[] { Constants.PublicLink.Properties.RelativeUrl, Constants.PublicLink.Properties.VersionHash, Constants.PublicLink.Properties.ConversionConfiguration });
            return new EntityLoadConfiguration(CultureLoadOption.All, propertyOptions, RelationLoadOption.None);
        }
    }
}