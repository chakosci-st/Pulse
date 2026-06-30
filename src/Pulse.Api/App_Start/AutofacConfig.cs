using Autofac;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Autofac.Integration.WebApi;
using Pulse.Infrastructure.DataAccess;
using Pulse.Infrastructure.Repositories;
using Pulse.Infrastructure.Services;
using Pulse.Core.Interfaces;
using Pulse.Services.Implementations;
using System.Web.Http; 
using Pulse.Core.EventArgs;
using Pulse.Api.Filters;
using log4net;
using Pulse.Infrastructure.Events;

namespace Pulse.Api
{
    public class AutofacConfig
    {
        public static void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Set the dependency resolver
            var config = GlobalConfiguration.Configuration;

            // Register Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // Register the Web API filter provider BEFORE building the container
            builder.RegisterWebApiFilterProvider(config);

            // ========================================================
            // Configuration values (AD, SMTP, etc.)
            // ========================================================
            string domain = ConfigurationManager.AppSettings["ad:domain"];
            string domainContainer = ConfigurationManager.AppSettings["ad:domaincontainer"];
            string domainWeb = ConfigurationManager.AppSettings["ad:domainweb"];
            string ldapServer = ConfigurationManager.AppSettings["ad:ldapserver"];
            string ldapContainer = ConfigurationManager.AppSettings["ad:ldapservercontainer"];
            string adGroupExtEmail = ConfigurationManager.AppSettings["ad:adgroup-emailext"];

