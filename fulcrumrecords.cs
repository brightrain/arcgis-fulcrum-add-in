using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace fulcrum
{
    //http://developer.fulcrumapp.com/api/fulcrum-api.html#records
    public class fulcrumrecords
    {
        public string current_page { get; set; }
        public string total_pages { get; set; }
        public string total_count { get; set; }
        public string per_page { get; set; }
        public List<fulcrumrecord> records { get; set; }
    }
    public class fulcrumrecord
    {
        public string id { get; set; }
        public string form_id { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string project_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string client_created_at { get; set; }
        public string client_updated_at { get; set; }
        public Dictionary<string, object> form_values { get; set; }
        public List<fulcrumattribute> attributes { get; set; }
        public List<Image> photos { get; set; }
        public List<string> photolinks { get; set; }
        public fulcrumrecord()
        {
            attributes = new List<fulcrumattribute>();
            photos = new List<Image>();
            photolinks = new List<string>();
        }
    }
    public class fulcrumattribute
    {
        public string recordID { get; set; }
        public string fieldName { get; set; }
        public string fieldValue { get; set; }
    }
}
