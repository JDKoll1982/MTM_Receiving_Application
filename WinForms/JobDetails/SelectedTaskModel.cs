using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MTM_Waitlist_Application_2._0.WinForms.JobDetails.SelectedTaskModel;

namespace MTM_Waitlist_Application_2._0.WinForms.JobDetails
{
    public class SelectedTaskModel
    {
        public ObservableCollection<SelectedTaskModel> SelectedTaskModelList { get; set; }
        public string? JobType { get; set; }
        public string? Priority { get; set; }
        public string? JobStatus { get; set; }
        public string? WorkStation { get; set; }
        public string? WorkOrder { get; set; }
        public string? PartNumber { get; set; }
        public string? Operation { get; set; }
        public string? PartDescription { get; set; }
        public string? DieFgt { get; set; }
        public string? DieLocation { get; set; }
        public string? Component1 { get; set; }
        public string? Component2 { get; set; }
        public string? Component3 { get; set; }
        public string? Component4 { get; set; }
        public string? Component5 { get; set; }
        public string? Component6 { get; set; }
        public string? Component7 { get; set; }
        public string? Component8 { get; set; }
        public string? Component9 { get; set; }
        public string? Component10 { get; set; }
        public string? Component11 { get; set; }
        public string? Component12 { get; set; }
        public string? Container { get; set; }
        public string? Skid { get; set; }
        public string? Cardboard { get; set; }
        public string? Box { get; set; }
        public string? Other1 { get; set; }
        public string? Other2 { get; set; }
        public string? Other3 { get; set; }
        public string? Other4 { get; set; }
        public string? Other5 { get; set; }
        public string? SetupNotes { get; set; }        
        public string Operator { get; set; }
        public string? SetupTech { get; set; }
        public string? MaterialHandler { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? ElapsedTime { get; set; }

        public SelectedTaskModel()
        {
            JobType = "JobType";
            Priority = "Priority";
            JobStatus = "JobStatus";
            WorkStation = "WorkStation";
            WorkOrder = "WorkOrder";
            PartNumber = "PartNumber";
            Operation = "Operation";
            PartDescription = "PartDescription";
            DieFgt = "DieFgt";
            DieLocation = "DieLocation";
            Component1 = "Component1";
            Component2 = "Component2";
            Component3 = "Component3";
            Component4 = "Component4";
            Component5 = "Component5";
            Component6 = "Component6";
            Component7 = "Component7";
            Component8 = "Component8";
            Component9 = "Component9";
            Component10 = "Component10";
            Component11 = "Component11";
            Component12 = "Component12";
            Container = "Container";
            Skid = "Skid";
            Cardboard = "Cardboard";
            Box = "Box";
            Other1 = "Other1";
            Other2 = "Other2";
            Other3 = "Other3";
            Other4 = "Other4";
            Other5 = "Other5";
            SetupNotes = "SetupNotes";
            Operator = "Operator";
            SetupTech = "SetupTech";
            MaterialHandler = "MaterialHandler";
            StartTime = "StartTime";
            EndTime = "EndTime";
            ElapsedTime = "ElapsedTime";               
        }
    }
}