            string smtpHost = ConfigurationManager.AppSettings["smtp:ClientHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["smtp:ClientPort"]);
            string smtpFromAddress = ConfigurationManager.AppSettings["smtp:ClientMailFromAddress"];
            string stmpFromDisplay = ConfigurationManager.AppSettings["smtp:ClientMailFromDisplay"];
            var urlHome = ConfigurationManager.AppSettings["cors-Origins"];
             




            // ========================================================
            // DAL
            // ========================================================
            builder.Register(c =>
            {
                var connection = "PulseConnection";
                var connectionString = ConfigurationManager.ConnectionStrings["defaultconnection"]?.ConnectionString ?? "";
                var logger = LogManager.GetLogger(typeof(Pulse.Infrastructure.DataAccess.OracleDataAccessLayer));
                return new OracleDataAccessLayer(connection, connectionString, logger);
            })
            .AsSelf()
            .InstancePerLifetimeScope();

            // ========================================================
            // Log4Net
            // ========================================================
            builder.Register((c, p) =>
            {
                var type = p.TypedAs<Type>();
                return LogManager.GetLogger(type);
            }).As<ILog>().InstancePerDependency();

            // ========================================================
            // Services
            // ========================================================

            // Email sender as singleton
            builder.Register(c =>
            {
                var host = ConfigurationManager.AppSettings["smtp:ClientHost"];
                var port = int.Parse(ConfigurationManager.AppSettings["smtp:ClientPort"]);
                var fromemail = ConfigurationManager.AppSettings["smtp:ClientMailFromAddress"];
                var displayname = ConfigurationManager.AppSettings["smtp:ClientMailFromDisplay"];
                return new SmtpEmailSenderService(host, port, fromemail, displayname);
            }).As<IEmailSender>().As<IEmailService>().SingleInstance();

            // AD service
            builder.RegisterType<ActiveDirectoryService>()
               .As<IActiveDirectoryService>()
               .WithParameter("domain", domain)
               .WithParameter("domainContainer", domainContainer)
               .WithParameter("domainWeb", domainWeb)
               .WithParameter("ldapServer", ldapServer)
               .WithParameter("ldapContainer", ldapContainer)
               .WithParameter("adGroupExtEmail", adGroupExtEmail);

            // Application Service
            builder.RegisterType<ApplicationService>()
               .As<IApplicationService>()
               .WithParameter("urlHome", urlHome)
               .SingleInstance();

            // Business services
            builder.RegisterType<AnnotationTypeService>().As<IAnnotationTypeService>().InstancePerRequest();
            builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerRequest();
            builder.RegisterType<FormService>().As<IFormService>().InstancePerRequest();
            builder.RegisterType<MaturityLevelService>().As<IMaturityLevelService>().InstancePerRequest();
            builder.RegisterType<ModuleService>().As<IModuleService>().InstancePerRequest();
            builder.RegisterType<PlantService>().As<IPlantService>().InstancePerRequest();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerRequest();
            builder.RegisterType<ProductDivisionService>().As<IProductDivisionService>().InstancePerRequest();
            builder.RegisterType<ProductGroupService>().As<IProductGroupService>().InstancePerRequest();
            builder.RegisterType<ProductionCalendarService>().As<IProductionCalendarService>().InstancePerRequest();
            builder.RegisterType<ProjectService>().As<IProjectService>().InstancePerRequest();
            builder.RegisterType<ProjectFormService>().As<IProjectFormService>().InstancePerRequest();
            builder.RegisterType<ProjectMilestoneService>().As<IProjectMilestoneService>().InstancePerRequest(); 
            builder.RegisterType<ProjectTaskService>().As<IProjectTaskService>().InstancePerRequest();
            builder.RegisterType<ProjectAttachmentService>().As<IProjectAttachmentService>().InstancePerRequest();
            builder.RegisterType<ProjectCommentService>().As<IProjectCommentService>().InstancePerRequest();
            builder.RegisterType<ProjectChatService>().As<IProjectChatService>().InstancePerRequest();
            //builder.RegisterType<ProjectNotificationService>().As<IProjectNotificationService>().InstancePerRequest();
            builder.RegisterType<ProjectMemberService>().As<IProjectMemberService>().InstancePerRequest();
            builder.RegisterType<ProjectOwnerService>().As<IProjectOwnerService>().InstancePerRequest();
            builder.RegisterType<ActivityService>().As<IActivityService>().InstancePerRequest();
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerRequest();

            builder.RegisterType<RoadmapService>().As<IRoadmapService>().InstancePerRequest();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerRequest();
            builder.RegisterType<UserGroupService>().As<IUserGroupService>().InstancePerRequest();
            builder.RegisterType<UserGroupMemberService>().As<IUserGroupMemberService>().InstancePerRequest();
            builder.RegisterType<PlantUserGroupMemberService>().As<IPlantUserGroupMemberService>().InstancePerRequest();

            builder.RegisterType<FieldService>().As<IFieldService>().InstancePerRequest(); 

            // ========================================================
            // Generic repository registration helper
            // ========================================================
            void RegisterRepository<TRepo, TInterface>(ContainerBuilder _builder)
                    where TRepo : TInterface
            {
                _builder.RegisterType<TRepo>()
                                        .As<TInterface>()
                                        .WithParameter(
                                            (pi, ctx) => pi.ParameterType == typeof(ILog),
                                            (pi, ctx) => LogManager.GetLogger(typeof(TRepo))
                                        )
                                        .InstancePerRequest();
            }



            // ========================================================
            // Repositories
            // ========================================================
            RegisterRepository<ActiveDirectoryGroupRepository, IActiveDirectoryGroupRepository>(builder);
            RegisterRepository<AnnotationTypeRepository, IAnnotationTypeRepository>(builder);
            RegisterRepository<CategoryRepository, ICategoryRepository>(builder);
            RegisterRepository<FormRepository, IFormRepository>(builder);
            RegisterRepository<FormFieldRepository, IFormFieldRepository>(builder);
            RegisterRepository<FormFieldOptionRepository, IFormFieldOptionRepository>(builder);
            RegisterRepository<FormFieldRuleRepository, IFormFieldRuleRepository>(builder);
            RegisterRepository<FormEntityLinkRepository, IFormEntityLinkRepository>(builder);
            RegisterRepository<MaturityLevelRepository, IMaturityLevelRepository>(builder);
            RegisterRepository<ModuleRepository, IModuleRepository>(builder);
            RegisterRepository<PlantRepository, IPlantRepository>(builder);
            RegisterRepository<PlantCategoryMilestoneRepository, IPlantCategoryMilestoneRepository>(builder);
            RegisterRepository<PlantFieldRepository, IPlantFieldRepository>(builder);
            RegisterRepository<PlantMemberRepository, IPlantMemberRepository>(builder);
            RegisterRepository<PlantUserGroupMemberRepository, IPlantUserGroupMemberRepository>(builder);
            RegisterRepository<PlantRoadmapLinkRepository, IPlantRoadmapLinkRepository>(builder);
            RegisterRepository<ProductRepository, IProductRepository>(builder);
            RegisterRepository<ProductDivisionRepository, IProductDivisionRepository>(builder);
            RegisterRepository<ProductGroupRepository, IProductGroupRepository>(builder);
            RegisterRepository<ProductionCalendarRepository, IProductionCalendarRepository>(builder);
            RegisterRepository<ProjectRepository, IProjectRepository>(builder);
            RegisterRepository<ProjectAttachmentRepository, IProjectAttachmentRepository>(builder);
            RegisterRepository<ProjectCommentRepository, IProjectCommentRepository>(builder);
            RegisterRepository<ProjectChatRepository, IProjectChatRepository>(builder);
            //RegisterRepository<ProjectNotificationRepository, IProjectNotificationRepository>(builder);
            RegisterRepository<UserGroupAccessRightRepository, IUserGroupAccessRightRepository>(builder);
            RegisterRepository<ActivityRepository, IActivityRepository>(builder);
            RegisterRepository<NotificationRepository, INotificationRepository>(builder);


            RegisterRepository<ProjectFormSubmissionRepository, IProjectFormSubmissionRepository>(builder);
            RegisterRepository<ProjectFormSubmissionValueRepository, IProjectFormSubmissionValueRepository>(builder);
            RegisterRepository<ProjectMemberRepository, IProjectMemberRepository>(builder);
            RegisterRepository<ProjectMilestoneRepository, IProjectMilestoneRepository>(builder);
            RegisterRepository<ProjectOwnerRepository, IProjectOwnerRepository>(builder);
            RegisterRepository<ProjectStatusChangeRepository, IProjectStatusChangeRepository>(builder);
            RegisterRepository<ProjectTaskRepository, IProjectTaskRepository>(builder);
            RegisterRepository<ProjectProductRepository, IProjectProductRepository>(builder);
            RegisterRepository<ProjectFormSubmissionRepository, IProjectFormSubmissionRepository>(builder);
            RegisterRepository<ProjectTargetRevisionRepository, IProjectTargetRevisionRepository>(builder); 
            RegisterRepository<RoadmapRepository, IRoadmapRepository>(builder);
            RegisterRepository<RoadmapMilestoneRepository, IRoadmapMilestoneRepository>(builder);
            RegisterRepository<RoadmapActivityRepository, IRoadmapActivityRepository>(builder);
            RegisterRepository<RoadmapActivityPrerequisiteRepository, IRoadmapActivityPrerequisiteRepository>(builder);


            RegisterRepository<FieldRepository, IFieldRepository>(builder);
            RegisterRepository<FieldOptionRepository, IFieldOptionRepository>(builder);
            RegisterRepository<FieldRuleRepository, IFieldRuleRepository>(builder);


            RegisterRepository<StatusChangeRepository, IStatusChangeRepository>(builder);

            RegisterRepository<TargetRevisionRepository, ITargetRevisionRepository>(builder);


            //////RegisterRepository<TaskRepository, ITaskRepository>(builder);
            //////RegisterRepository<TaskMemberRepository, ITaskMemberRepository>(builder);
            //////RegisterRepository<TaskPrerequisiteRepository, ITaskPrerequisiteRepository>(builder);
            RegisterRepository<UserRepository, IUserRepository>(builder);
            RegisterRepository<UserGroupRepository, IUserGroupRepository>(builder);
            RegisterRepository<UserGroupMemberRepository, IUserGroupMemberRepository>(builder);
            //RegisterRepository<WorkItemRepository, IWorkItemRepository>(builder);
            //RegisterRepository<WorkItemMemberRepository, IWorkItemMemberRepository>(builder);
            //RegisterRepository<WorkItemPrerequisiteRepository, IWorkItemPrerequisiteRepository>(builder);

            RegisterRepository<ProjectFieldRepository, IProjectFieldRepository>(builder);








            // Event subscribers
            ////builder.RegisterType<ProjectEventSubscribersService>()
            ////       .As<IEventSubscriber<ProjectCreatedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectCanceledEventArgs>>()
            ////       .As<IEventSubscriber<ProjectCompletedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectFailedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectHoldEventArgs>>()
            ////       .As<IEventSubscriber<ProjectStartedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectNotStartedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectResumedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectUpdatedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectDeletedEventArgs>>()
            ////       .As<IEventSubscriber<ProjectPromotedEventArgs>>()
            ////       .InstancePerRequest(); // important: same request scope as repositories

            //builder.RegisterType<PlantEventSubscribersService>()
            //       .As<IEventSubscriber<PlantMemberRegisteredEventArgs>>()
            //       .InstancePerDependency();


            // ========================================================
            // Filters
            // ========================================================
            //builder.RegisterType<RequirePlantCodeExistsAttribute>().PropertiesAutowired();

            // Register other dependencies
            //builder.RegisterType<EventBus>().As<IEventPublisher>().InstancePerRequest();
            //builder.RegisterType<EventBus>().As<IEventPublisher>().SingleInstance();

            //builder.RegisterType<SmtpEmailSenderService>().As<IEmailSender>();

            //     builder.RegisterType<PlantMemberRegisteredSubscriber>()
            //.As<IEventSubscriber<PlantMemberRegisteredEventArgs>>()
            //.InstancePerDependency();



            // ========================================================
            // Events / EventBus
            // ======================================================== 
            ////builder.RegisterModule<EventBusModule>();
            builder.RegisterType<EventBus>()
                .As<IEventPublisher>()
                .InstancePerRequest();

            builder.RegisterType<EmailNotificationService>()
                .As<IEventSubscriber<ProjectCreatedEventArgs>>()
                .As<IEventSubscriber<ProjectHoldEventArgs>>()
                .As<IEventSubscriber<ProjectCanceledEventArgs>>()
                .As<IEventSubscriber<ProjectResumedEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneNotStartedEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneStartedEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneHoldEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneResumedEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneCanceledEventArgs>>()
                .As<IEventSubscriber<ProjectMilestoneCompletedEventArgs>>()
                .As<IEventSubscriber<ProjectTaskNotStartedEventArgs>>()
                .As<IEventSubscriber<ProjectTaskStartedEventArgs>>()
                .As<IEventSubscriber<ProjectTaskHoldEventArgs>>()
                .As<IEventSubscriber<ProjectTaskResumedEventArgs>>()
                .As<IEventSubscriber<ProjectTaskCanceledEventArgs>>()
                .As<IEventSubscriber<ProjectTaskCompletedEventArgs>>()
                .InstancePerRequest();



            // ========================================================
            // Build the container and set Web API resolver
            // ========================================================
            var container = builder.Build();



            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
 
            //SubscribeEventHandlers(container);
        }
        ////public static void SubscribeEventHandlers(IContainer container)
        ////{
        ////    using (var scope = container.BeginLifetimeScope())
        ////    {
        ////        var eventBus = scope.Resolve<IEventPublisher>();

        ////        // Resolve and subscribe the same instance for multiple event types
        ////        var projectEventsSubscriber = scope.Resolve<ProjectEventSubscribersService>();

        ////        eventBus.Subscribe<ProjectCreatedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectStartedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectCanceledEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectCompletedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectFailedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectHoldEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectNotStartedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectResumedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectUpdatedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectDeletedEventArgs>(projectEventsSubscriber);
        ////        eventBus.Subscribe<ProjectPromotedEventArgs>(projectEventsSubscriber);
        ////    }
        ////}

    }


}