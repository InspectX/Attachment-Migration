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
        public int? DOCCODE { get; set; }
        public string INSP_NUM { get; set; }
        public string DOC_DESC { get; set; }
        public string DOC_NAME { get; set; }
        public string DOC_GROUP { get; set; }
        public string DOC_CATEGORY { get; set; }
        public string DOC_COMMENT { get; set; }
        public int? FILE_SIZE { get; set; }
        public string FILE_UPLOAD_BY { get; set; }
        public DateTime? FILE_UPLOAD_DATE { get; set; }
        public string DOC_RECORD_INSP { get; set; }
        public string URL { get; set; }
        public string TOKEN { get; set; }
        public string DOC_CHECKLIST_ITEM { get; set; }
        public string GUIDE_ITEM_COMMENT { get; set; }

    }
}
