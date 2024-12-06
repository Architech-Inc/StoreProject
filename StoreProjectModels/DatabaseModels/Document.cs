using System;
using System.Collections.Generic;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class Document
    {
        public int DocumentId { get; set; }
        public string EntityId { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string DocumentGuid { get; set; }
        public string FileExtension { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public string AccessLogs { get; set; }
    }
}
