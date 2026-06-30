using AutoMapper;
using Pulse.ViewModels;
using Pulse.Core.Entities;
using Pulse.DataTransformationObjects;
using System.Linq;

namespace Pulse.Api
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<dtoUser, ActiveDirectoryUser>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.UserId))
            .ReverseMap()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.EmployeeId))
            ;


            CreateMap<dtoAnnotationType, AnnotationType>()
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => ((src.IsPrivate) ? 1 : 0)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsPrivate, opt => opt.MapFrom(src => (src.IsPrivate == 1)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoCategory, Category>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoCategoryWithStats, CategoryWithStats>()
            .ReverseMap();

            CreateMap<dtoForm, Form>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoFormExtended, Form>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoFormExtended, FormExtended>()
            .ReverseMap();

            CreateMap<dtoFormEntityLink, FormExtended>()
            .ReverseMap();

            CreateMap<dtoFormEntityLink, Form>()
            .ReverseMap();

            CreateMap<dtoNodeForm, RootFormRow>()
            .ForMember(dest => dest.FormRootKey, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.FormRootId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.FormDescription, opt => opt.MapFrom(src => src.Desc))
            .ReverseMap()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.FormRootKey))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FormRootId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FormName))
            .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => src.FormDescription));


            CreateMap<dtoField, Field>() 
            .ForMember(dest => dest.UrlIsParam, opt => opt.MapFrom(src => ((src.UrlIsParameter) ? 1 : 0)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap() 
            .ForMember(dest => dest.UrlIsParameter, opt => opt.MapFrom(src => (src.UrlIsParam == 1)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoFieldOption, dtoFieldOption>()
            .ReverseMap();

            CreateMap<dtoFieldRule, FieldRule>()
            .ReverseMap();

            CreateMap<dtoFieldWithStats, FieldWithStats>()
            .ReverseMap();




            CreateMap<dtoFormField, FormField>()
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.IsRequired) ? 1 : 0)))
            .ForMember(dest => dest.UrlIsParam, opt => opt.MapFrom(src => ((src.UrlIsParameter) ? 1 : 0)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => (src.IsRequired == 1)))
            .ForMember(dest => dest.UrlIsParameter, opt => opt.MapFrom(src => (src.UrlIsParam == 1)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoFormFieldOption, dtoFormFieldOption>()
            .ReverseMap();

            CreateMap<dtoFormFieldRule, FormFieldRule>()
            .ReverseMap();

            CreateMap<dtoFormEntityLink, FormEntityLink>()
            .ReverseMap();

            CreateMap<dtoNodeForm, NodeFormRow>()
            .ForMember(dest => dest.FormNodeKey, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.FormNodeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FormName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.FormDescription, opt => opt.MapFrom(src => src.Desc))
            .ReverseMap()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.FormNodeKey))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FormNodeId))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FormName))
            .ForMember(dest => dest.Desc, opt => opt.MapFrom(src => src.FormDescription));



            CreateMap<dtoNode, NodeRow>()
            .ForMember(dest => dest.NodeKey, opt => opt.MapFrom(src => src.Key))
            .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NodeType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.DataName, opt => opt.MapFrom(src => src.Data.Name))
            .ForMember(dest => dest.DataDescription, opt => opt.MapFrom(src => src.Data.Desc))
            .ForMember(dest => dest.DataIsRequired, opt => opt.MapFrom(src => src.Data.IsRequired.Value ? 1 : 0))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Data.IsActive.Value ? 1 : 0))
            .ForMember(dest => dest.DataMandays, opt => opt.MapFrom(src => src.Data.Mandays))
            .ForMember(dest => dest.DataMaturityCode, opt => opt.MapFrom(src => src.Data.Maturity))
            .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.Data.TransactionKey))
             .ReverseMap()
            .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.NodeKey))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.NodeId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.NodeType))
            .ForPath(dest => dest.Data.Name, opt => opt.MapFrom(src => src.DataName))
            .ForPath(dest => dest.Data.Desc, opt => opt.MapFrom(src => src.DataDescription))
            .ForPath(dest => dest.Data.IsRequired, opt => opt.MapFrom(src => src.DataIsRequired == null ? (bool?)null : src.DataIsRequired == 1))
            .ForPath(dest => dest.Data.IsActive, opt => opt.MapFrom(src => src.IsActive == null ? (bool?)null : src.IsActive == 1))
            .ForPath(dest => dest.Data.Mandays, opt => opt.MapFrom(src => src.DataMandays))
            .ForPath(dest => dest.Data.Maturity, opt => opt.MapFrom(src => src.DataMaturityCode))
            .ForPath(dest => dest.Data.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey));





            CreateMap<dtoMaturityLevel, MaturityLevel>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;
            CreateMap<dtoMaturityLevelWithStats, MaturityLevelWithStats>()
            .ReverseMap()
            ;


            CreateMap<dtoModule, Module>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoPlant, Plant>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoPlantRoadmapLink, PlantRoadmapLink>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsSelected) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsSelected, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;



            CreateMap<dtoPlantWithStats, PlantWithStats>()
              .ReverseMap();

            CreateMap<dtoProductDivision, ProductDivision>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;


            CreateMap<dtoProduct, Product>()
            .ReverseMap();

            CreateMap<dtoProductDivisionWithStats, ProductDivisionWithStats>()
            .ReverseMap();


            CreateMap<dtoProductGroup, ProductGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoProductGroupWithStats, ProductGroupWithStats>()
            .ReverseMap();

            CreateMap<dtoProductionCalendar, ProductionCalendar>()
            .ReverseMap();

            CreateMap<dtoProductionCalendarWithStats, ProductionCalendarWithStats>()
            .ReverseMap();


            CreateMap<ProjectInitViewModel, Project>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.ProjectDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ProjectIcon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.ProjectIconColor, opt => opt.MapFrom(src => src.IconColor))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.AutoStart ? "ONGOING" : "NOT STARTED"))
                .ForMember(dest => dest.TargetStartYear, opt => opt.MapFrom(src => ParseIntOrNull(src.ProjectstartYear)))
                .ForMember(dest => dest.TargetStartWorkWeek, opt => opt.MapFrom(src => src.ProjectstartWorkWeek))
                .ForMember(dest => dest.TargetCompletionYear, opt => opt.MapFrom(src => ParseIntOrNull(src.ProjectendYear)))
                .ForMember(dest => dest.TargetCompletionWorkWeek, opt => opt.MapFrom(src => src.ProjectendWorkWeek))
                .ForMember(dest => dest.ActualStartDate, opt => opt.MapFrom(src => src.ActualStartDate))
                .ForMember(dest => dest.PlantCode, opt => opt.MapFrom(src => src.SiteValue))
                .ForMember(dest => dest.CategoryCode, opt => opt.MapFrom(src => src.TemplateCategoryValue))
                .ForMember(dest => dest.ProductGroupCode, opt => opt.MapFrom(src => src.ProductgroupValue))
                .ForMember(dest => dest.ProductDivisionCode, opt => opt.MapFrom(src => src.ProductdivisionValue))
                .ForMember(dest => dest.ProjectOwnerUserName, opt => opt.MapFrom(src => src.OwnerValue))
                .ForMember(dest => dest.ProjectMaturityCode, opt => opt.MapFrom(src => src.ProjectMaturityCode))
                .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.MapFrom(src => src.CurrentMilestoneSysId))
                .ForMember(dest => dest.RoadmapSysId, opt => opt.MapFrom(src => src.TemplateValue))
                .ForMember(dest => dest.PlantRoadmapLinkSysId, opt => opt.MapFrom(src => src.TemplatePlantRoadmapLinkSysId))
                .ForMember(dest => dest.TargetStartedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TargetCompletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Plant, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedByUser, opt => opt.Ignore())
                .ForMember(dest => dest.AuditTrails, opt => opt.Ignore())
                .ForMember(dest => dest.StatusChanges, opt => opt.Ignore())
                .ForMember(dest => dest.Tasks, opt => opt.Ignore())
                .ForMember(dest => dest.Annotations, opt => opt.Ignore())
                .ForMember(dest => dest.Fields, opt => opt.Ignore())
                // Collections
                .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members))
                .ForMember(dest => dest.Milestones, opt => opt.MapFrom(src => src.Milestones))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.ProductCodes))
                .AfterMap((src, dest) =>
                {
                    dest.Members.Clear();
                    if (src.Members != null)
                    {
                        foreach (var ownerName in src.Members.Distinct())
                        {
                            dest.Members.Add(new ProjectMember
                            {
                                UserId = "", // resolve later 
                                User = new User { UserName = ownerName.Name.Replace("(", "|").Replace(")", "").Split('|')[1] }
                            });
                        }
                    }

                    ////if (src.ProductCodes != null)
                    ////{
                    ////    foreach (var product in src.ProductCodes.Distinct())
                    ////    {
                    ////        dest.Products.Add(new ProjectProduct
                    ////        {
                    ////            ProductCode = product.ProductCode,
                    ////            Product = new Product
                    ////            {
                    ////                ProductCode = product.ProductCode,
                    ////                ProductCurrentMaturity = product.Maturity,
                    ////                PlantCode = "", // resolve later 
                    ////                PlantType = product.PlantType,
                    ////                PlantTypeDescription = product.PlantTypeDesc,
                    ////                ProductFamilyCode = product.ProductFamily,
                    ////                ProductFamilyDescription = product.ProductFamilyDesc,
                    ////                MacroPackageCode = product.MacroPackage,
                    ////                MacroPackageDescription = product.MacroPackageDesc,
                    ////                PackageCode = product.Pack,
                    ////                PackageDescription = product.PackDesc,
                    ////                ProductLineCode = product.PLine,
                    ////                ProductLine = product.PLineDesc,
                    ////                MaturityCode = product.Maturity,
                    ////                CreatedBy = ""
                    ////            }
                    ////        });
                    ////    }
                    ////}
                });




            CreateMap<dtoMember, ProjectMember>()
                .ForMember(dest => dest.ProjectMemberSysId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectNo, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // Milestones
            CreateMap<dtoMilestone, ProjectMilestone>()
                .ForMember(dest => dest.TargetStartYear, opt => opt.MapFrom(src => ParseIntOrNull(src.StartDate)))
                .ForMember(dest => dest.TargetStartWorkWeek, opt => opt.MapFrom(src => src.StartWeek))
                .ForMember(dest => dest.TargetCompletionYear, opt => opt.MapFrom(src => ParseIntOrNull(src.EndDate)))
                .ForMember(dest => dest.TargetCompletionWorkWeek, opt => opt.MapFrom(src => src.EndWeek))
                .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.MapFrom(src => src.Meta.Id))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
                .ForMember(dest => dest.IsRequired, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectNo, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.PlantRoadmapLinkSysId, opt => opt.Ignore())
                .ForMember(dest => dest.RoadmapSysId, opt => opt.Ignore())
                .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Remarks, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionKey, opt => opt.Ignore())
                .ForMember(dest => dest.Owners, opt => opt.Ignore())
                .ForMember(dest => dest.StatusChanges, opt => opt.Ignore())
                .ForMember(dest => dest.TargetRevisions, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.Owners.Clear();
                    dest.RoadmapMilestoneSysId = (src.Meta.Id != "__ROOT_ACTIVITIES__" ? src.Meta.Id : "");
                    dest.IsRequired = (src.Meta.IsRequired) ? 1 : 0;
                    if (src.Owners != null)
                    {
                        foreach (var owner in src.Owners.Distinct())
                        {
                            dest.Owners.Add(new ProjectOwner
                            {
                                ParentType = "milestone",
                                ParentSysId = "", // resolve later 
                                OwnerMeta = new User { UserName = owner.Replace("(", "|").Replace(")", "").Split('|')[1] }

                            });
                        }
                    }

                    //dest.Tasks.Clear();
                    //if (src.Tasks != null)
                    //{
                    //    foreach (var task in src.Tasks.Distinct())
                    //    {
                    //        dest.Tasks.Add(new ProjectTask
                    //        {

                    //            ParentType = "task",
                    //            ParentSysId = "", // resolve later  

                    //        });
                    //    }
                    //}


                });


            CreateMap<dtoProjectFormSubmission, ProjectFormSubmission>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)));

            CreateMap<dtoProjectFormSubmission, ProjectFormSubmissionExtended>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)));


            // Tasks
            CreateMap<dtoTask, ProjectTask>()
                .ForMember(dest => dest.RoadmapActivitySysId, opt => opt.MapFrom(src => src.Meta.Id))
                .ForMember(dest => dest.TargetStartYear, opt => opt.MapFrom(src => ParseIntOrNull(src.StartDate)))
                .ForMember(dest => dest.TargetStartWorkWeek, opt => opt.MapFrom(src => src.StartWeek))
                .ForMember(dest => dest.TargetCompletionYear, opt => opt.MapFrom(src => ParseIntOrNull(src.EndDate)))
                .ForMember(dest => dest.TargetCompletionWorkWeek, opt => opt.MapFrom(src => src.EndWeek))
                .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.Meta.IsRequired.Value) ? 1 : 0)))
                .ForMember(dest => dest.ParentSysId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentType, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectNo, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualStartedBy, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCompletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.PlantRoadmapLinkSysId, opt => opt.Ignore())
                .ForMember(dest => dest.RoadmapSysId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Remarks, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionKey, opt => opt.Ignore())
                .ForMember(dest => dest.SubTasks, opt => opt.Ignore())
                .ForMember(dest => dest.Milestones, opt => opt.Ignore())
                .ForMember(dest => dest.Owners, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    dest.RoadmapActivitySysId = src.Meta.Id;
                    dest.TargetStartYear = ParseIntOrNull(src.StartDate);
                    dest.TargetStartWorkWeek = src.StartWeek;
                    dest.TargetCompletionYear = ParseIntOrNull(src.EndDate);
                    dest.TargetCompletionWorkWeek = src.StartWeek;
                    dest.Owners.Clear();

                    if (src.Owners != null)
                    {
                        foreach (var owner in src.Owners.Distinct())
                        {
                            dest.Owners.Add(new ProjectOwner
                            {
                                ParentType = "task",
                                ParentSysId = "", // resolve later 
                                OwnerMeta = new User { UserName = owner.Replace("(", "|").Replace(")", "").Split('|')[1] }

                            });
                        }
                    }
                });

            // Product codes -> ProjectProducts (needs lookup later)
            CreateMap<dtoProductCodeRow, ProjectProduct>()
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode))
                .ForMember(dest => dest.ProjectNo, opt => opt.Ignore())
                 .AfterMap((src, dest) =>
                 {
                     dest.Product = new Product
                     {
                         ProductCode = src.ProductCode,
                         ProductCurrentMaturity = src.Maturity,

                         PlantType = src.PlantType,
                         PlantTypeDescription = src.PlantTypeDesc,
                         ProductFamilyCode = src.ProductFamily,
                         ProductFamilyDescription = src.ProductFamilyDesc,
                         MacroPackageCode = src.MacroPackage,
                         MacroPackageDescription = src.MacroPackageDesc,
                         PackageCode = src.Pack,
                         PackageDescription = src.PackDesc,
                         ProductLineCode = src.PLine,
                         ProductLine = src.PLineDesc,
                         MaturityCode = src.Maturity,
                         PlantCode = "", // resolve later 
                         CreatedBy = "" // resolve later 
                     };
                 });

            CreateMap<ProjectUpdateViewModel, Project>()
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.ProjectNo))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.ProjectDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ProjectIcon, opt => opt.MapFrom(src => src.Icon))
                .ForMember(dest => dest.ProjectIconColor, opt => opt.MapFrom(src => src.IconColor))
                .ForMember(dest => dest.ProductGroupCode, opt => opt.MapFrom(src => src.ProductGroupCode))
                .ForMember(dest => dest.ProductDivisionCode, opt => opt.MapFrom(src => src.ProductDivisionCode))
                .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey))
                 ;


            CreateMap<ProjectFormSubmission, ProjectFormSubmit>()
            .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey))
            .ReverseMap()
            .AfterMap((src, dest) =>
            {

                dest.SubmissionValues.Clear();

                if (src.Fields != null)
                {
                    foreach (var field in src.Fields.Distinct())
                    {
                        dest.SubmissionValues.Add(new ProjectFormSubmissionValue
                        {
                            ProjectNo = src.ProjectNo,
                            FormFieldSysId = field.FormFieldSysId,
                            FieldValue = field.Type.ToLower() != "richtext" ? field.Value : null,
                            FieldValueClob = field.Type.ToLower() == "richtext" ? field.Value : null,
                            EntitySysId = field.EntitySysId,
                            EntityType = field.EntityType,
                            SubmissionSysId = src.SubmissionSysId,
                            SubmissionValueSysId = field.SubmissionValueSysId,
                            FormSysId = field.FormSysId,
                            FormEntityLinkSysId = field.FormEntityLinkSysId,
                            TransactionKey = field.TransactionKey,
                            CreatedBy = src.CreatedBy,
                            ModifiedBy = src.ModifiedBy


                        });
                    }
                }
            });

            CreateMap<ProjectExtend, ProjectMilestone>()
                    .ForMember(dest => dest.MilestoneSysId, opt => opt.MapFrom(src => src.ProjectNodeSysId))
                    .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey));

            CreateMap<ProjectExtend, ProjectTask>()
                    .ForMember(dest => dest.ProjectTaskSysId, opt => opt.MapFrom(src => src.ProjectNodeSysId))
                    .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey));


            CreateMap<ProjectFormSubmissionValue, dtoFormSubmissionValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SubmissionValueSysId))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.FieldValueClob ?? src.FieldValue))
                .ForMember(dest => dest.TransactionKey, opt => opt.MapFrom(src => src.TransactionKey))
                .ForMember(dest => dest.SubmissionSysId, opt => opt.MapFrom(src => src.SubmissionSysId))
                .ForMember(dest => dest.SubmissionTransactionKey, opt => opt.MapFrom(src => src.SubmissionTransactionKey))
                .ForMember(dest => dest.FormSysId, opt => opt.MapFrom(src => src.FormSysId))
                .ForMember(dest => dest.FormEntityLinkSysId, opt => opt.MapFrom(src => src.FormEntityLinkSysId))
                .ForMember(dest => dest.EntitySysId, opt => opt.MapFrom(src => src.EntitySysId))
                .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.EntityType))
                .ForMember(dest => dest.FormFieldSysId, opt => opt.MapFrom(src => src.FormFieldSysId));

            CreateMap<UserGroupModule, UserGroupAccessRight>()
                .ForMember(dest => dest.UserGroupAccessRightSysId, opt => opt.MapFrom(src => src.Id))
                .ReverseMap();

            CreateMap<dtoSubmissionValue, ProjectFormSubmissionValue>()
                .ReverseMap();

            CreateMap<dtoRoadmap, Roadmap>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoRoadmapExtended, RoadmapExtended>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoRoadmapExtended, Roadmap>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoRoadmapActivity, RoadmapActivity>()
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.IsRequired) ? 1 : 0)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))

            .ReverseMap()
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => (src.IsRequired == 1)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<dtoRoadmapMilestone, RoadmapMilestone>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.IsRequired) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsRequired == 1)))
            ;

            CreateMap<dtoRoadmapActivityPrerequisite, RoadmapActivityPrerequisite>()
            .ReverseMap()
            ;

            CreateMap<dtoUser, User>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            CreateMap<ActiveDirectoryUser, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.EmployeeId ?? src.STEduid))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));



            CreateMap<dtoUserGroup, UserGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ((src.IsActive) ? 1 : 0)))
            .ReverseMap()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => (src.IsActive == 1)))
            ;

            ////            CreateMap<dtoTask, Task>()
            ////           .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.IsRequired) ? 1 : 0)))
            ////            .ReverseMap()
            ////            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => (src.IsRequired == 1)))
            ////;

            ////            CreateMap<dtoTaskSearchQuery, Task>()
            ////           .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => ((src.IsRequired) ? 1 : 0)))
            ////            .ReverseMap()
            ////            .ForMember(dest => dest.IsRequired, opt => opt.MapFrom(src => (src.IsRequired == 1)))
            ////;

            CreateMap<dtoStructRoadmapForm, dtoFormEntityLink>()
            .ForMember(dest => dest.FormEntityLinkSysId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FormSysId, opt => opt.MapFrom(src => src.Key));

            CreateMap<dtoStructRoadmapTreeNode, dtoRoadmapMilestone>()
           .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.MaturityCode, opt => opt.MapFrom(src => src.Data.Maturity))
           .ForMember(dest => dest.MilestoneAlias, opt => opt.MapFrom(src => src.Data.Name))
           .ForMember(dest => dest.MilestoneDescription, opt => opt.MapFrom(src => src.Data.Desc))
           .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.MapFrom(src => src.Key))
           .ForMember(dest => dest.RoadmapMilestoneSysId, opt => opt.MapFrom(src => src.Key))
           .ForMember(dest => dest.Forms, opt => opt.MapFrom((src, dest, _, ctx) =>
                src.Forms.Select(f =>
                {
                    var mapped = ctx.Mapper.Map<dtoStructRoadmapForm>(f);
                    mapped.ParentType = src.Type;
                    mapped.ParentId = src.Id;
                    return mapped;
                })));

        }
        private static int? ParseIntOrNull(string s)
        {
            if (int.TryParse(s, out var v)) return v;
            return null;
        }
    }
}