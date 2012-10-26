using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.ADF.COMSupport;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Display;

namespace fulcrum
{
    public class ArcStuff
    {
        private const string photoFieldName = "Photos";

        public static bool DoArcStuff(fulcrumform form, fulcrumrecords records)
        {
            try
            {
                IFeatureClass newFeatureClass = CreateFeatureClass(form, Properties.Settings.Default.pathToGeoDB);
                InsertFeatures(records, newFeatureClass);
                IFeatureLayer layer = CreateFeatureLayer(newFeatureClass, form.name);
                IMap map = ArcMap.Document.FocusMap;
                map.AddLayer((ILayer)layer);
                ArcMap.Document.ActiveView.Refresh();
                return true;
            }
            catch (Exception e)
            {
                string bomb = e.Message;
                return false;
            }
        }
        
        public static IFeatureClass CreateFeatureClass(fulcrumform form, string pathToGeoDB)
        {
            try
            {
                ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
                IGeographicCoordinateSystem geographicCoordinateSystem = spatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
                ISpatialReference sr = geographicCoordinateSystem;

                IWorkspaceFactory2 wsf = new FileGDBWorkspaceFactoryClass();
                IWorkspace2 workspace = (IWorkspace2)wsf.OpenFromFile(pathToGeoDB, 0);
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

                string fcName = GetUniqueFeatureClassName(form.name, workspace);

                IFields fieldsCollection = new FieldsClass();
                IFieldEdit newField = fieldsCollection as IFieldEdit;

                IFeatureClassDescription fcDesc = new ESRI.ArcGIS.Geodatabase.FeatureClassDescriptionClass();

                IObjectClassDescription ocDesc = (IObjectClassDescription)fcDesc;
                // create required fields using the required fields method
                IFields fields = ocDesc.RequiredFields;

                //Grab the GeometryDef from the shape field, edit it and give it back
                int shapeFieldIndex = fields.FindField(fcDesc.ShapeFieldName);
                IField shapeField = fields.get_Field(shapeFieldIndex);
                IGeometryDef geomDef = shapeField.GeometryDef;
                IGeometryDefEdit geomDefEdit = (IGeometryDefEdit)geomDef;
                geomDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
                geomDefEdit.GridCount_2 = 1;
                geomDefEdit.set_GridSize(0, 0);
                geomDefEdit.SpatialReference_2 = sr;

                IFields newFields = CreateNewFields(form, fields, sr);

                IFeatureClass featureClass = featureWorkspace.CreateFeatureClass
                    (fcName, newFields, ocDesc.InstanceCLSID, ocDesc.ClassExtensionCLSID, esriFeatureType.esriFTSimple, fcDesc.ShapeFieldName, "");
                
                return featureClass;
            }
            catch (Exception e)
            {
                string errMsg = e.Message;
                return null;
            }
        }
        public static IFeatureLayer CreateFeatureLayer(IFeatureClass featureClass, string layerName)
        {
            try
            {
                IFeatureLayer featureLayer = new FeatureLayer();
                featureLayer.Name = layerName;
                featureLayer.FeatureClass = featureClass;
                return featureLayer;
            }
            catch (Exception e)
            {
                string ouch = e.Message;
                return null;
            }
                  
        }
        private static IFields CreateNewFields(fulcrumform form, IFields fields, ISpatialReference sr)
        {
            try
            {
                IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
                foreach (fulcrumelement element in form.elements)
                {
                    //!!! ToDo: Add data types as needed (numeric, etc)
                    if (element.data_name != "")
                    {
                        IField field = new FieldClass();
                        IFieldEdit2 fieldEdit = (IFieldEdit2)field;
                        switch (element.type)
                        {
                            case "Section":
                                //a Section is a whole different animal it will have sub elements (children) like so, element.elements
                                //!!!~but not dealing with them for now
                                fieldEdit.Name_2 = element.data_name;
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 100;
                                // add field to field collection
                                fieldsEdit.AddField(field);
                                break;
                            case "ClassificationField":
                                fieldEdit.Name_2 = element.data_name;
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 100;
                                fieldsEdit.AddField(field);
                                break;
                            case "ChoiceField":
                                fieldEdit.Name_2 = element.data_name;
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 100;
                                fieldsEdit.AddField(field);
                                break;
                            case "TextField":
                                fieldEdit.Name_2 = element.data_name;
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 255;
                                fieldsEdit.AddField(field);
                                break;
                            case "DateTimeField":
                                //if the name happens to be Date we need to override it
                                if (element.data_name == "date")
                                {
                                    fieldEdit.Name_2 = "record_date";
                                }
                                else
                                {
                                    fieldEdit.Name_2 = element.data_name;
                                }
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 50;
                                fieldsEdit.AddField(field);
                                break;
                            case "PhotoField":
                                fieldEdit.Name_2 = photoFieldName;
                                fieldEdit.IsNullable_2 = true;
                                fieldEdit.AliasName_2 = element.label;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                                fieldEdit.Length_2 = 200;
                                //If we want to stuff the image in a raster field, here's a start
                                /*!!!
                                IRasterDef rasterDef = new RasterDefClass();
                                rasterDef.Description = "PictureField";
                                rasterDef.IsManaged = true;
                                rasterDef.IsRasterDataset = true;
                                //rasterDef.SpatialReference = sr;
                            
                                 This is throwing an error
                                fieldEdit.RasterDef = rasterDef;
                                fieldEdit.Type_2 = esriFieldType.esriFieldTypeRaster;
                                 */
                                fieldsEdit.AddField(field);
                                break;
                            case "Label":
                                //skip it
                            default:
                                //a data type we haven't accounted for...
                                //don't create a field for it
                                break;
                        }
                        field = null;
                        fieldEdit = null;
                    }
                }
                fields = (IFields)fieldsEdit;

                // Use IFieldChecker to create a validated fields collection.
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IEnumFieldError enumFieldError = null;
                IFields validatedFields = null;
                fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

                // The enumFieldError enumerator can be inspected at this point to determine 
                // which fields were modified during validation.
                if (enumFieldError != null)
                {
                    enumFieldError.Reset();
                    IFieldError err = enumFieldError.Next();
                    while (!(err == null))
                    {
                        //!!!ToDo:
                        //we need to do something here cause the feature class will not be created with invalid fields
                        Console.WriteLine(err.FieldError + " at index " + err.FieldIndex);
                        err = enumFieldError.Next();
                    }
                }
                return validatedFields;
            }
            catch (Exception e)
            {
                string choke = e.Message;
                return null;
            }
        }

