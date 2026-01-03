using System.Collections.Generic;

namespace MTM_Receiving_Application.ReceivingModule.Models
{
    public class Model_SaveResult
    {
        public bool Success { get; set; }
        public int LoadsSaved { get; set; }
        public bool LocalCSVSuccess { get; set; }
        public bool NetworkCSVSuccess { get; set; }
        public bool DatabaseSuccess { get; set; }
        public string? LocalCSVPath { get; set; }
        public string? NetworkCSVPath { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Legacy property support if needed, or just remove if unused
        public string ErrorMessage { get; set; } = string.Empty;
        public int RecordsSaved { get => LoadsSaved; set => LoadsSaved = value; }
        public bool IsSuccess { get => Success; set => Success = value; }
        public Model_CSVWriteResult? CSVExportResult { get; set; }

        public bool IsFullSuccess => LocalCSVSuccess && NetworkCSVSuccess && DatabaseSuccess;
        public bool IsPartialSuccess => (LocalCSVSuccess || DatabaseSuccess) && !IsFullSuccess;
    }
}
