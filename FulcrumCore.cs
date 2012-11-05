using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Web.Script.Serialization;
using System.Drawing;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace fulcrum
{
    static class FulcrumCore
    {
        //Per v2 documentation these are the field types
        //Not implementing, just documenting for now
        enum FulcrumFieldTypes
        {
            Section,
            ClassificationField,
            ChoiceField,
            TextField,
            PhotoField,
            DateTimeField,
            Label
        }

        public static bool FireUp(fulcrumform form, System.Windows.Controls.ProgressBar progBar, System.Windows.Controls.Label statusLabel)
        {
            try
            {
                statusLabel.Content = "Retrieving records...";
                fulcrumrecords records = FulcrumCore.GetRecords(form.id);
                statusLabel.Content = "Processing records...";
                FulcrumCore.ParseRecords(form, records, progBar);
                statusLabel.Content = "Creating layer...";
                ArcStuff.DoArcStuff(form, records);
                statusLabel.Content = "Complete!";
                return true;
            }
            catch (Exception e)
            {
                string ouch = e.Message;
                return false;
            }
        }
        public static fulcrumforms GetForms()
        {
            try
            {
                string reqString = FulcrumTools.apiBase + "/forms";
 
                HttpWebRequest req = WebRequest.Create(new Uri(reqString)) as HttpWebRequest;
                req.Method = "GET";
                req.Headers.Add("X-ApiToken:" + Properties.Settings.Default.fulcrumApiKey);
                //This is critical to authentication!!!
                req.Accept = "application/json";

                string result = null;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                    //Console.WriteLine(result.ToString());
                    //A good explanation of what the serializer returns
                    //http://www.tomasvera.com/programming/using-javascriptserializer-to-parse-json-objects/

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    fulcrumforms fulcrumForms = ser.Deserialize<fulcrumforms>(result);

                    return fulcrumForms;
                }

                /** A POST for writing back to fulcrum via api *
                // Encode the parameters as form data:
                byte[] formData = UTF8Encoding.UTF8.GetBytes(reqString);
                // Send the request:
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
                 * */
            }
            catch (Exception ex)
            {
                string ouch = ex.Message;
                return null;
            }
        }
        //Not used but you could use this to return a single form
        public static fulcrumform GetForm(string formId)
        {
            try
            {
                string reqString = FulcrumTools.apiBase + "/forms/" + formId;

                HttpWebRequest req = WebRequest.Create(new Uri(reqString)) as HttpWebRequest;
                req.Method = "GET";
                req.Headers.Add("X-ApiToken:" + Properties.Settings.Default.fulcrumApiKey);
                //This is critical to authentication!!!
                req.Accept = "application/json";

                string result = null;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    fulcrumform fulcrumForm = ser.Deserialize<fulcrumform>(result);

                    return fulcrumForm;
                }
            }
            catch (Exception ex)
            {
                string ouch = ex.Message;
                return null;
            }
        }
        public static fulcrumrecords GetRecords(string formId)
        {
            try
            {
                //if no form id is passed all records are returned
                string reqString;
                if (formId == string.Empty)
                {
                    reqString = FulcrumTools.apiBase + "/records";
                }
                else
                {
                    reqString = FulcrumTools.apiBase + "/records?form_id=" + formId;
                }
                HttpWebRequest req = WebRequest.Create(new Uri(reqString)) as HttpWebRequest;
                req.Method = "GET";
                req.Headers.Add("X-ApiToken:" + Properties.Settings.Default.fulcrumApiKey);
                //This is critical to authentication!!!
                req.Accept = "application/json";

                string result = null;
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                    //Console.WriteLine(result.ToString());

                    JavaScriptSerializer ser = new JavaScriptSerializer();
                    fulcrumrecords orecords = ser.Deserialize<fulcrumrecords>(result);
                    return orecords;
                }
            }
            catch (Exception ex)
            {
                string ouch = ex.Message;
                return null;
            }
        }
        public static bool ParseRecords(fulcrumform form, fulcrumrecords fulcrumRecords, System.Windows.Controls.ProgressBar progBar)
        {
            try
            {
                progBar.Maximum = fulcrumRecords.records.Count;
                //WPF Prog Bar, WTF?!
                //System.Windows.Media.Duration duration = new Duration(TimeSpan.FromSeconds(10));
                //System.Windows.Media.Animation.DoubleAnimation doubleanimation = new System.Windows.Media.Animation.DoubleAnimation(100.0, duration);
                //progBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, doubleanimation);
                int count = 1;
                foreach (fulcrumrecord record in fulcrumRecords.records)
                {
                    progBar.Value = count;
                    string recordId = record.id;
 
                    foreach (string key in record.form_values.Keys)
                    {
                        fulcrumattribute recordAttribute = new fulcrumattribute();
                        recordAttribute.recordID = recordId;
                        string fieldname;
                        string fieldType;
                        if (GetFieldNameAndTypeFromFormDefinition(form, key, out fieldname, out fieldType))
                        {
                            //if the value is a set of sub objects, we'll get another dictionary
                            //if the value is a javascript array, we'll get an arraylist
                            //otherwise we'll get the string value
                            object o = record.form_values[key];

                            if (o is Dictionary<string, object>)
                            {
                                //I think!!! the only time this will happen is when the value comes from a choice list
                                //so grab the value from the choice_values item

                                Dictionary<string, object> d = ((Dictionary<string, object>)o);
                                //ToDo: if this is a value\label pair we're always going to ge the coded value
                                //is there a way to determine if it is a coded value???
                                //e.g. coded value = 002, label = Krazy Kool
                                ArrayList a = (ArrayList)d["choice_values"];
                                recordAttribute.fieldName = fieldname;
                                recordAttribute.fieldValue = a[0].ToString();
                            }
                            else if (o is ArrayList)
                            {
                                foreach (object child in ((ArrayList)o))
                                {
                                    if (child is string)
                                    {
                                        recordAttribute.fieldName = fieldname;
                                        recordAttribute.fieldValue = child.ToString();
                                        //Console.WriteLine(fieldname + ": " + child.ToString());
                                    }
                                    else if (child is Dictionary<string, object>)
                                    {
                                        Dictionary<string, object> childDict = (Dictionary<string, object>)child;
                                        foreach (string childKey in childDict.Keys)
                                        {
                                            //ToDo: we are writing the link to the photo as well as
                                            //grabbing the photo thumb as an image and adding it to the records' photo collection
                                            //and saving it to disk
                                            //you will likely want to choose from these options, downloading is expensive
                                            if (fieldType == "PhotoField")
                                            {
                                                string photoId = childDict["photo_id"].ToString();
                                                string thumbUrl = childDict["thumbnail"].ToString();
                                                string url = childDict["url"].ToString();
                                                Image img = GetFulcrumImage(thumbUrl);
                                                record.photos.Add(img);
                                                string pathToPhoto = Properties.Settings.Default.pathToPhotoFolder + "\\" + photoId + ".jpg";
                                                //string linkToThumb = "http://web.fulcrumapp.com/photos/" + photoId + "/thumbnail";
                                                //string linkToPhoto = " http://web.fulcrumapp.com/photos/" + photoId;
                                                img.Save(pathToPhoto, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                //record.photolinks.Add(linkToPhoto);
                                                record.photolinks.Add(pathToPhoto);
                                            }
                                            else
                                            {
                                                //assuming all values are now strings
                                                recordAttribute.fieldName = childKey.ToString();
                                                recordAttribute.fieldValue = childDict[childKey].ToString();
                                                //Console.WriteLine(childKey.ToString() + ": " + childDict[childKey].ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                recordAttribute.fieldName = fieldname;
                                recordAttribute.fieldValue = o.ToString();
                                //Console.WriteLine(fieldname + ": " + o.ToString());
                            }
                            record.attributes.Add(recordAttribute);
                        }
                        else
                        {
                            //couldnt get the field name or type
                        }
                    }
                    count++;
                }
                return true;
            }
            catch (Exception e)
            {
                string ouch = e.Message;
                return false;
            }
        }
        private static Image GetFulcrumImage(string urlToImage)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("X-ApiToken:" + Properties.Settings.Default.fulcrumApiKey);
                byte[] imageBytes = webClient.DownloadData(urlToImage);
                MemoryStream mem = new MemoryStream(imageBytes);
                return Image.FromStream(mem);
            }
            catch
            {
                return null;
            }
        }

        private static bool GetFieldNameAndTypeFromFormDefinition(fulcrumform form, string fieldId, out string fieldName, out string fieldType)
        {
            try
            {
                foreach (fulcrumelement element in form.elements)
                {
                    if (element.elements == null)
                    {
                        if (element.key == fieldId)
                        {
                            fieldName = element.data_name;
                            fieldType = element.type;
                            return true;
                        }
                    }
                    else
                    {
                        //we have nested elements
                        //ToDo: implement recursion to get all, we are only going one level here
                        foreach (fulcrumelement childElement in element.elements)
                        {
                            if (childElement.key == fieldId)
                            {
                                fieldName = childElement.data_name;
                                fieldType = childElement.type;
                                return true;
                            }
                        }

                    }
                }
                fieldName = "";
                fieldType = "";
                return false;
            }
            catch (Exception e)
            {
                string ouch = e.Message;
                fieldName = "";
                fieldType = "";
                return false;
            }
        }
    }
}
