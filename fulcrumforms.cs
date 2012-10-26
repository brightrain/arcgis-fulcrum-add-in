using System;
using System.Collections.Generic;
using System.Text;

namespace fulcrum
{
    //http://developer.fulcrumapp.com/api/fulcrum-api.html#forms
    class fulcrumforms
    {
        public string current_page { get; set; }
        public string total_pages { get; set; }
        public string total_count { get; set; }
        public string per_page { get; set; }
        public List<fulcrumform> forms { get; set; }
    }
    public class fulcrumform
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public System.Collections.ArrayList boundingbox { get; set; }
        public string record_count { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public fulcrumelement[] elements { get; set; }
    }
    public class fulcrumelement
    {
        public bool disabled { get; set; }
        public bool hidden { get; set; }
        public string key { get; set; }
        /*
        Every form element has a field type. These field types include:
        Section, ClassificationField, ChoiceField, TextField, PhotoField
        */
        public string type { get; set; }
        public string data_name { get; set; }
        public bool required { get; set; }
        public string label { get; set; }
        public fulcrumelement[] elements { get; set; }

    }
    class fulcrumphotos
    {
        public string photo { get; set; }
    }
}
