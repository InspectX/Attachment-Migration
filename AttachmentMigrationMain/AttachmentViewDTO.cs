using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttachmentMigrationMain
{
    public class AttachmentViewDTO
    {
        
        public string RECORD_ID { get; set; }
        [Optional]
        public int? DOCCODE { get; set; }
        [Optional]
        public string INSP_NUM { get; set; }
        [Optional]
        public string DOC_DESC { get; set; }
        [Optional]
        public string DOC_NAME { get; set; }
        [Optional]
        public string DOC_GROUP { get; set; }
        [Optional]
        public string DOC_CATEGORY { get; set; }
        [Optional]
        public string DOC_COMMENT { get; set; }
        [Optional]
        public int? FILE_SIZE { get; set; }
        [Optional]
        public string FILE_UPLOAD_BY { get; set; }
        [Optional]
        public DateTime? FILE_UPLOAD_DATE { get; set; }
        [Optional]
        public string DOC_RECORD_INSP { get; set; }
        [Optional]
        public string URL { get; set; }
        [Optional]
        public string TOKEN { get; set; }
        [Optional]
        public string DOC_CHECKLIST_ITEM { get; set; }
        [Optional]
        public string GUIDE_ITEM_COMMENT { get; set; }

    }
}
