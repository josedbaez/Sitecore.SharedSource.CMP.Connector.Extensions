# Sitecore.SharedSource.CMP.Connector.Extensions #
This module extends [Sitecore Connect™ for Sitecore CMP 2.0.0](https://dev.sitecore.net/Downloads/Sitecore_Connect_for_Sitecore_CMP/20/Sitecore_Connect_for_Sitecore_CMP_200.aspx) so images set on CMP entities can be synchronised.

It will synchronise to image fields using the xml format used by [Sitecore Connect™ for Sitecore DAM 2.0.0](https://dev.sitecore.net/Downloads/Sitecore_Plugin_for_Stylelabs_DAM/20/Sitecore_Connect_for_Sitecore_DAM_200.aspx), so this module is required. 

[Blog with explanation here] (https://josedbaez.com/2020/02/sitecore-content-hub-cmp-connector-extensions/).

## Installation Instructions ##
- Install [Sitecore Connect™ for Sitecore DAM 2.0.0](https://dev.sitecore.net/Downloads/Sitecore_Plugin_for_Stylelabs_DAM/20/Sitecore_Connect_for_Sitecore_DAM_200.aspx)
- Install [Sitecore Connect™ for Sitecore CMP 2.0.0](https://dev.sitecore.net/Downloads/Sitecore_Connect_for_Sitecore_CMP/20/Sitecore_Connect_for_Sitecore_CMP_200.aspx).
- Install package inside items folder. It contains one template and adds the template to standard values of CMP entity mapping item.
- Deploy code.

## Usage ##
- Create an Image Field Mapping item and populate the fields
    - `CMP Field Name` is the relation containing the image.
    - `Sitecore Field Name` is the sitecore image field to map to.
    - `Asset index` is the index number from a collection of images.
    - `Rendition` the image rendition to use.
