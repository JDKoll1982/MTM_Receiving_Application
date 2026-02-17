using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Models
{
    public class Model_SaveResult
    {
        public bool Success { get; set; }
        public int LoadsSaved { get; set; }
        public bool LocalXLSSuccess { get; set; }
        public bool NetworkXLSSuccess { get; set; }
        public bool DatabaseSuccess { get; set; }
        public string? LocalXLSPath { get; set; }
        public string? NetworkXLSPath { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Legacy property support if needed, or just remove if unused
        public string ErrorMessage { get; set; } = string.Empty;
        public int RecordsSaved { get => LoadsSaved; set => LoadsSaved = value; }
        public bool IsSuccess { get => Success; set => Success = value; }
        public Model_XLSWriteResult? CSVExportResult { get; set; }

        // Guided mode consolidated CSV status (single CSV file)
        public bool XLSFileSuccess => LocalXLSSuccess;
        public string XLSFileErrorMessage { get; set; } = string.Empty;
        public string DatabaseErrorMessage { get; set; } = string.Empty;

        public bool IsFullSuccess => LocalXLSSuccess && NetworkXLSSuccess && DatabaseSuccess;
        public bool IsPartialSuccess => (LocalXLSSuccess || DatabaseSuccess) && !IsFullSuccess;
    }
}
