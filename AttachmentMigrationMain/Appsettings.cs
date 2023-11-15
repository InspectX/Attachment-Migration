using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttachmentMigrationMain
{
    public class Appsettings
    {
        public string BravoConnectionString { get; set; }
        public string MigrationAttachmentXSLPath { get; set; }
        public string TasksPath { get; set; }
        public string AppealsPath { get; set; }
    }
}