        private static bool InsertFeatures(fulcrumrecords fulcrumRecords, IFeatureClass featureClass)
        {
            try
            {
                    // Create an insert cursor.
                    IFeatureCursor insertCursor = featureClass.Insert(false);
                    //comReleaser.ManageLifetime(insertCursor);

                    foreach (fulcrumrecord record in fulcrumRecords.records)
                    {
                        // Create a feature buffer.
                        IFeatureBuffer featureBuffer = featureClass.CreateFeatureBuffer();
                        //comReleaser.ManageLifetime(featureBuffer);

                        IPoint pt = new Point();
                        pt.PutCoords(record.longitude, record.latitude);
                        featureBuffer.Shape = pt as IGeometry;
                        //handle photos separately
                        if (record.photos.Count >= 1)
                        {
                            try
                            {
                                //only link to the first photo for now
                                //ToDo: how to put the actual image in the photos field (the actual image is held in the photos property of the record)
                                //like so...
                                //featureBuffer.set_Value(featureClass.FindField("photos"), record.photos[0]);
                                featureBuffer.set_Value(featureClass.FindField(photoFieldName), record.photolinks[0]);
                                //ToDo: enhancement: multiple photo support; could create a separate table to store the photo links and join it back to resulting feature class via record id
                            }
                            catch (Exception e)
                            {
                                string photoInsertError = e.Message;
                            }
                        }
                        foreach (fulcrumattribute attr in record.attributes)
                        {
                            if (attr.fieldName != "photos")
                            {
                                //ToDo: why are some (first?) field names empty strings?
                                if (attr.fieldName != "" && attr.fieldName != null)
                                {
                                    try
                                    {
                                        featureBuffer.set_Value(featureClass.FindField(attr.fieldName), attr.fieldValue);
                                    }
                                    catch(Exception e)
                                    {
                                        string ouch = e.Message;
                                    }
                                }
                            }
                        }
                        try
                        {
                            insertCursor.InsertFeature(featureBuffer);
                            featureBuffer = null;
                            GC.Collect();
                        }
                        catch (Exception e)
                        {
                            string insertFailed = e.Message;
                        }
                    }
                    // Flush the buffer to the geodatabase.
                    insertCursor.Flush();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetGeodatabaseFromUser()
        {
            try
            {
                IEnumGxObject enumGxObject = null;
                IGxObjectFilter gxObjectFilterFGBs = new GxFilterFileGeodatabasesClass();
                IGxDialog gxDialog = new GxDialogClass();
                IGxObjectFilterCollection gxObjectFilterCollection = (IGxObjectFilterCollection)gxDialog;

                gxObjectFilterCollection.AddFilter(gxObjectFilterFGBs, false);

                gxDialog.Title = "Locate Geodatabase";
                gxDialog.AllowMultiSelect = false;
                gxDialog.DoModalOpen(0, out enumGxObject);

                if (!(enumGxObject == null))
                {
                    //we don't allow multiple select so grab the first one
                    IGxObject gxObj = enumGxObject.Next();
                    string pathToFGDB = gxObj.FullName;

                    return pathToFGDB;
                }
                else
                {
                    //user cancelled
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
        public static IFeatureLayer GetFeatureLayerByName(string layerName)
        {
            IEnumLayer enumLayer;
            try
            {
                enumLayer = ArcMap.Document.FocusMap.Layers;
                enumLayer.Reset();
                ILayer layer;
                while ((layer = enumLayer.Next()) != null)
                {
                    if (layer is FeatureLayer)
                    {
                        if (layer.Name == layerName)
                        {
                            return (IFeatureLayer)layer;
                        }
                    }
                }
                //if we made it here we didn't find a layer with the passed name
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetUniqueFeatureClassName(string name, IWorkspace2 workspace)
        {
            //ToDo: This works to get a unique name but it's goobery, need to implement recursion here...
            try
            {
            IFieldChecker fieldChecker = new FieldChecker();
            string fcName;
            fieldChecker.ValidateTableName(name, out fcName);
            int i = 1;
            while(workspace.get_NameExists(esriDatasetType.esriDTFeatureClass, fcName))
            {
                fcName = fcName + i;
                i++;
            }
            return fcName;
            }
            catch
            {
                return "";
            }
        }
    }
}
