using log4net;
using Pulse.Core.Entities;
using Pulse.Core.Interfaces;
using Pulse.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Infrastructure.Repositories
{
    public class ProjectRepository : BaseRepository<Project, string>, IProjectRepository
    {
        public ProjectRepository(OracleDataAccessLayer dataAccess, ILog logger) : base(dataAccess, logger) { }

        private static List<ProjectExtend> DeduplicateProjectNodeResults(IEnumerable<ProjectExtend> nodes)
        {
            return (nodes ?? Enumerable.Empty<ProjectExtend>())
                .Where(node => node != null)
                .GroupBy(node => new
                {
                    ProjectNo = (node.ProjectNo ?? string.Empty).Trim().ToUpperInvariant(),
                    NodeType = (node.NodeType ?? string.Empty).Trim().ToUpperInvariant(),
                    NodeId = (node.NodeId ?? string.Empty).Trim().ToUpperInvariant(),
                    ProjectNodeSysId = (node.ProjectNodeSysId ?? string.Empty).Trim().ToUpperInvariant()
                })
                .Select(group => group
                    .OrderByDescending(node => !string.IsNullOrWhiteSpace(node.ProjectNodeSysId))
                    .ThenBy(node => node.OrderIndex)
                    .First())
                .OrderBy(node => node.OrderIndex)
                .ToList();
        }



          string sql_flat_det = @"
WITH
calendar_range AS (
    SELECT
        calendaryear,
        calendarworkweek,
        MIN(fiscaldate) AS week_start_date,
        MAX(fiscaldate) AS week_end_date
    FROM productioncalendars
    GROUP BY calendaryear, calendarworkweek
), 

accessible_projects AS (SELECT projectno
                              FROM PROJECTOWNERS
                             WHERE :loggeduser IS NULL OR userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTMEMBERS
                             WHERE  :loggeduser IS NULL OR userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTS prj
                             WHERE EXISTS
                                      (SELECT plantcode
                                         FROM plantmembers pm
                                        WHERE (:loggeduser IS NULL OR userid = :loggeduser) AND prj.plantcode = pm.plantcode))
                                   ,

project_list AS (
    SELECT
           prj.projectno,
           prj.roadmapsysid,
           prj.projectname,
           prj.projectdescription,
           prj.projecticon,
           prj.projecticoncolor,
           prj.productgroupcode,
           prj.productdivisioncode,
           prj.plantcode,
           prj.categorycode,
           prj.projectownerid, 
           prj.projectmaturitycode,
           prj.status,
           prj.targetstartedby,
           prj.targetstartyear,
           prj.targetstartworkweek, 
           prj.targetstartdate,
           prj.targetcompletedby,
           prj.targetcompletionyear,
           prj.targetcompletionworkweek, 
           prj.targetcompletiondate, 
           prj.actualstartdate,
           prj.actualstartedby,
           prj.actualcompletiondate,
           prj.actualcompletedby, 
           prj.isresched, 
           prj.createdby,
           prj.createdDate,
           prj.modifiedby,
           prj.modifiedDate ,
           prj.transactionkey
    FROM projects prj
    WHERE (:search IS NULL
           OR LOWER(prj.projectname) LIKE LOWER('%' || :search || '%')
           OR LOWER(prj.projectno)   LIKE LOWER('%' || :search || '%'))
    AND (:productgroupcode IS NULL
           OR prj.projectname = :productgroupcode)
    AND (:productdivisioncode IS NULL
           OR prj.productdivisioncode = :productdivisioncode)  
    AND (:plantcode IS NULL
           OR prj.plantcode = :plantcode) 
    AND (:categorycode IS NULL
           OR prj.categorycode = :categorycode) 
    AND (:projectownerid IS NULL
           OR prj.projectownerid = :projectownerid)  
    AND (:status IS NULL
           OR :status like '%'||prj.status||'%')  
    AND (:loggeduser IS NULL
           OR EXISTS (
                 SELECT 1
                 FROM accessible_projects ap
                 WHERE ap.projectno = prj.projectno
               ))
), 



node_value AS (
    SELECT 
        pm.milestonesysid          AS nodesysid,
        nvl2(pm.roadmapmilestonesysid, 'milestone','rootactivity')   AS nodetype,
        pm.projectno,
        nvl(pm.roadmapmilestonesysid,'__ROOTACTIVITY__')   AS structuresysid,
        pm.status,
        pm.remarks, 
        pm.targetstartedby,
        pm.targetstartyear,
        pm.targetstartworkweek,
        cs.week_start_date         AS targetstart,
        pm.targetstartdate,
        pm.targetcompletedby,
        pm.targetcompletionyear,
        pm.targetcompletionworkweek,
        ce.week_end_date           AS targetcompletion,
        pm.targetcompletiondate,
        pm.actualstartdate,
        pm.actualstartedby,
        pm.actualcompletiondate,
        pm.actualcompletedby,
        pm.isresched, 
        pm.createdby,
        pm.createddate,
        pm.modifiedby,
        pm.modifieddate,
        pm.transactionkey 
    FROM projectmilestones pm
    INNER JOIN project_list pl
        ON pl.projectno = pm.projectno
    LEFT JOIN calendar_range cs
           ON cs.calendaryear     = pm.targetstartyear
          AND cs.calendarworkweek = pm.targetstartworkweek
    LEFT JOIN calendar_range ce
           ON ce.calendaryear     = pm.targetcompletionyear
          AND ce.calendarworkweek = pm.targetcompletionworkweek
 
    UNION ALL

    SELECT 
        pt.projecttasksysid        AS nodesysid,
        'task' AS nodetype,
        pt.projectno,
        NVL(pt.roadmapactivitysysid, pt.projecttasksysid) AS structuresysid,
        pt.status,
        pt.remarks,
 
        pt.targetstartedby,
        pt.targetstartyear,
        pt.targetstartworkweek,
        cs.week_start_date         AS targetstart,
        pt.targetstartdate,
        pt.targetcompletedby,
        pt.targetcompletionyear,
        pt.targetcompletionworkweek, 
        ce.week_end_date           AS targetcompletion,
        pt.targetcompletiondate,
        pt.actualstartdate,
        pt.actualstartedby,
        pt.actualcompletiondate,
        pt.actualcompletedby,
        pt.isresched, 
        pt.createdby,
        pt.createddate,
        pt.modifiedby,
        pt.modifieddate,
        pt.transactionkey
    FROM projecttasks pt
    INNER JOIN project_list pl
        ON pl.projectno = pt.projectno    
    LEFT JOIN calendar_range cs
           ON cs.calendaryear     = pt.targetstartyear
          AND cs.calendarworkweek = pt.targetstartworkweek
    LEFT JOIN calendar_range ce
           ON ce.calendaryear     = pt.targetcompletionyear
          AND ce.calendarworkweek = pt.targetcompletionworkweek
 
)
,

project_productcodes AS (
    SELECT 
        pp.projectno,
        LISTAGG(DISTINCT pp.productcode, ', ')
            WITHIN GROUP (ORDER BY pp.productcode) AS productcodes
    FROM PROJECTPRODUCTS pp 
    JOIN project_list pl
      ON pp.projectno = pl.projectno 
    GROUP BY pp.projectno
), 

project_owner AS (
    SELECT 
        pl.projectno,
        pl.projectownerid,
        u.username  AS projectownerusername,
        u.firstname AS projectownerfirstname,
        u.lastname  AS projectownerlastname,
        u.email     AS projectowneremail,
        JSON_OBJECT(
            'projectno' VALUE pl.projectno,
            'userid'    VALUE pl.projectownerid,
            'username'  VALUE u.username,
            'firstname' VALUE u.firstname,
            'lastname'  VALUE u.lastname,
            'email'     VALUE u.email
        ) AS ownerinfo
    FROM project_list pl 
    JOIN users u
      ON u.userid = pl.projectownerid 
), 

project_members AS (
    SELECT 
        pm.projectno,
        JSON_ARRAYAGG(
            JSON_OBJECT(
                'projectno' VALUE pm.projectno,
                'userid'    VALUE u.userid,
                'username'  VALUE u.username,
                'firstname' VALUE u.firstname,
                'lastname'  VALUE u.lastname,
                'email'     VALUE u.email,
                'isowner'     VALUE pm.isowner
            )
        ) AS members 
    FROM projectmembers pm
    INNER JOIN project_list pl
        ON pl.projectno = pm.projectno
    JOIN users u
      ON u.userid = pm.userid
    GROUP BY pm.projectno
),

node_owners AS (
    SELECT 
        po.projectno,
        po.parenttype nodetype,
        po.parentsysid nodesysid,
        JSON_ARRAYAGG(
            JSON_OBJECT(
                'projectno' VALUE po.projectno,
                'userid'    VALUE u.userid,
                'username'  VALUE u.username,
                'firstname' VALUE u.firstname,
                'lastname'  VALUE u.lastname,
                'email'     VALUE u.email
            )
        ) AS nodeowners
    FROM projectowners po
    INNER JOIN project_list pl
        ON pl.projectno = po.projectno
    JOIN users u
      ON u.userid = po.userid
    GROUP BY po.projectno, po.parenttype, po.parentsysid
), 
roadmap_tree AS (
    SELECT
        rm.projectno,
        rm.roadmapsysid           AS nodeid,
        rm.roadmapsysid           AS nodekey,
        rm.roadmapsysid,
        NULL                      AS parenttype,
        NULL                      AS parentsysid,
        NULL                      AS datamaturitycode,
        rm.roadmapname            AS dataname,
        rm.roadmapdescription     AS datadescription,
        0                         AS datamandays,
        '1'                       AS dataisrequired,
        -1                        AS orderindex,
        rm.isactive,
        rm.transactionkey,
        'roadmap'                 AS nodetype 
    FROM projectroadmaps rm
    INNER JOIN project_list pl
        ON pl.roadmapsysid = rm.roadmapsysid
        AND pl.projectno = rm.projectno

    UNION ALL

    SELECT
        rm.projectno,
        '__ROOTACTIVITY__'        AS nodeid,
        '__ROOTACTIVITY__'        AS nodekey,
        rm.roadmapsysid,
        'roadmap'                 AS parenttype,
        rm.roadmapsysid           AS parentsysid,
        NULL                      AS datamaturitycode,
        'Root Activity'           AS dataname,
        'Root Activity'           AS datadescription,
        0                         AS datamandays,
        '1'                       AS dataisrequired,
        0                         AS orderindex,
        rm.isactive,
        rm.transactionkey,
        'rootactivity'            AS nodetype 
    FROM projectroadmaps rm
    INNER JOIN project_list pl
        ON pl.roadmapsysid = rm.roadmapsysid
        AND pl.projectno = rm.projectno

    UNION ALL

    SELECT
        act.projectno,
        act.roadmapactivitysysid  AS nodeid,
        act.roadmapactivitysysid  AS nodekey,
        act.roadmapsysid,
        DECODE(act.parenttype, 'roadmap', 'rootactivity', act.parenttype)      AS parenttype,
        DECODE(act.parenttype, 'roadmap', '__ROOTACTIVITY__', act.parentsysid) AS parentsysid,
        NULL                      AS datamaturitycode,
        act.activityname          AS dataname,
        act.activitydescription   AS datadescription,
        act.estimatedmandays      AS datamandays,
        act.isrequired            AS dataisrequired,
        act.orderindex,
        act.isactive,
        act.transactionkey,
        'activity'                AS nodetype 
    FROM projectroadmapactivities act
    INNER JOIN project_list pl
        ON pl.projectno = act.projectno
    UNION ALL

    SELECT
        pt.projectno,
        pt.projecttasksysid       AS nodeid,
        pt.projecttasksysid       AS nodekey,
        pt.roadmapsysid,
        CASE
            WHEN LOWER(NVL(pt.parenttype, 'milestone')) = 'roadmap' OR pt.parentsysid = '__ROOTACTIVITY__' THEN 'rootactivity'
            ELSE LOWER(NVL(pt.parenttype, 'milestone'))
        END AS parenttype,
        CASE
            WHEN LOWER(NVL(pt.parenttype, 'milestone')) = 'roadmap' OR pt.parentsysid = '__ROOTACTIVITY__' THEN '__ROOTACTIVITY__'
            ELSE pt.parentsysid
        END AS parentsysid,
        NULL                      AS datamaturitycode,
        NVL(pt.alttaskname, 'Untitled Task') AS dataname,
        pt.alttaskdescription     AS datadescription,
        pt.estimatedmandays       AS datamandays,
        pt.isrequired             AS dataisrequired,
        pt.orderindex,
        pt.isactive,
        pt.transactionkey,
        'activity'                AS nodetype
    FROM projecttasks pt
    INNER JOIN project_list pl
        ON pl.projectno = pt.projectno
    WHERE pt.roadmapactivitysysid IS NULL
    UNION ALL

    SELECT
        mil.projectno,
        mil.roadmapmilestonesysid AS nodeid,
        mil.roadmapmilestonesysid AS nodekey,
        mil.roadmapsysid,
        mil.parenttype,
        mil.parentsysid,
        mil.maturitycode          AS datamaturitycode,
        mil.milestonealias        AS dataname,
        mil.milestonedescription  AS datadescription,
        NULL                      AS datamandays,
        NULL                      AS dataisrequired,
        mil.orderindex + 1        AS orderindex,
        mil.isactive,
        mil.transactionkey,
        'milestone'               AS nodetype 
    FROM projectroadmapmilestones mil
    INNER JOIN project_list pl
        ON pl.projectno = mil.projectno
),

hier AS (
    SELECT
        LEVEL                                AS lvl,
        SYS_CONNECT_BY_PATH(dataname, ' / ') AS full_path,
        projectno,
        nodeid,
        nodetype,
        parenttype,
        parentsysid,
        roadmapsysid,
        dataname,
        datadescription,
        datamandays,
        dataisrequired,
        orderindex
    FROM roadmap_tree
    START WITH ParentType IS NULL
       AND parentsysid IS NULL
    CONNECT BY PRIOR nodetype     = parenttype
              AND PRIOR nodeid       = parentsysid
                  AND PRIOR projectno    = projectno
)  

,

rel (
    projectno,
    ancestor_id,
    ancestor_type,
    descendant_id,
    descendant_type,
    descendant_roadmap
) AS (
    SELECT
        projectno,
        nodeid       AS ancestor_id,
        nodetype     AS ancestor_type,
        nodeid       AS descendant_id,
        nodetype     AS descendant_type,
        roadmapsysid AS descendant_roadmap
    FROM roadmap_tree

    UNION ALL

    SELECT
        r.projectno,
        r.ancestor_id,
        r.ancestor_type,
        c.nodeid       AS descendant_id,
        c.nodetype     AS descendant_type,
        c.roadmapsysid AS descendant_roadmap
    FROM rel r
    JOIN roadmap_tree c
            ON c.projectno    = r.projectno
         AND c.parenttype   = r.descendant_type
     AND c.parentsysid  = r.descendant_id
     AND c.roadmapsysid = r.descendant_roadmap
),

project_value_agg AS (
select pv.projectno,
        COUNT(*)   - 1 AS PROJECT_OVERALL_COUNT,
        SUM(CASE WHEN pv.status = 'COMPLETED'      THEN 1 ELSE 0 END) AS project_ovalue_complete_count,
        SUM(CASE WHEN pv.status = 'CANCELLED'   THEN 1 ELSE 0 END) AS project_ovalue_cancel_count,
        SUM(CASE WHEN pv.status = 'ONGOING'     THEN 1 ELSE 0 END) AS project_ovalue_ongoing_count,
        SUM(CASE WHEN pv.status = 'NOT STARTED' THEN 1 ELSE 0 END) AS project_ovalue_pending_count
from   node_value pv 
group by pv.projectno
) ,

project_task_value_agg AS (
select pt.projectno,
    SUM(CASE WHEN NVL(NULLIF(TRIM(pt.status), ''), 'NOT STARTED') = 'NOT STARTED' THEN 1 ELSE 0 END) AS project_task_pending_count,
    SUM(CASE WHEN NVL(NULLIF(TRIM(pt.status), ''), 'NOT STARTED') = 'ONGOING'
              AND NVL(pt.targetcompletiondate, ct.week_end_date) IS NOT NULL
              AND TRUNC(NVL(pt.targetcompletiondate, ct.week_end_date)) < TRUNC(SYSDATE)
         THEN 1 ELSE 0 END) AS project_task_at_risk_count,
    SUM(CASE WHEN NVL(NULLIF(TRIM(pt.status), ''), 'NOT STARTED') IN ('COMPLETED', 'CANCELLED') THEN 1 ELSE 0 END) AS project_task_closed_count,
    SUM(CASE WHEN NVL(NULLIF(TRIM(pt.status), ''), 'NOT STARTED') = 'COMPLETED'
              AND NVL(pt.targetcompletiondate, ct.week_end_date) IS NOT NULL
              AND TRUNC(NVL(pt.actualcompletiondate, pt.modifieddate)) > TRUNC(NVL(pt.targetcompletiondate, ct.week_end_date))
         THEN 1 ELSE 0 END) AS project_task_closed_delayed_count
from   projecttasks pt
       left join calendar_range ct
     on ct.calendaryear = pt.targetcompletionyear
    and ct.calendarworkweek = pt.targetcompletionworkweek
group by pt.projectno
) ,
 

value_agg AS (
    SELECT
        r.projectno,
        r.ancestor_id        AS nodeid,
        r.ancestor_type      AS nodetype,
        r.descendant_roadmap AS roadmapsysid, 
        COUNT(pv.nodesysid) AS node_value_count,
        SUM(CASE WHEN pv.status = 'COMPLETED'      THEN 1 ELSE 0 END) AS node_value_complete_count,
        SUM(CASE WHEN pv.status = 'CANCELLED'   THEN 1 ELSE 0 END) AS node_value_cancel_count,
        SUM(CASE WHEN pv.status = 'ONGOING'     THEN 1 ELSE 0 END) AS node_value_ongoing_count,
        SUM(CASE WHEN pv.status = 'NOT STARTED' THEN 1 ELSE 0 END) AS node_value_pending_count
    FROM rel r
    LEFT JOIN node_value pv
      ON pv.structuresysid = r.descendant_id
         AND pv.projectno      = r.projectno
    GROUP BY
                r.projectno,
        r.ancestor_id,
        r.ancestor_type,
        r.descendant_roadmap
),



finalsummary AS (
    SELECT
        h.lvl,
        h.full_path,
        h.projectno,
        h.nodeid,
        h.nodetype,
        h.parenttype,
        h.parentsysid,
        h.roadmapsysid,
        h.orderindex,
        h.dataname,
        h.datadescription,
        h.datamandays,
        h.dataisrequired,
        COUNT(*) - 1 AS total_descendants 
    FROM hier h
    JOIN rel r
            ON r.projectno          = h.projectno
         AND r.ancestor_id        = h.nodeid
     AND r.ancestor_type      = h.nodetype
     AND r.descendant_roadmap = h.roadmapsysid
    GROUP BY
        h.lvl,
        h.full_path,
        h.projectno,
        h.nodeid,
        h.nodetype,
        h.parenttype,
        h.parentsysid,
        h.roadmapsysid,
        h.orderindex,
        h.dataname,
        h.datadescription,
        h.datamandays,
        h.dataisrequired
),
 
flat_det AS (
    SELECT
        pl.projectno,
        pl.projectname,
        pl.projectdescription,
        pl.projecticon,
        pl.projecticoncolor,
        pc.productcodes,
        pm.members        AS jsonmembers,
        po.nodeowners     AS JsonNodeOwners,
        pl.projectmaturitycode,
        pl.status,
        pl.targetstartedby,
        pl.targetstartyear,
        pl.targetstartworkweek,
        cs.week_start_date AS targetstart,
        pl.targetstartdate,
        pl.targetcompletedby,
        pl.targetcompletionyear,
        pl.targetcompletionworkweek,
        ce.week_end_date  AS targetcompletion,
        pl.targetcompletiondate,
        pl.actualstartdate,
        pl.actualstartedby,
        pl.actualcompletiondate,
        pl.actualcompletedby,
        pl.isresched,
        
        fsum.dataname     AS nodename,
        fsum.datadescription AS nodedescription,
        fsum.lvl          AS nodelevel,
        fsum.full_path    AS nodefullpath,
        fsum.nodeid,
        fsum.nodetype,
        fsum.parenttype,
        fsum.parentsysid,
        fsum.roadmapsysid,
        fsum.datamandays  AS estimatedmandays,
        fsum.dataisrequired AS isrequired,
        
        JSON_OBJECT(
            'prerequisites' VALUE (
                SELECT JSON_ARRAYAGG(
                    JSON_OBJECT(
                        'id' VALUE pr.prerequisitesysid,
                        'name' VALUE fsm.dataname,
                        'path' VALUE fsm.full_path,
                        'status' VALUE NVL(nv.status,'NOT STARTED')
                    RETURNING CLOB) 
                RETURNING CLOB)
                FROM PROJECTROADMAPACTIVITYPREREQS pr 
                    INNER JOIN finalsummary fsm ON fsm.projectno = pl.projectno
                        AND fsm.nodeid = pr.prerequisitesysid  
                    LEFT OUTER JOIN node_value nv ON nv.projectno = pl.projectno
                        AND nv.structuresysid = pr.prerequisitesysid 
                WHERE pr.roadmapactivitysysid = fsum.nodeid
                AND pr.projectno = pl.projectno
            )
        RETURNING CLOB
        ) AS PrerequisitesJson,    
        
        fsum.orderindex,
        fsum.total_descendants AS nodetotaldescendants,
        
        NVL(pva.PROJECT_OVERALL_COUNT, 0 )         AS projectcount,
        NVL(pva.project_ovalue_complete_count, 0)  AS projectcompletecount,
        NVL(pva.project_ovalue_cancel_count, 0)    AS projectcancelcount,
        NVL(pva.project_ovalue_ongoing_count, 0)   AS projectongoingcount,
        NVL(pva.project_ovalue_pending_count, 0)   AS projectpendingcount,
        NVL(pta.project_task_pending_count, 0)     AS projecttaskpendingcount,
        NVL(pta.project_task_at_risk_count, 0)     AS projecttaskatriskcount,
        NVL(pta.project_task_closed_count, 0)      AS projecttaskclosedcount,
        NVL(pta.project_task_closed_delayed_count, 0) AS projecttaskcloseddelayedcount,
        NVL(va.node_value_count - DECODE(fsum.nodeid,'__ROOTACTIVITY__',1,0), 0)             AS projectnodecount,
        NVL(va.node_value_complete_count, 0)    AS projectnodecompletecount,
        NVL(va.node_value_cancel_count, 0)      AS projectnodecancelcount,
        NVL(va.node_value_ongoing_count, 0)     AS projectnodeongoingcount,
        NVL(va.node_value_pending_count, 0)     AS projectnodependingcount,
        
        pv.nodesysid            AS projectnodesysid,
        pv.status               AS projectnodestatus,
        pv.remarks              AS projectnoderemarks,
        pv.targetstartedby      AS projectnodetargetstartedby,
        pv.targetstartyear      AS projectnodetargetstartyear,
        pv.targetstartworkweek  AS projectnodetargetstartworkweek,
        pv.targetstart          AS projectnodetargetstart,
        pv.targetstartdate          AS projectnodetargetstartdate,
        pv.targetcompletedby    AS projectnodetargetcompletedby,
        pv.targetcompletionyear AS projectnodetargetcompletionyear,
        pv.targetcompletionworkweek AS projectnodetargetcompletionworkweek,
        pv.targetcompletiondate     AS projectnodetargetcompletiondate,
        pv.targetcompletion     AS projectnodetargetcompletion,
        pv.actualstartdate      AS projectnodeactualstartdate,
        pv.actualstartedby      AS projectnodeactualstartedby,
        pv.actualcompletiondate AS projectnodeactualcompletiondate,
        pv.actualcompletedby    AS projectnodeactualcompletedby,
        pv.isresched    AS projectnodeisresched,
        DECODE(fsum.nodetype,'roadmap', pl.createdby, pv.createdby) createdby,
        DECODE(fsum.nodetype,'roadmap', pl.createddate, pv.createddate) createddate,
        DECODE(fsum.nodetype,'roadmap', pl.modifiedby, pv.modifiedby) modifiedby,
        DECODE(fsum.nodetype,'roadmap', pl.modifieddate, pv.modifieddate) modifieddate,
        DECODE(fsum.nodetype,'roadmap', pl.transactionkey, pv.transactionkey) transactionkey, 
        
        pl.projectownerid,
        projectownerusername,
        projectownerfirstname,
        projectownerlastname,
        projectowneremail,
        
        pl.productgroupcode,
        pg.productgroupname,
        pl.productdivisioncode,
        pd.productdivisionname,
        pl.plantcode,
        pln.plantname,
        pl.categorycode,
        ctg.categoryname
    FROM finalsummary fsum  
    INNER JOIN project_list pl
            ON pl.projectno = fsum.projectno
           AND pl.roadmapsysid = fsum.roadmapsysid 
    INNER JOIN project_owner pown
        ON pown.projectno     = pl.projectno  
       AND pown.projectownerid = pl.projectownerid
    INNER JOIN productgroups pg
        ON pg.productgroupcode = pl.productgroupcode  
    INNER JOIN productdivisions pd
        ON pd.productdivisioncode = pl.productdivisioncode      
    INNER JOIN plants pln
        ON pln.plantcode = pl.plantcode
    INNER JOIN categories ctg
        ON ctg.categorycode = pl.categorycode    
    LEFT JOIN project_members pm
        ON pm.projectno = pl.projectno
    LEFT JOIN node_value pv
        ON pv.projectno      = pl.projectno
       AND pv.structuresysid = fsum.nodeid
    LEFT JOIN node_owners po
        ON po.projectno  = pl.projectno 
       AND po.nodesysid = pv.nodesysid     
    LEFT JOIN value_agg va
        ON va.projectno    = pl.projectno
       AND va.nodeid       = fsum.nodeid
       AND UPPER(va.nodetype)     = UPPER(fsum.nodetype)
       AND va.roadmapsysid = fsum.roadmapsysid
    LEFT JOIN project_value_agg pva
        ON pva.projectno    = pl.projectno
    LEFT JOIN project_task_value_agg pta
        ON pta.projectno    = pl.projectno
    LEFT JOIN calendar_range cs
        ON cs.calendaryear     = pl.targetstartyear
       AND cs.calendarworkweek = pl.targetstartworkweek
    LEFT JOIN calendar_range ce
        ON ce.calendaryear     = pl.targetcompletionyear
       AND ce.calendarworkweek = pl.targetcompletionworkweek
    LEFT JOIN project_productcodes pc
        ON pc.projectno = pl.projectno 
WHERE (:status IS NULL OR UPPER(:status) like '%'||UPPER(pv.status)||'%')  
    AND (:parenttype IS NULL OR(UPPER(:parenttype) LIKE '%'||UPPER(NVL(fsum.parenttype,'PARENT'))||'%'))
    AND (:nodetype IS NULL OR(UPPER(:nodetype) LIKE '%'||UPPER(NVL(fsum.nodetype,'PARENT'))||'%'))
)  
 ";

        string sql_flat_det_child = @"
WITH
calendar_range AS (
    SELECT
        calendaryear,
        calendarworkweek,
        MIN(fiscaldate) AS week_start_date,
        MAX(fiscaldate) AS week_end_date
    FROM productioncalendars
    GROUP BY calendaryear, calendarworkweek
), 

accessible_projects AS (SELECT projectno
                              FROM PROJECTOWNERS
                             WHERE :loggeduser IS NULL OR userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTMEMBERS
                             WHERE  :loggeduser IS NULL OR userid = :loggeduser
                            UNION
                            SELECT projectno
                              FROM PROJECTS prj
                             WHERE EXISTS
                                      (SELECT plantcode
                                         FROM plantmembers pm
                                        WHERE (:loggeduser IS NULL OR userid = :loggeduser) AND prj.plantcode = pm.plantcode))
                                   ,

project_list AS (
    SELECT
           prj.projectno,
           prj.roadmapsysid,
           prj.projectname,
           prj.projectdescription,
           prj.projecticon,
           prj.projecticoncolor,
           prj.productgroupcode,
           prj.productdivisioncode,
           prj.plantcode,
           prj.categorycode,
           prj.projectownerid, 
           prj.projectmaturitycode,
           prj.status,
           prj.targetstartedby,
           prj.targetstartyear,
           prj.targetstartworkweek, 
           prj.targetstartdate,
           prj.targetcompletedby,
           prj.targetcompletionyear,
           prj.targetcompletionworkweek, 
           prj.targetcompletiondate, 
           prj.actualstartdate,
           prj.actualstartedby,
           prj.actualcompletiondate,
           prj.actualcompletedby, 
           prj.isresched, 
           prj.createdby,
           prj.createdDate,
           prj.modifiedby,
           prj.modifiedDate ,
           prj.transactionkey
    FROM projects prj
    WHERE (:search IS NULL
           OR LOWER(prj.projectname) LIKE LOWER('%' || :search || '%')
           OR LOWER(prj.projectno)   LIKE LOWER('%' || :search || '%'))
    AND (:productgroupcode IS NULL
           OR prj.projectname = :productgroupcode)
    AND (:productdivisioncode IS NULL
           OR prj.productdivisioncode = :productdivisioncode)  
    AND (:plantcode IS NULL
           OR prj.plantcode = :plantcode) 
    AND (:categorycode IS NULL
           OR prj.categorycode = :categorycode) 
    AND (:projectownerid IS NULL
           OR prj.projectownerid = :projectownerid)  
    AND (:status IS NULL
           OR :status like '%'||prj.status||'%')  
    AND (:loggeduser IS NULL
           OR EXISTS (
                 SELECT 1
                 FROM accessible_projects ap
                 WHERE ap.projectno = prj.projectno
               ))
), 



node_value AS (
    SELECT 
        pm.milestonesysid          AS nodesysid,
        nvl2(pm.roadmapmilestonesysid, 'milestone','rootactivity')   AS nodetype,
        pm.projectno,
        nvl(pm.roadmapmilestonesysid,'__ROOTACTIVITY__')   AS structuresysid,
        pm.status,
        pm.remarks, 
        pm.targetstartedby,
        pm.targetstartyear,
        pm.targetstartworkweek,
        cs.week_start_date         AS targetstart,
        pm.targetstartdate,
        pm.targetcompletedby,
        pm.targetcompletionyear,
        pm.targetcompletionworkweek,
        ce.week_end_date           AS targetcompletion,
        pm.targetcompletiondate,
        pm.actualstartdate,
        pm.actualstartedby,
        pm.actualcompletiondate,
        pm.actualcompletedby,
        pm.isresched, 
        pm.createdby,
        pm.createddate,
        pm.modifiedby,
        pm.modifieddate,
        pm.transactionkey 
    FROM projectmilestones pm
    INNER JOIN project_list pl
        ON pl.projectno = pm.projectno
    LEFT JOIN calendar_range cs
           ON cs.calendaryear     = pm.targetstartyear
          AND cs.calendarworkweek = pm.targetstartworkweek
    LEFT JOIN calendar_range ce
           ON ce.calendaryear     = pm.targetcompletionyear
          AND ce.calendarworkweek = pm.targetcompletionworkweek
 
    UNION ALL

    SELECT 
        pt.projecttasksysid        AS nodesysid,
        'task' AS nodetype,
        pt.projectno,
        NVL(pt.roadmapactivitysysid, pt.projecttasksysid) AS structuresysid,
        pt.status,
        pt.remarks,
 
        pt.targetstartedby,
        pt.targetstartyear,
        pt.targetstartworkweek,
        cs.week_start_date         AS targetstart,
        pt.targetstartdate,
        pt.targetcompletedby,
        pt.targetcompletionyear,
        pt.targetcompletionworkweek, 
        ce.week_end_date           AS targetcompletion,
        pt.targetcompletiondate,
        pt.actualstartdate,
        pt.actualstartedby,
        pt.actualcompletiondate,
        pt.actualcompletedby,
        pt.isresched, 
        pt.createdby,
        pt.createddate,
        pt.modifiedby,
        pt.modifieddate,
        pt.transactionkey
    FROM projecttasks pt
    INNER JOIN project_list pl
        ON pl.projectno = pt.projectno    
    LEFT JOIN calendar_range cs
           ON cs.calendaryear     = pt.targetstartyear
          AND cs.calendarworkweek = pt.targetstartworkweek
    LEFT JOIN calendar_range ce
           ON ce.calendaryear     = pt.targetcompletionyear
          AND ce.calendarworkweek = pt.targetcompletionworkweek
 
)
,

project_productcodes AS (
    SELECT 
        pp.projectno,
        LISTAGG(DISTINCT pp.productcode, ', ')
            WITHIN GROUP (ORDER BY pp.productcode) AS productcodes
    FROM PROJECTPRODUCTS pp 
    JOIN project_list pl
      ON pp.projectno = pl.projectno 
    GROUP BY pp.projectno
), 

project_owner AS (
    SELECT 
        pl.projectno,
        pl.projectownerid,
        u.username  AS projectownerusername,
        u.firstname AS projectownerfirstname,
        u.lastname  AS projectownerlastname,
        u.email     AS projectowneremail,
        JSON_OBJECT(
            'projectno' VALUE pl.projectno,
            'userid'    VALUE pl.projectownerid,
            'username'  VALUE u.username,
            'firstname' VALUE u.firstname,
            'lastname'  VALUE u.lastname,
            'email'     VALUE u.email
        ) AS ownerinfo
    FROM project_list pl 
    JOIN users u
      ON u.userid = pl.projectownerid 
), 

project_members AS (
    SELECT 
        pm.projectno,
        JSON_ARRAYAGG(
            JSON_OBJECT(
                'projectno' VALUE pm.projectno,
                'userid'    VALUE u.userid,
                'username'  VALUE u.username,
                'firstname' VALUE u.firstname,
                'lastname'  VALUE u.lastname,
                'email'     VALUE u.email,
                'isowner'   VALUE pm.isowner
            )
        ) AS members
    FROM projectmembers pm
    INNER JOIN project_list pl
        ON pl.projectno = pm.projectno
    JOIN users u
      ON u.userid = pm.userid
    GROUP BY pm.projectno
),

node_owners AS (
    SELECT 
        po.projectno,
        po.parenttype nodetype,
        po.parentsysid nodesysid,
        JSON_ARRAYAGG(
            JSON_OBJECT(
                'projectno' VALUE po.projectno,
                'userid'    VALUE u.userid,
                'username'  VALUE u.username,
                'firstname' VALUE u.firstname,
                'lastname'  VALUE u.lastname,
                'email'     VALUE u.email
            )
        ) AS nodeowners
    FROM projectowners po
    INNER JOIN project_list pl
        ON pl.projectno = po.projectno
    JOIN users u
      ON u.userid = po.userid
    GROUP BY po.projectno, po.parenttype, po.parentsysid
), 
roadmap_tree AS (
    SELECT
        rm.projectno,
        rm.roadmapsysid           AS nodeid,
        rm.roadmapsysid           AS nodekey,
        rm.roadmapsysid,
        NULL                      AS parenttype,
        NULL                      AS parentsysid,
        NULL                      AS datamaturitycode,
        rm.roadmapname            AS dataname,
        rm.roadmapdescription     AS datadescription,
        0                         AS datamandays,
        '1'                       AS dataisrequired,
        -1                        AS orderindex,
        rm.isactive,
        rm.transactionkey,
        'roadmap'                 AS nodetype 
    FROM projectroadmaps rm
    INNER JOIN project_list pl
        ON pl.roadmapsysid = rm.roadmapsysid
        AND pl.projectno = rm.projectno
    UNION ALL

    SELECT
        rm.projectno,
        '__ROOTACTIVITY__'        AS nodeid,
        '__ROOTACTIVITY__'        AS nodekey,
        rm.roadmapsysid,
        'roadmap'                 AS parenttype,
        rm.roadmapsysid           AS parentsysid,
        NULL                      AS datamaturitycode,
        'Root Activity'           AS dataname,
        'Root Activity'           AS datadescription,
        0                         AS datamandays,
        '1'                       AS dataisrequired,
        0                         AS orderindex,
        rm.isactive,
        rm.transactionkey,
        'rootactivity'            AS nodetype 
    FROM projectroadmaps rm
    INNER JOIN project_list pl
        ON pl.roadmapsysid = rm.roadmapsysid
        AND pl.projectno = rm.projectno
    UNION ALL

    SELECT
        act.projectno,
        act.roadmapactivitysysid  AS nodeid,
        act.roadmapactivitysysid  AS nodekey,
        act.roadmapsysid,
        DECODE(act.parenttype, 'roadmap', 'rootactivity', act.parenttype)      AS parenttype,
        DECODE(act.parenttype, 'roadmap', '__ROOTACTIVITY__', act.parentsysid) AS parentsysid,
        NULL                      AS datamaturitycode,
        act.activityname          AS dataname,
        act.activitydescription   AS datadescription,
        act.estimatedmandays      AS datamandays,
        act.isrequired            AS dataisrequired,
        act.orderindex,
        act.isactive,
        act.transactionkey,
        'activity'                AS nodetype 
    FROM projectroadmapactivities act
    INNER JOIN project_list pl
        ON pl.projectno = act.projectno
    UNION ALL

    SELECT
        pt.projectno,
        pt.projecttasksysid       AS nodeid,
        pt.projecttasksysid       AS nodekey,
        pt.roadmapsysid,
        CASE
            WHEN LOWER(NVL(pt.parenttype, 'milestone')) = 'roadmap' OR pt.parentsysid = '__ROOTACTIVITY__' THEN 'rootactivity'
            ELSE LOWER(NVL(pt.parenttype, 'milestone'))
        END AS parenttype,
        CASE
            WHEN LOWER(NVL(pt.parenttype, 'milestone')) = 'roadmap' OR pt.parentsysid = '__ROOTACTIVITY__' THEN '__ROOTACTIVITY__'
            ELSE pt.parentsysid
        END AS parentsysid,
        NULL                      AS datamaturitycode,
        NVL(pt.alttaskname, 'Untitled Task') AS dataname,
        pt.alttaskdescription     AS datadescription,
        pt.estimatedmandays       AS datamandays,
        pt.isrequired             AS dataisrequired,
        pt.orderindex,
        pt.isactive,
        pt.transactionkey,
        'activity'                AS nodetype
    FROM projecttasks pt
    INNER JOIN project_list pl
        ON pl.projectno = pt.projectno
    WHERE pt.roadmapactivitysysid IS NULL
    UNION ALL

    SELECT
        mil.projectno,
        mil.roadmapmilestonesysid AS nodeid,
        mil.roadmapmilestonesysid AS nodekey,
        mil.roadmapsysid,
        mil.parenttype,
        mil.parentsysid,
        mil.maturitycode          AS datamaturitycode,
        mil.milestonealias        AS dataname,
        mil.milestonedescription  AS datadescription,
        NULL                      AS datamandays,
        NULL                      AS dataisrequired,
        mil.orderindex + 1        AS orderindex,
        mil.isactive,
        mil.transactionkey,
        'milestone'               AS nodetype 
    FROM projectroadmapmilestones mil
    INNER JOIN project_list pl
        ON pl.projectno = mil.projectno
),

hier AS (
    SELECT
        LEVEL                                AS lvl,
        SYS_CONNECT_BY_PATH(dataname, ' / ') AS full_path,
        projectno,
        nodeid,
        nodetype,
        parenttype,
        parentsysid,
        roadmapsysid,
        dataname,
        datadescription,
        datamandays,
        dataisrequired,
        orderindex
    FROM roadmap_tree
    START WITH  lower(parenttype) = lower(:ParentType)
       AND  parentsysid = :ParentSysId
    CONNECT BY PRIOR nodetype     = parenttype
              AND PRIOR nodeid       = parentsysid
                  AND PRIOR projectno    = projectno
)  

,

rel (
    projectno,
    ancestor_id,
    ancestor_type,
    descendant_id,
    descendant_type,
    descendant_roadmap
) AS (
    SELECT
        projectno,
        nodeid       AS ancestor_id,
        nodetype     AS ancestor_type,
        nodeid       AS descendant_id,
        nodetype     AS descendant_type,
        roadmapsysid AS descendant_roadmap
    FROM roadmap_tree

    UNION ALL

    SELECT
        r.projectno,
        r.ancestor_id,
        r.ancestor_type,
        c.nodeid       AS descendant_id,
        c.nodetype     AS descendant_type,
        c.roadmapsysid AS descendant_roadmap
    FROM rel r
    JOIN roadmap_tree c
            ON c.projectno    = r.projectno
         AND c.parenttype   = r.descendant_type
     AND c.parentsysid  = r.descendant_id
     AND c.roadmapsysid = r.descendant_roadmap
),

project_value_agg AS (
select pv.projectno,
        COUNT(*)   - 1 AS PROJECT_OVERALL_COUNT,
        SUM(CASE WHEN pv.status = 'COMPLETED'      THEN 1 ELSE 0 END) AS project_ovalue_complete_count,
        SUM(CASE WHEN pv.status = 'CANCELLED'   THEN 1 ELSE 0 END) AS project_ovalue_cancel_count,
        SUM(CASE WHEN pv.status = 'ONGOING'     THEN 1 ELSE 0 END) AS project_ovalue_ongoing_count,
        SUM(CASE WHEN pv.status = 'NOT STARTED' THEN 1 ELSE 0 END) AS project_ovalue_pending_count
from   node_value pv 
group by pv.projectno
) ,
 

value_agg AS (
    SELECT
        r.projectno,
        r.ancestor_id        AS nodeid,
        r.ancestor_type      AS nodetype,
        r.descendant_roadmap AS roadmapsysid, 
        COUNT(pv.nodesysid) AS node_value_count,
        SUM(CASE WHEN pv.status = 'COMPLETED'      THEN 1 ELSE 0 END) AS node_value_complete_count,
        SUM(CASE WHEN pv.status = 'CANCELLED'   THEN 1 ELSE 0 END) AS node_value_cancel_count,
        SUM(CASE WHEN pv.status = 'ONGOING'     THEN 1 ELSE 0 END) AS node_value_ongoing_count,
        SUM(CASE WHEN pv.status = 'NOT STARTED' THEN 1 ELSE 0 END) AS node_value_pending_count
    FROM rel r
    LEFT JOIN node_value pv
      ON pv.structuresysid = r.descendant_id
         AND pv.projectno      = r.projectno
    GROUP BY
                r.projectno,
        r.ancestor_id,
        r.ancestor_type,
        r.descendant_roadmap
),



finalsummary AS (
    SELECT
        h.lvl,
        h.full_path,
        h.projectno,
        h.nodeid,
        h.nodetype,
        h.parenttype,
        h.parentsysid,
        h.roadmapsysid,
        h.orderindex,
        h.dataname,
        h.datadescription,
        h.datamandays,
        h.dataisrequired,
        COUNT(*) - 1 AS total_descendants 
    FROM hier h
    JOIN rel r
            ON r.projectno          = h.projectno
         AND r.ancestor_id        = h.nodeid
     AND r.ancestor_type      = h.nodetype
     AND r.descendant_roadmap = h.roadmapsysid
    GROUP BY
        h.lvl,
        h.full_path,
        h.projectno,
        h.nodeid,
        h.nodetype,
        h.parenttype,
        h.parentsysid,
        h.roadmapsysid,
        h.orderindex,
        h.dataname,
        h.datadescription,
        h.datamandays,
        h.dataisrequired
),
 
flat_det AS (
    SELECT
        pl.projectno,
        pl.projectname,
        pl.projectdescription,
        pl.projecticon,
        pl.projecticoncolor,
        pc.productcodes,
        pm.members        AS jsonmembers,
        po.nodeowners     AS JsonNodeOwners,
        pl.projectmaturitycode,
        pl.status,
        pl.targetstartedby,
        pl.targetstartyear,
        pl.targetstartworkweek,
        cs.week_start_date AS targetstart,
        pl.targetstartdate,
        pl.targetcompletedby,
        pl.targetcompletionyear,
        pl.targetcompletionworkweek,
        ce.week_end_date  AS targetcompletion,
        pl.targetcompletiondate,
        pl.actualstartdate,
        pl.actualstartedby,
        pl.actualcompletiondate,
        pl.actualcompletedby,
        pl.isresched,
        
        fsum.dataname     AS nodename,
        fsum.datadescription AS nodedescription,
        fsum.lvl          AS nodelevel,
        fsum.full_path    AS nodefullpath,
        fsum.nodeid,
        fsum.nodetype,
        fsum.parenttype,
        fsum.parentsysid,
        fsum.roadmapsysid,
        fsum.datamandays  AS estimatedmandays,
        fsum.dataisrequired AS isrequired,
        
        JSON_OBJECT(
            'prerequisites' VALUE (
                SELECT JSON_ARRAYAGG(
                    JSON_OBJECT(
                        'id' VALUE pr.prerequisitesysid,
                        'name' VALUE fsm.dataname,
                        'path' VALUE fsm.full_path,
                        'status' VALUE NVL(nv.status,'NOT STARTED')
                    RETURNING CLOB) 
                RETURNING CLOB)
                FROM PROJECTROADMAPACTIVITYPREREQS pr 
                    INNER JOIN finalsummary fsm ON fsm.projectno = pl.projectno
                        AND fsm.nodeid = pr.prerequisitesysid   
                    LEFT OUTER JOIN node_value nv ON nv.projectno = pl.projectno
                        AND nv.structuresysid = pr.prerequisitesysid 
                WHERE pr.roadmapactivitysysid = fsum.nodeid 
                AND pr.projectno = pl.projectno
            )
        RETURNING CLOB
        ) AS PrerequisitesJson,    
        
        fsum.orderindex,
        fsum.total_descendants AS nodetotaldescendants,
        
        NVL(pva.PROJECT_OVERALL_COUNT, 0 )         AS projectcount,
        NVL(pva.project_ovalue_complete_count, 0)  AS projectcompletecount,
        NVL(pva.project_ovalue_cancel_count, 0)    AS projectcancelcount,
        NVL(pva.project_ovalue_ongoing_count, 0)   AS projectongoingcount,
        NVL(pva.project_ovalue_pending_count, 0)   AS projectpendingcount,       
        NVL(va.node_value_count - DECODE(fsum.nodeid,'__ROOTACTIVITY__',1,0), 0)             AS projectnodecount,
        NVL(va.node_value_complete_count, 0)    AS projectnodecompletecount,
        NVL(va.node_value_cancel_count, 0)      AS projectnodecancelcount,
        NVL(va.node_value_ongoing_count, 0)     AS projectnodeongoingcount,
        NVL(va.node_value_pending_count, 0)     AS projectnodependingcount,
        
        pv.nodesysid            AS projectnodesysid,
        pv.status               AS projectnodestatus,
        pv.remarks              AS projectnoderemarks,
        pv.targetstartedby      AS projectnodetargetstartedby,
        pv.targetstartyear      AS projectnodetargetstartyear,
        pv.targetstartworkweek  AS projectnodetargetstartworkweek,
        pv.targetstart          AS projectnodetargetstart,
        pv.targetstartdate          AS projectnodetargetstartdate,
        pv.targetcompletedby    AS projectnodetargetcompletedby,
        pv.targetcompletionyear AS projectnodetargetcompletionyear,
        pv.targetcompletionworkweek AS projectnodetargetcompletionworkweek,
        pv.targetcompletiondate     AS projectnodetargetcompletiondate,
        pv.targetcompletion     AS projectnodetargetcompletion,
        pv.actualstartdate      AS projectnodeactualstartdate,
        pv.actualstartedby      AS projectnodeactualstartedby,
        pv.actualcompletiondate AS projectnodeactualcompletiondate,
        pv.actualcompletedby    AS projectnodeactualcompletedby,
        pv.isresched    AS projectnodeisresched,
        DECODE(fsum.nodetype,'roadmap', pl.createdby, pv.createdby) createdby,
        DECODE(fsum.nodetype,'roadmap', pl.createddate, pv.createddate) createddate,
        DECODE(fsum.nodetype,'roadmap', pl.modifiedby, pv.modifiedby) modifiedby,
        DECODE(fsum.nodetype,'roadmap', pl.modifieddate, pv.modifieddate) modifieddate,
        DECODE(fsum.nodetype,'roadmap', pl.transactionkey, pv.transactionkey) transactionkey, 
        
        pl.projectownerid,
        projectownerusername,
        projectownerfirstname,
        projectownerlastname,
        projectowneremail,
        
        pl.productgroupcode,
        pg.productgroupname,
        pl.productdivisioncode,
        pd.productdivisionname,
        pl.plantcode,
        pln.plantname,
        pl.categorycode,
        ctg.categoryname
    FROM finalsummary fsum  
    INNER JOIN project_list pl
            ON pl.projectno = fsum.projectno
           AND pl.roadmapsysid = fsum.roadmapsysid 
    INNER JOIN project_owner pown
        ON pown.projectno     = pl.projectno  
       AND pown.projectownerid = pl.projectownerid
    INNER JOIN productgroups pg
        ON pg.productgroupcode = pl.productgroupcode  
    INNER JOIN productdivisions pd
        ON pd.productdivisioncode = pl.productdivisioncode      
    INNER JOIN plants pln
        ON pln.plantcode = pl.plantcode
    INNER JOIN categories ctg
        ON ctg.categorycode = pl.categorycode    
    LEFT JOIN project_members pm
        ON pm.projectno = pl.projectno
    LEFT JOIN node_value pv
        ON pv.projectno      = pl.projectno
       AND pv.structuresysid = fsum.nodeid
    LEFT JOIN node_owners po
        ON po.projectno  = pl.projectno 
       AND po.nodesysid = pv.nodesysid     
    LEFT JOIN value_agg va
        ON va.projectno    = pl.projectno
       AND va.nodeid       = fsum.nodeid
       AND UPPER(va.nodetype)     = UPPER(fsum.nodetype)
       AND va.roadmapsysid = fsum.roadmapsysid
    LEFT JOIN project_value_agg pva
        ON pva.projectno    = pl.projectno 
    LEFT JOIN calendar_range cs
        ON cs.calendaryear     = pl.targetstartyear
       AND cs.calendarworkweek = pl.targetstartworkweek
    LEFT JOIN calendar_range ce
        ON ce.calendaryear     = pl.targetcompletionyear
       AND ce.calendarworkweek = pl.targetcompletionworkweek
    LEFT JOIN project_productcodes pc
        ON pc.projectno = pl.projectno 
WHERE (:status IS NULL OR UPPER(:status) like '%'||UPPER(pv.status)||'%')  
    AND (:parenttype IS NULL OR(UPPER(:parenttype) LIKE '%'||UPPER(NVL(fsum.parenttype,'PARENT'))||'%'))
    AND (:nodetype IS NULL OR(UPPER(:nodetype) LIKE '%'||UPPER(NVL(fsum.nodetype,'PARENT'))||'%'))
)  
 ";

        public override async Task<string> AddAsync(Project project)
        {
            return await _dataAccess.SaveDataReturnParameterNameAsync<Project>(@"  INSERT INTO PROJECTS
      (
        PROJECTNAME
        ,PROJECTDESCRIPTION
        ,PROJECTICON
        ,PROJECTICONCOLOR
        ,PRODUCTGROUPCODE
        ,PRODUCTDIVISIONCODE
        ,PLANTCODE
        ,CATEGORYCODE
        ,PROJECTOWNERID
        ,PROJECTMATURITYCODE
        ,MILESTONESYSID
        ,ROADMAPMILESTONESYSID
        ,STATUS
        ,TARGETSTARTYEAR
        ,TARGETSTARTWORKWEEK
        ,TARGETSTARTEDBY
        ,TARGETCOMPLETIONYEAR
        ,TARGETCOMPLETIONWORKWEEK
        ,TARGETCOMPLETEDBY
        ,ACTUALSTARTDATE
        ,ACTUALSTARTEDBY
        ,ACTUALCOMPLETIONDATE
        ,ACTUALCOMPLETEDBY 
        ,ROADMAPSYSID
        ,PLANTROADMAPLINKSYSID
        ,CREATEDBY  
      )
    VALUES
      (
        :PROJECTNAME
        ,:PROJECTDESCRIPTION
        ,:PROJECTICON
        ,:PROJECTICONCOLOR
        ,:PRODUCTGROUPCODE
        ,:PRODUCTDIVISIONCODE
        ,:PLANTCODE
        ,:CATEGORYCODE
        ,:PROJECTOWNERID
        ,:PROJECTMATURITYCODE
        ,:MILESTONESYSID
        ,:ROADMAPMILESTONESYSID
        ,:STATUS
        ,:TARGETSTARTYEAR
        ,:TARGETSTARTWORKWEEK
        ,:TARGETSTARTEDBY
        ,:TARGETCOMPLETIONYEAR
        ,:TARGETCOMPLETIONWORKWEEK
        ,:TARGETCOMPLETEDBY
        ,:ACTUALSTARTDATE
        ,:ACTUALSTARTEDBY
        ,:ACTUALCOMPLETIONDATE
        ,:ACTUALCOMPLETEDBY 
        ,:ROADMAPSYSID
        ,:PLANTROADMAPLINKSYSID
        ,:CREATEDBY  
      )
RETURNING PROJECTNO INTO :PROJECTNO
", project, "PROJECTNO");
        }

        public override async Task<int> UpdateAsync(Project project)
        {
            return await _dataAccess.SaveDataAsync<Project>(@"UPDATE PROJECTS 
        SET PROJECTNAME=:PROJECTNAME
        ,PROJECTDESCRIPTION=:PROJECTDESCRIPTION
        ,PROJECTICON=:PROJECTICON
        ,PROJECTICONCOLOR=:PROJECTICONCOLOR
        ,PRODUCTGROUPCODE=:PRODUCTGROUPCODE
        ,PRODUCTDIVISIONCODE=:PRODUCTDIVISIONCODE
        ,PLANTCODE=:PLANTCODE
        ,CATEGORYCODE=:CATEGORYCODE
        ,PROJECTOWNERID=:PROJECTOWNERID
        ,PROJECTMATURITYCODE=:PROJECTMATURITYCODE
        ,MILESTONESYSID=:MILESTONESYSID
        ,ROADMAPMILESTONESYSID=:ROADMAPMILESTONESYSID
        ,STATUS=:STATUS
        ,TARGETSTARTYEAR=:TARGETSTARTYEAR
        ,TARGETSTARTWORKWEEK=:TARGETSTARTWORKWEEK
        ,TARGETSTARTEDBY=:TARGETSTARTEDBY
        ,TARGETCOMPLETIONYEAR=:TARGETCOMPLETIONYEAR
        ,TARGETCOMPLETIONWORKWEEK=:TARGETCOMPLETIONWORKWEEK
        ,TARGETCOMPLETEDBY=:TARGETCOMPLETEDBY
        ,ACTUALSTARTDATE=:ACTUALSTARTDATE
        ,ACTUALSTARTEDBY=:ACTUALSTARTEDBY
        ,ACTUALCOMPLETIONDATE=:ACTUALCOMPLETIONDATE
        ,ACTUALCOMPLETEDBY=:ACTUALCOMPLETEDBY 
        ,ROADMAPSYSID=:ROADMAPSYSID
        ,PLANTROADMAPLINKSYSID=:PLANTROADMAPLINKSYSID
        ,MODIFIEDBY = :MODIFIEDBY, MODIFIEDDATE = SYSTIMESTAMP, TRANSACTIONKEY = SYS_GUID() 
                        WHERE PROJECTNO = :PROJECTNO --AND TRANSACTIONKEY = :TRANSACTIONKEY
", project);
        }

        public override async Task<int> DeleteAsync(string projectno)
        {
            return await _dataAccess.SaveDataAsync<Project>("DELETE FROM PROJECTS WHERE PROJECTNO = :PROJECTNO", new Project { ProjectNo = projectno });
        }

        public override async Task<IEnumerable<Project>> GetListAsync()
        {
            return await _dataAccess.LoadDataAsync<Project>("SELECT * FROM PROJECTS")
                  .ContinueWith(t => (IEnumerable<Project>)t.Result);
        }

        public override async Task<Project> GetAsync(string projectno)
        {
            return await _dataAccess.FindDataAsync<Project>("SELECT * FROM PROJECTS WHERE PROJECTNO = :PROJECTNO",
                new Project { ProjectNo = projectno });
        }

        public async Task<Project> GetByProductCodeAsync(string productcode)
        {
            return await _dataAccess.FindDataAsync<ProjectSearch>("SELECT * FROM PROJECTS WHERE EXISTS (SELECT 1 FROM PROJECTPRODUCTS lnk WHERE lnk.PROJECTNO = prj.PROJECTNO AND lnk.PRODUCTCODE = :KeyString)",
          new ProjectSearch { KeyString = productcode });
        }

        public async Task<PagedResult<Project>> GetPagedProjectsAsync(int pageNumber, int pageSize, string searchTerm = null, string orderBy = null, string orderDirection = null)
        {
            // Calculate the offset for the current page
            int offset = (pageNumber - 1) * pageSize;

            string _orderby = $"{orderDirection} {orderBy}";

            string sqlBase = @" FROM PROJECTS prj
        WHERE (:SearchTerm IS NULL OR PROJECTNAME LIKE '%' || :SearchTerm || '%')
        OR (:SearchTerm IS NULL OR PROJECTNO LIKE '%' || :SearchTerm || '%')
        OR (EXISTS (SELECT 1 FROM PROJECTPRODUCTLINKS lnk WHERE lnk.PROJECTNO = prj.PROJECTNO AND (:SearchTerm IS NULL OR lnk.PRODUCTCODE LIKE '%' || :SearchTerm || '%'))";

            // SQL query to retrieve the paged data
            string pagedQuery = $@"
        SELECT *
        {sqlBase}
        ORDER BY {_orderby}
        OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY";

            // SQL query to retrieve the total record count
            string countQuery = $"SELECT COUNT(*) {sqlBase}";

            // Parameters for the queries
            var parameters = new
            {
                Offset = offset,       // The starting row for the current page
                PageSize = pageSize,   // The number of rows to retrieve
                SearchTerm = searchTerm // The search term for filtering (optional)
            };

            // Use the generic async method to retrieve the paged data and total count
            return await _dataAccess.GetPagedDataAsync<Project>(pagedQuery, countQuery, parameters);
        }


        public async Task<PagedResult<ProjectExtend>> GetPagedProjectsWithStatsAsync(ProjectExtendSearch searchTerm)
        {

            var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                    dataQuery: sql_flat_det + @"
select * from flat_det
WHERE fsum.parenttype = 'roadmap'
ORDER BY
    CASE WHEN :orderColumn = 'projectname' AND :orderDir = 'asc'  THEN pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'projectno' AND :orderDir = 'asc'  THEN pl.projectno || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'plantcode' AND :orderDir = 'asc'  THEN pl.plantcode || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'productgroup' AND :orderDir = 'asc'  THEN pg.productgroupname || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'productdivision' AND :orderDir = 'asc'  THEN pd.productdivisionname || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'orderindex' AND :orderDir = 'asc'  THEN LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn IS NULL AND :orderDir = 'asc'  THEN pl.projectno || LPAD(fsum.orderindex, 8, '0') END ASC,
    CASE WHEN :orderColumn = 'projectname' AND :orderDir = 'desc'  THEN pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'projectno' AND :orderDir = 'desc'  THEN pl.projectno || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'plantcode' AND :orderDir = 'desc'  THEN pl.plantcode || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'productgroup' AND :orderDir = 'desc'  THEN pg.productgroupname || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'productdivision' AND :orderDir = 'desc'  THEN pd.productdivisionname || pl.projectname || LPAD(fsum.orderindex, 8, '0')
         WHEN :orderColumn = 'orderindex' AND :orderDir = 'desc'  THEN LPAD(fsum.orderindex, 8, '0')  
         WHEN :orderColumn IS NULL AND :orderDir = 'desc'  THEN pl.projectno || LPAD(fsum.orderindex, 8, '0')  END DESC
OFFSET :startindex ROWS FETCH NEXT :lengthcount ROWS ONLY",
                                    parameters: searchTerm,
                                    ////parameters: new
                                    ////{
                                    ////    Search = searchTerm.Search,
                                    ////    ProjectOwnerId = searchTerm.ProjectOwnerId,
                                    ////    ProductGroupCode = searchTerm.ProductGroupCode,
                                    ////    ProductDivisionCode = searchTerm.ProductDivisionCode,
                                    ////    PlantCode = searchTerm.PlantCode,
                                    ////    CategoryCode = searchTerm.CategoryCode,
                                    ////    LoggedUser = searchTerm.LoggedUser,
                                    ////    OrderColumn = searchTerm.OrderColumn,
                                    ////    OrderDir = searchTerm.OrderDir,
                                    ////    StartIndex = searchTerm.StartIndex,
                                    ////    LengthCount = searchTerm.LengthCount,
                                    ////},
                                    map: (p, pg, pd, pl, c) => new ProjectExtend
                                    {
                                        TotalRows = p.TotalRows,
                                        ProjectNo = p.ProjectNo,
                                        ProjectName = p.ProjectName,
                                        ProjectDescription = p.ProjectDescription,
                                        ProjectIcon = p.ProjectIcon,
                                        ProjectIconColor = p.ProjectIconColor,
                                        JsonMembers = p.JsonMembers,
                                        JsonNodeOwners = p.JsonNodeOwners,
                                        ProjectMaturityCode = p.ProjectMaturityCode,
                                        ProductCodes = p.ProductCodes,
                                        PlantCode = p.PlantCode,
                                        Status = p.Status,
                                        TransactionKey = p.TransactionKey,
                                        TargetStart = p.TargetStart,
                                        TargetStartYear = p.TargetStartYear,
                                        TargetStartWorkWeek = p.TargetStartWorkWeek,
                                        TargetStartedBy = p.TargetStartedBy,
                                        TargetCompletion = p.TargetCompletion,
                                        TargetCompletionYear = p.TargetCompletionYear,
                                        TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                        TargetCompletedBy = p.TargetCompletedBy,
                                        ActualStartDate = p.ActualStartDate,
                                        ActualStartedBy = p.ActualStartedBy,
                                        ActualCompletionDate = p.ActualCompletionDate,
                                        ActualCompletedBy = p.ActualCompletedBy,
                                        CreatedBy = p.CreatedBy,
                                        CreatedDate = p.CreatedDate,
                                        ModifiedBy = p.ModifiedBy,
                                        ModifiedDate = p.ModifiedDate,
                                        NodeName = p.NodeName,
                                        NodeDescription = p.NodeDescription,
                                        NodeLevel = p.NodeLevel,
                                        NodeFullPath = p.NodeFullPath,
                                        NodeId = p.NodeId,
                                        NodeType = p.NodeType,
                                        ParentSysId = p.ParentSysId,
                                        ParentType = p.ParentType,
                                        EstimatedMandays = p.EstimatedMandays,
                                        IsRequired = p.IsRequired,
                                        OrderIndex = p.OrderIndex,
                                        NodeTotalDescendants = p.NodeTotalDescendants,
                                        ProjectCount = p.ProjectCount,
                                        ProjectCompleteCount = p.ProjectCompleteCount,
                                        ProjectCancelCount = p.ProjectCancelCount,
                                        ProjectOngoingCount = p.ProjectOngoingCount,
                                        ProjectPendingCount = p.ProjectPendingCount,
                                        ProjectTaskPendingCount = p.ProjectTaskPendingCount,
                                        ProjectTaskAtRiskCount = p.ProjectTaskAtRiskCount,
                                        ProjectTaskClosedCount = p.ProjectTaskClosedCount,
                                        ProjectTaskClosedDelayedCount = p.ProjectTaskClosedDelayedCount,

                                        ProjectNodeCount = p.ProjectNodeCount,
                                        ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                        ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                        ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                        ProjectNodePendingCount = p.ProjectNodePendingCount,

                                        ProjectNodeSysId = p.ProjectNodeSysId,
                                        ProjectNodeStatus = p.ProjectNodeStatus,
                                        ProjectNodeRemarks = p.ProjectNodeRemarks,
                                        PrerequisitesJson = p.PrerequisitesJson,
                                        ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                        ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                        ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                        ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                        ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                        ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                        ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                        ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                        ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                        ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                        ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                        ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                        ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                        ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                        ProjectOwnerId = p.ProjectOwnerId,
                                        ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                        ProjectOwnerLastName = p.ProjectOwnerLastName,
                                        ProjectOwnerEmail = p.ProjectOwnerEmail,

                                        ProductGroupCode = pg.ProductGroupCode,
                                        ProductGroup = new ProductGroup
                                        {
                                            ProductGroupCode = pg.ProductGroupCode,
                                            ProductGroupName = pg.ProductGroupName
                                        },
                                        ProductDivisionCode = pd.ProductDivisionCode,
                                        ProductDivision = new ProductDivision
                                        {
                                            ProductDivisionCode = pd.ProductDivisionCode,
                                            ProductDivisionName = pd.ProductDivisionName
                                        },
                                        Plant = new Plant
                                        {
                                            PlantCode = pl.PlantCode,
                                            PlantName = pl.PlantName
                                        },
                                        Category = new Category
                                        {
                                            CategoryCode = c.CategoryCode,
                                            CategoryName = c.CategoryName
                                        }

                                    },
                                    splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                                );



            // Parameters for the queries
            var returnvalue = new PagedResult<ProjectExtend>
            {
                Data = dataresult,
                TotalRecords = dataresult.FirstOrDefault().TotalRows ?? 0
            };

            // Use the generic async method to retrieve the paged data and total count
            return returnvalue;
        }

        public async Task<PagedResult<ProjectExtend>> GetPagedFullProjectsAsync(ProjectExtendSearch searchTerm)
        {
            var sql = sql_flat_det + @", 
flat_count AS(
    SELECT COUNT(1) AS TotalRows FROM flat_det
),

flat_result AS (select fc.TotalRows, fd.* 
    FROM flat_det fd 
    CROSS JOIN  flat_count fc),

flat_paged AS (
    SELECT fr.*
    FROM flat_result fr
    ORDER BY
        CASE WHEN :orderColumn = 'projectname'   AND :orderDir = 'asc'  THEN fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'projectno'     AND :orderDir = 'asc'  THEN fr.projectno   || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'plantcode'     AND :orderDir = 'asc'  THEN fr.plantcode   || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'productgroup'  AND :orderDir = 'asc'  THEN fr.productgroupname    || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'productdivision' AND :orderDir = 'asc' THEN fr.productdivisionname || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'orderindex'    AND :orderDir = 'asc'  THEN LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn IS NULL           AND :orderDir = 'asc'  THEN fr.projectno   || LPAD(fr.orderindex, 8, '0') END ASC,
        CASE WHEN :orderColumn = 'projectname'   AND :orderDir = 'desc' THEN fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'projectno'     AND :orderDir = 'desc' THEN fr.projectno   || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'plantcode'     AND :orderDir = 'desc' THEN fr.plantcode   || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'productgroup'  AND :orderDir = 'desc' THEN fr.productgroupname    || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'productdivision' AND :orderDir = 'desc' THEN fr.productdivisionname || fr.projectname || LPAD(fr.orderindex, 8, '0')
             WHEN :orderColumn = 'orderindex'    AND :orderDir = 'desc' THEN LPAD(fr.orderindex, 8, '0')  
             WHEN :orderColumn IS NULL           AND :orderDir = 'desc' THEN fr.projectno   || LPAD(fr.orderindex, 8, '0')  END DESC
    OFFSET :startindex ROWS FETCH NEXT :lengthcount ROWS ONLY
)
select * from flat_paged
WHERE (:projectno is null 
    or projectno IN (SELECT REGEXP_SUBSTR(:projectno, '[^,]+', 1, LEVEL) FROM dual CONNECT BY REGEXP_SUBSTR(:projectno, '[^,]+', 1, LEVEL) IS NOT NULL))
AND (:productcode is null  
    OR EXISTS (
        SELECT 1
        FROM (
            SELECT TRIM(REGEXP_SUBSTR(:PRODUCTCODE, '[^,]+', 1, LEVEL)) AS p
            FROM dual
            CONNECT BY REGEXP_SUBSTR(:PRODUCTCODE, '[^,]+', 1, LEVEL) IS NOT NULL
        ) t
        WHERE INSTR(',' || REPLACE(productcodes, ' ', '') || ',', ',' || REPLACE(p, ' ', '') || ',') > 0
   )
)
AND (:plantcode is null or plantcode = :plantcode)
AND (:categorycode is null or categorycode = :categorycode)
AND (:productgroupcode is null or productgroupcode = :productgroupcode)
AND (:productdivisioncode is null or productdivisioncode = :productdivisioncode)
";

            try
            {
                var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                    dataQuery: sql,
                                    parameters: searchTerm,
                                    map: (p, pg, pd, pl, c) => new ProjectExtend
                                    {
                                        TotalRows = p.TotalRows,
                                        ProjectNo = p.ProjectNo,
                                        ProjectName = p.ProjectName,
                                        ProjectDescription = p.ProjectDescription,
                                        ProjectIcon = p.ProjectIcon,
                                        ProjectIconColor = p.ProjectIconColor,
                                        JsonMembers = p.JsonMembers,
                                        JsonNodeOwners = p.JsonNodeOwners,
                                        ProjectMaturityCode = p.ProjectMaturityCode,
                                        ProductCodes = p.ProductCodes,
                                        
                                        Status = p.Status,
                                        TransactionKey = p.TransactionKey,
                                        TargetStart = p.TargetStart,
                                        TargetStartYear = p.TargetStartYear,
                                        TargetStartWorkWeek = p.TargetStartWorkWeek,
                                        TargetStartedBy = p.TargetStartedBy,
                                        TargetCompletion = p.TargetCompletion,
                                        TargetCompletionYear = p.TargetCompletionYear,
                                        TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                        TargetCompletedBy = p.TargetCompletedBy,
                                        ActualStartDate = p.ActualStartDate,
                                        ActualStartedBy = p.ActualStartedBy,
                                        ActualCompletionDate = p.ActualCompletionDate,
                                        ActualCompletedBy = p.ActualCompletedBy,
                                        CreatedBy = p.CreatedBy,
                                        CreatedDate = p.CreatedDate,
                                        ModifiedBy = p.ModifiedBy,
                                        ModifiedDate = p.ModifiedDate,
                                        NodeName = p.NodeName,
                                        NodeDescription = p.NodeDescription,
                                        NodeLevel = p.NodeLevel,
                                        NodeFullPath = p.NodeFullPath,
                                        NodeId = p.NodeId,
                                        NodeType = p.NodeType,
                                        ParentSysId = p.ParentSysId,
                                        ParentType = p.ParentType,
                                        EstimatedMandays = p.EstimatedMandays,
                                        IsRequired = p.IsRequired,
                                        OrderIndex = p.OrderIndex,
                                        NodeTotalDescendants = p.NodeTotalDescendants,
                                        ProjectCount = p.ProjectCount,
                                        ProjectCompleteCount = p.ProjectCompleteCount,
                                        ProjectCancelCount = p.ProjectCancelCount,
                                        ProjectOngoingCount = p.ProjectOngoingCount,
                                        ProjectPendingCount = p.ProjectPendingCount,

                                        ProjectNodeCount = p.ProjectNodeCount,
                                        ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                        ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                        ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                        ProjectNodePendingCount = p.ProjectNodePendingCount,

                                        ProjectNodeSysId = p.ProjectNodeSysId,
                                        ProjectNodeStatus = p.ProjectNodeStatus,
                                        ProjectNodeRemarks = p.ProjectNodeRemarks,
                                        PrerequisitesJson = p.PrerequisitesJson,
                                        ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                        ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                        ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                        ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                        ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                        ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                        ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                        ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                        ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                        ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                        ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                        ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                        ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                        ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                        ProjectOwnerId = p.ProjectOwnerId,
                                        ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                        ProjectOwnerLastName = p.ProjectOwnerLastName,
                                        ProjectOwnerEmail = p.ProjectOwnerEmail,
                                        ProjectOwnerUserName = p.ProjectOwnerUserName,
                                        ProductGroupCode = pg.ProductGroupCode,
                                        ProductGroup = new ProductGroup
                                        {
                                            ProductGroupCode = pg.ProductGroupCode,
                                            ProductGroupName = pg.ProductGroupName
                                        },
                                        ProductDivisionCode = pd.ProductDivisionCode,
                                        ProductDivision = new ProductDivision
                                        {
                                            ProductDivisionCode = pd.ProductDivisionCode,
                                            ProductDivisionName = pd.ProductDivisionName
                                        },
                                        PlantCode = pl.PlantCode,
                                        Plant = new Plant
                                        {
                                            PlantCode = pl.PlantCode,
                                            PlantName = pl.PlantName
                                        },
                                        CategoryCode = c.CategoryCode,
                                        Category = new Category
                                        {
                                            CategoryCode = c.CategoryCode,
                                            CategoryName = c.CategoryName
                                        }

                                    },
                                    splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                                );

                var totalrows = 0;
                if (dataresult.Count() == 0)
                {
                    dataresult = Enumerable.Empty<ProjectExtend>().ToList();
                }
                else
                {
                    totalrows = dataresult.FirstOrDefault().TotalRows.Value;
                }

                // Parameters for the queries
                var returnvalue = new PagedResult<ProjectExtend>
                {
                    Data = dataresult,
                    TotalRecords = totalrows
                };

                // Use the generic async method to retrieve the paged data and total count
                return returnvalue;
            }
            catch (Exception)
            {
                return new PagedResult<ProjectExtend>
                {
                    Data = Enumerable.Empty<ProjectExtend>().ToList(),
                    TotalRecords = 0
                };

            }


        }


        public async Task<List<ProjectExtend>> GetProjectNodeChildrenAsync(string projectNo, string nodeType, string nodeId)
        {
            var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                   dataQuery: sql_flat_det_child + @"
select * from flat_det
 ",
                                   parameters: new
                                   {
                                       Search = projectNo,
                                       ParentSysId = nodeId,
                                       ParentType = nodeType,
                                       ProductDivisionCode = "",
                                       ProjectOwnerId = "",
                                       NodeType = "",
                                       CategoryCode = "",
                                       ProductGroupCode = "",
                                       LoggedUser = "",
                                       PlantCode = "",
                                       PlantType = "",
                                       Status = ""
                                   },
                                   map: (p, pg, pd, pl, c) => new ProjectExtend
                                   {
                                       TotalRows = p.TotalRows,
                                       ProjectNo = p.ProjectNo,
                                       ProjectName = p.ProjectName,
                                       ProjectDescription = p.ProjectDescription,
                                       ProjectIcon = p.ProjectIcon,
                                       ProjectIconColor = p.ProjectIconColor,
                                       JsonMembers = p.JsonMembers,
                                       JsonNodeOwners = p.JsonNodeOwners,
                                       ProjectMaturityCode = p.ProjectMaturityCode,
                                       ProductCodes = p.ProductCodes,

                                       Status = p.Status,
                                       TransactionKey = p.TransactionKey,
                                       TargetStart = p.TargetStart,
                                       TargetStartYear = p.TargetStartYear,
                                       TargetStartWorkWeek = p.TargetStartWorkWeek,
                                       TargetStartedBy = p.TargetStartedBy,
                                       TargetCompletion = p.TargetCompletion,
                                       TargetCompletionYear = p.TargetCompletionYear,
                                       TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                       TargetCompletedBy = p.TargetCompletedBy,
                                       ActualStartDate = p.ActualStartDate,
                                       ActualStartedBy = p.ActualStartedBy,
                                       ActualCompletionDate = p.ActualCompletionDate,
                                       ActualCompletedBy = p.ActualCompletedBy,
                                       CreatedBy = p.CreatedBy,
                                       CreatedDate = p.CreatedDate,
                                       ModifiedBy = p.ModifiedBy,
                                       ModifiedDate = p.ModifiedDate,
                                       NodeName = p.NodeName,
                                       NodeDescription = p.NodeDescription,
                                       NodeLevel = p.NodeLevel,
                                       NodeFullPath = p.NodeFullPath,
                                       NodeId = p.NodeId,
                                       NodeType = p.NodeType,
                                       ParentSysId = p.ParentSysId,
                                       ParentType = p.ParentType,
                                       EstimatedMandays = p.EstimatedMandays,
                                       IsRequired = p.IsRequired,
                                       OrderIndex = p.OrderIndex,
                                       NodeTotalDescendants = p.NodeTotalDescendants,
                                       ProjectCount = p.ProjectCount,
                                       ProjectCompleteCount = p.ProjectCompleteCount,
                                       ProjectCancelCount = p.ProjectCancelCount,
                                       ProjectOngoingCount = p.ProjectOngoingCount,
                                       ProjectPendingCount = p.ProjectPendingCount,

                                       ProjectNodeCount = p.ProjectNodeCount,
                                       ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                       ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                       ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                       ProjectNodePendingCount = p.ProjectNodePendingCount,

                                       ProjectNodeSysId = p.ProjectNodeSysId,
                                       ProjectNodeStatus = p.ProjectNodeStatus,
                                       ProjectNodeRemarks = p.ProjectNodeRemarks,
                                       PrerequisitesJson = p.PrerequisitesJson,
                                       ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                       ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                       ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                       ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                       ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                       ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                       ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                       ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                       ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                       ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                       ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                       ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                       ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                       ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                       ProjectOwnerId = p.ProjectOwnerId,
                                       ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                       ProjectOwnerLastName = p.ProjectOwnerLastName,
                                       ProjectOwnerEmail = p.ProjectOwnerEmail,
                                       ProjectOwnerUserName = p.ProjectOwnerUserName,
                                       ProductGroupCode = pg.ProductGroupCode,
                                       ProductGroup = new ProductGroup
                                       {
                                           ProductGroupCode = pg.ProductGroupCode,
                                           ProductGroupName = pg.ProductGroupName
                                       },
                                       ProductDivisionCode = pd.ProductDivisionCode,
                                       ProductDivision = new ProductDivision
                                       {
                                           ProductDivisionCode = pd.ProductDivisionCode,
                                           ProductDivisionName = pd.ProductDivisionName
                                       },
                                       PlantCode = pl.PlantCode,
                                       Plant = new Plant
                                       {
                                           PlantCode = pl.PlantCode,
                                           PlantName = pl.PlantName
                                       },
                                       CategoryCode = c.CategoryCode,
                                       Category = new Category
                                       {
                                           CategoryCode = c.CategoryCode,
                                           CategoryName = c.CategoryName
                                       }

                                   },
                                   splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                               );

            return DeduplicateProjectNodeResults(dataresult);
        }


        public async Task<List<ProjectExtend>> GetProjectNodesAsync(string projectNo)
        {

            var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                    dataQuery: sql_flat_det + @"
select * from flat_det fsum
WHERE fsum.nodetype <> 'roadmap'
ORDER BY LPAD(fsum.orderindex, 8, '0') ASC",
                                    parameters: new
                                    {
                                        Search = projectNo,
                                        ParentSysId = "",
                                        ParentType = "",
                                        ProductDivisionCode = "",
                                        ProjectOwnerId = "",
                                        NodeType = "",
                                        CategoryCode = "",
                                        ProductGroupCode = "",
                                        LoggedUser = "",
                                        PlantCode = "",
                                        PlantType = "",
                                        Status = ""
                                    }, 
                                    map: (p, pg, pd, pl, c) => new ProjectExtend
                                    {
                                        TotalRows = p.TotalRows,
                                        ProjectNo = p.ProjectNo,
                                        ProjectName = p.ProjectName,
                                        ProjectDescription = p.ProjectDescription,
                                        ProjectIcon = p.ProjectIcon,
                                        ProjectIconColor = p.ProjectIconColor,
                                        JsonMembers = p.JsonMembers,
                                        JsonNodeOwners = p.JsonNodeOwners,
                                        ProjectMaturityCode = p.ProjectMaturityCode,
                                        ProductCodes = p.ProductCodes,
                                        PlantCode = p.PlantCode,
                                        Status = p.Status,
                                        TransactionKey = p.TransactionKey,
                                        TargetStart = p.TargetStart,
                                        TargetStartYear = p.TargetStartYear,
                                        TargetStartWorkWeek = p.TargetStartWorkWeek,
                                        TargetStartedBy = p.TargetStartedBy,
                                        TargetCompletion = p.TargetCompletion,
                                        TargetCompletionYear = p.TargetCompletionYear,
                                        TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                        TargetCompletedBy = p.TargetCompletedBy,
                                        ActualStartDate = p.ActualStartDate,
                                        ActualStartedBy = p.ActualStartedBy,
                                        ActualCompletionDate = p.ActualCompletionDate,
                                        ActualCompletedBy = p.ActualCompletedBy,
                                        CreatedBy = p.CreatedBy,
                                        CreatedDate = p.CreatedDate,
                                        ModifiedBy = p.ModifiedBy,
                                        ModifiedDate = p.ModifiedDate,
                                        NodeName = p.NodeName,
                                        NodeDescription = p.NodeDescription,
                                        NodeLevel = p.NodeLevel,
                                        NodeFullPath = p.NodeFullPath,
                                        NodeId = p.NodeId,
                                        NodeType = p.NodeType,
                                        ParentSysId = p.ParentSysId,
                                        ParentType = p.ParentType,
                                        EstimatedMandays = p.EstimatedMandays,
                                        IsRequired = p.IsRequired,
                                        OrderIndex = p.OrderIndex,
                                        NodeTotalDescendants = p.NodeTotalDescendants,
                                        ProjectCount = p.ProjectCount,
                                        ProjectCompleteCount = p.ProjectCompleteCount,
                                        ProjectCancelCount = p.ProjectCancelCount,
                                        ProjectOngoingCount = p.ProjectOngoingCount,
                                        ProjectPendingCount = p.ProjectPendingCount,

                                        ProjectNodeCount = p.ProjectNodeCount,
                                        ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                        ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                        ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                        ProjectNodePendingCount = p.ProjectNodePendingCount,

                                        ProjectNodeSysId = p.ProjectNodeSysId,
                                        ProjectNodeStatus = p.ProjectNodeStatus,
                                        ProjectNodeRemarks = p.ProjectNodeRemarks,
                                        PrerequisitesJson = p.PrerequisitesJson,
                                        ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                        ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                        ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                        ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                        ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                        ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                        ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                        ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                        ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                        ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                        ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                        ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                        ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                        ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                        ProjectOwnerId = p.ProjectOwnerId,
                                        ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                        ProjectOwnerLastName = p.ProjectOwnerLastName,
                                        ProjectOwnerEmail = p.ProjectOwnerEmail,

                                        ProductGroupCode = pg.ProductGroupCode,
                                        ProductGroup = new ProductGroup
                                        {
                                            ProductGroupCode = pg.ProductGroupCode,
                                            ProductGroupName = pg.ProductGroupName
                                        },
                                        ProductDivisionCode = pd.ProductDivisionCode,
                                        ProductDivision = new ProductDivision
                                        {
                                            ProductDivisionCode = pd.ProductDivisionCode,
                                            ProductDivisionName = pd.ProductDivisionName
                                        },
                                        Plant = new Plant
                                        {
                                            PlantCode = pl.PlantCode,
                                            PlantName = pl.PlantName
                                        },
                                        Category = new Category
                                        {
                                            CategoryCode = c.CategoryCode,
                                            CategoryName = c.CategoryName
                                        }

                                    },
                                    splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                                );
             
            return dataresult;
        }

        public async Task<ProjectExtend> GetProjectNodeItemAsync(string projectNo, string nodeType, string nodeId)
        {
            var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                   dataQuery: sql_flat_det + @"
select * from flat_det
WHERE  NodeId = :nodeid
AND NodeType = :nodetype
ORDER BY LPAD(orderindex, 8, '0') ASC",
                                   parameters: new ProjectExtendSearch
                                   {
                                       Search = projectNo,
                                       NodeId = nodeId,
                                       NodeType = nodeType
                                   }
                                   ,
                                   map: (p, pg, pd, pl, c) => new ProjectExtend
                                   {
                                       TotalRows = p.TotalRows,
                                       ProjectNo = p.ProjectNo,
                                       ProjectName = p.ProjectName,
                                       ProjectDescription = p.ProjectDescription,
                                       ProjectIcon = p.ProjectIcon,
                                       ProjectIconColor = p.ProjectIconColor,
                                       JsonMembers = p.JsonMembers,
                                       JsonNodeOwners = p.JsonNodeOwners,
                                       ProjectMaturityCode = p.ProjectMaturityCode,
                                       ProductCodes = p.ProductCodes,

                                       Status = p.Status,
                                       TransactionKey = p.TransactionKey,
                                       TargetStart = p.TargetStart,
                                       TargetStartYear = p.TargetStartYear,
                                       TargetStartWorkWeek = p.TargetStartWorkWeek,
                                       TargetStartedBy = p.TargetStartedBy,
                                       TargetCompletion = p.TargetCompletion,
                                       TargetCompletionYear = p.TargetCompletionYear,
                                       TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                       TargetCompletedBy = p.TargetCompletedBy,
                                       ActualStartDate = p.ActualStartDate,
                                       ActualStartedBy = p.ActualStartedBy,
                                       ActualCompletionDate = p.ActualCompletionDate,
                                       ActualCompletedBy = p.ActualCompletedBy,
                                       CreatedBy = p.CreatedBy,
                                       CreatedDate = p.CreatedDate,
                                       ModifiedBy = p.ModifiedBy,
                                       ModifiedDate = p.ModifiedDate,
                                       NodeName = p.NodeName,
                                       NodeDescription = p.NodeDescription,
                                       NodeLevel = p.NodeLevel,
                                       NodeFullPath = p.NodeFullPath,
                                       NodeId = p.NodeId,
                                       NodeType = p.NodeType,
                                       ParentSysId = p.ParentSysId,
                                       ParentType = p.ParentType,
                                       EstimatedMandays = p.EstimatedMandays,
                                       IsRequired = p.IsRequired,
                                       OrderIndex = p.OrderIndex,
                                       NodeTotalDescendants = p.NodeTotalDescendants,
                                       ProjectCount = p.ProjectCount,
                                       ProjectCompleteCount = p.ProjectCompleteCount,
                                       ProjectCancelCount = p.ProjectCancelCount,
                                       ProjectOngoingCount = p.ProjectOngoingCount,
                                       ProjectPendingCount = p.ProjectPendingCount,

                                       ProjectNodeCount = p.ProjectNodeCount,
                                       ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                       ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                       ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                       ProjectNodePendingCount = p.ProjectNodePendingCount,

                                       ProjectNodeSysId = p.ProjectNodeSysId,
                                       ProjectNodeStatus = p.ProjectNodeStatus,
                                       ProjectNodeRemarks = p.ProjectNodeRemarks,
                                       PrerequisitesJson = p.PrerequisitesJson,
                                       ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                       ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                       ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                       ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                       ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                       ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                       ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                       ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                       ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                       ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                       ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                       ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                       ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                       ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                       ProjectOwnerId = p.ProjectOwnerId,
                                       ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                       ProjectOwnerLastName = p.ProjectOwnerLastName,
                                       ProjectOwnerEmail = p.ProjectOwnerEmail,
                                       ProjectOwnerUserName = p.ProjectOwnerUserName,
                                       ProductGroupCode = pg.ProductGroupCode,
                                       ProductGroup = new ProductGroup
                                       {
                                           ProductGroupCode = pg.ProductGroupCode,
                                           ProductGroupName = pg.ProductGroupName
                                       },
                                       ProductDivisionCode = pd.ProductDivisionCode,
                                       ProductDivision = new ProductDivision
                                       {
                                           ProductDivisionCode = pd.ProductDivisionCode,
                                           ProductDivisionName = pd.ProductDivisionName
                                       },
                                       PlantCode = pl.PlantCode,
                                       Plant = new Plant
                                       {
                                           PlantCode = pl.PlantCode,
                                           PlantName = pl.PlantName
                                       },
                                       CategoryCode = c.CategoryCode,
                                       Category = new Category
                                       {
                                           CategoryCode = c.CategoryCode,
                                           CategoryName = c.CategoryName
                                       }

                                   },
                                   splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                               );

            return dataresult.SingleOrDefault();
        }

          public async Task<ProjectDashboardCounter> GetDashboardCardsCounter(string userid)
        {
            var sql = @"
WITH validprojects AS (SELECT p.*, pc.fiscaldate projectwkfiscaldate
                                 FROM projects p
                                        LEFT OUTER JOIN productioncalendars pc
                                            ON pc.calendaryear = p.targetcompletionyear AND pc.calendarworkweek = p.targetcompletionworkweek
                                WHERE (:userid IS NULL
                                     OR EXISTS (SELECT 1
                                                      FROM plantmembers pm
                                                     WHERE pm.userid = :userid
                                                        AND pm.plantcode = p.plantcode))),
     validtasks AS (SELECT pt.*, vp.projectwkfiscaldate, pc.fiscaldate taskwkfiscaldate
                      FROM projecttasks pt
                           INNER JOIN validprojects vp
                              ON vp.projectno = pt.projectno
                           LEFT OUTER JOIN productioncalendars pc
                              ON pc.calendaryear = pt.targetcompletionyear AND pc.calendarworkweek = pt.targetcompletionworkweek)
SELECT (SELECT COUNT (DISTINCT projectno)
          FROM validtasks vt
         WHERE vt.status NOT IN ('COMPLETED', 'CANCELED', 'NOT STARTED'))
          activeprojects,
       (SELECT COUNT (DISTINCT projecttasksysid)
          FROM validtasks vt
         WHERE vt.status NOT IN ('COMPLETED', 'CANCELED', 'NOT STARTED'))
          inprogress,
       (SELECT COUNT (DISTINCT projecttasksysid)
          FROM validtasks vt
         WHERE vt.status IN ('COMPLETED', 'CANCELED'))
          completedtasks,
       (SELECT COUNT (DISTINCT projecttasksysid)
          FROM validtasks vt
         WHERE vt.status NOT IN ('COMPLETED', 'CANCELED', 'NOT STARTED')
               AND ( (targetcompletiondate IS NOT NULL AND targetcompletiondate < SYSDATE) OR (taskwkfiscaldate IS NOT NULL AND taskwkfiscaldate < SYSDATE))
               OR (projectwkfiscaldate IS NOT NULL AND projectwkfiscaldate < SYSDATE))
                    overdue
  FROM DUAL
";
            return await _dataAccess.FindDataAsync<ProjectDashboardCounter>(sql, new ProjectDashboardCounter
            {
                UserId = userid
            });
        }


        public async Task<List<ProjectExtend>> GetProjectNodesByUserAsync(string userid)
        {

            var dataresult = await _dataAccess.GetMappedDataAsync<ProjectExtend, ProductGroup, ProductDivision, Plant, Category, ProjectExtend>(
                                    dataQuery: sql_flat_det + @"
select * from flat_det fsum
WHERE fsum.nodetype <> 'roadmap'
ORDER BY LPAD(fsum.orderindex, 8, '0') ASC",
                                    parameters: new
                                    {
                                        Search = "",
                                        ParentSysId = "",
                                        ParentType = "",
                                        ProductDivisionCode = "",
                                        ProjectOwnerId = "",
                                        NodeType = "",
                                        CategoryCode = "",
                                        ProductGroupCode = "",
                                        LoggedUser = userid,
                                        PlantCode = "",
                                        PlantType = "",
                                        Status = ""
                                    },
                                    map: (p, pg, pd, pl, c) => new ProjectExtend
                                    {
                                        TotalRows = p.TotalRows,
                                        ProjectNo = p.ProjectNo,
                                        ProjectName = p.ProjectName,
                                        ProjectDescription = p.ProjectDescription,
                                        ProjectIcon = p.ProjectIcon,
                                        ProjectIconColor = p.ProjectIconColor,
                                        JsonMembers = p.JsonMembers,
                                        JsonNodeOwners = p.JsonNodeOwners,
                                        ProjectMaturityCode = p.ProjectMaturityCode,
                                        ProductCodes = p.ProductCodes,
                                        PlantCode = p.PlantCode,
                                        Status = p.Status,
                                        TransactionKey = p.TransactionKey,
                                        TargetStart = p.TargetStart,
                                        TargetStartYear = p.TargetStartYear,
                                        TargetStartWorkWeek = p.TargetStartWorkWeek,
                                        TargetStartedBy = p.TargetStartedBy,
                                        TargetCompletion = p.TargetCompletion,
                                        TargetCompletionYear = p.TargetCompletionYear,
                                        TargetCompletionWorkWeek = p.TargetCompletionWorkWeek,
                                        TargetCompletedBy = p.TargetCompletedBy,
                                        ActualStartDate = p.ActualStartDate,
                                        ActualStartedBy = p.ActualStartedBy,
                                        ActualCompletionDate = p.ActualCompletionDate,
                                        ActualCompletedBy = p.ActualCompletedBy,
                                        CreatedBy = p.CreatedBy,
                                        CreatedDate = p.CreatedDate,
                                        ModifiedBy = p.ModifiedBy,
                                        ModifiedDate = p.ModifiedDate,
                                        NodeName = p.NodeName,
                                        NodeDescription = p.NodeDescription,
                                        NodeLevel = p.NodeLevel,
                                        NodeFullPath = p.NodeFullPath,
                                        NodeId = p.NodeId,
                                        NodeType = p.NodeType,
                                        ParentSysId = p.ParentSysId,
                                        ParentType = p.ParentType,
                                        EstimatedMandays = p.EstimatedMandays,
                                        IsRequired = p.IsRequired,
                                        OrderIndex = p.OrderIndex,
                                        NodeTotalDescendants = p.NodeTotalDescendants,
                                        ProjectCount = p.ProjectCount,
                                        ProjectCompleteCount = p.ProjectCompleteCount,
                                        ProjectCancelCount = p.ProjectCancelCount,
                                        ProjectOngoingCount = p.ProjectOngoingCount,
                                        ProjectPendingCount = p.ProjectPendingCount,

                                        ProjectNodeCount = p.ProjectNodeCount,
                                        ProjectNodeCompleteCount = p.ProjectNodeCompleteCount,
                                        ProjectNodeCancelCount = p.ProjectNodeCancelCount,
                                        ProjectNodeOngoingCount = p.ProjectNodeOngoingCount,
                                        ProjectNodePendingCount = p.ProjectNodePendingCount,

                                        ProjectNodeSysId = p.ProjectNodeSysId,
                                        ProjectNodeStatus = p.ProjectNodeStatus,
                                        ProjectNodeRemarks = p.ProjectNodeRemarks,
                                        PrerequisitesJson = p.PrerequisitesJson,
                                        ProjectNodeTargetStartedBy = p.ProjectNodeTargetStartedBy,
                                        ProjectNodeTargetStartYear = p.ProjectNodeTargetStartYear,
                                        ProjectNodeTargetStartWorkWeek = p.ProjectNodeTargetStartWorkWeek,
                                        ProjectNodeTargetStart = p.ProjectNodeTargetStart,
                                        ProjectNodeTargetStartDate = p.ProjectNodeTargetStartDate,
                                        ProjectNodeTargetCompletedBy = p.ProjectNodeTargetCompletedBy,
                                        ProjectNodeTargetCompletionYear = p.ProjectNodeTargetCompletionYear,
                                        ProjectNodeTargetCompletionWorkWeek = p.ProjectNodeTargetCompletionWorkWeek,
                                        ProjectNodeTargetCompletionDate = p.ProjectNodeTargetCompletionDate,
                                        ProjectNodeTargetCompletion = p.ProjectNodeTargetCompletion,
                                        ProjectNodeActualStartDate = p.ProjectNodeActualStartDate,
                                        ProjectNodeActualStartedBy = p.ProjectNodeActualStartedBy,
                                        ProjectNodeActualCompletionDate = p.ProjectNodeActualCompletionDate,
                                        ProjectNodeActualCompletedBy = p.ProjectNodeActualCompletedBy,
                                        ProjectOwnerId = p.ProjectOwnerId,
                                        ProjectOwnerFirstName = p.ProjectOwnerFirstName,
                                        ProjectOwnerLastName = p.ProjectOwnerLastName,
                                        ProjectOwnerEmail = p.ProjectOwnerEmail,

                                        ProductGroupCode = pg.ProductGroupCode,
                                        ProductGroup = new ProductGroup
                                        {
                                            ProductGroupCode = pg.ProductGroupCode,
                                            ProductGroupName = pg.ProductGroupName
                                        },
                                        ProductDivisionCode = pd.ProductDivisionCode,
                                        ProductDivision = new ProductDivision
                                        {
                                            ProductDivisionCode = pd.ProductDivisionCode,
                                            ProductDivisionName = pd.ProductDivisionName
                                        },
                                        Plant = new Plant
                                        {
                                            PlantCode = pl.PlantCode,
                                            PlantName = pl.PlantName
                                        },
                                        Category = new Category
                                        {
                                            CategoryCode = c.CategoryCode,
                                            CategoryName = c.CategoryName
                                        }

                                    },
                                    splitOn: "productgroupcode, productdivisioncode, plantcode, categorycode"
                                );

            return DeduplicateProjectNodeResults(dataresult);
        }

        public async Task<RoadmapMilestone> GetProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId)
        {
            return (await _dataAccess.QueryAsync<RoadmapMilestone>(@"
SELECT roadmapsysid,
       roadmapmilestonesysid,
       maturitycode,
       parenttype,
       parentsysid,
       milestonealias,
       milestonedescription,
       orderindex,
       NVL(isactive, 1) AS isactive,
       NVL(isrequired, 1) AS isrequired,
       createdby,
       createddate,
       modifiedby,
       modifieddate,
       transactionkey
  FROM projectroadmapmilestones
 WHERE projectno = :ProjectNo
   AND roadmapmilestonesysid = :RoadmapMilestoneSysId",
                new { ProjectNo = projectNo, RoadmapMilestoneSysId = roadmapMilestoneSysId })).SingleOrDefault();
        }

        public async Task<List<RoadmapMilestone>> GetProjectRoadmapMilestonesAsync(string projectNo)
        {
            return (await _dataAccess.QueryAsync<RoadmapMilestone>(@"
SELECT roadmapsysid, roadmapmilestonesysid, maturitycode, parenttype, parentsysid, milestonealias, milestonedescription,
       orderindex, NVL(isactive, 1) AS isactive, NVL(isrequired, 1) AS isrequired,
       createdby, createddate, modifiedby, modifieddate, transactionkey
  FROM projectroadmapmilestones
 WHERE projectno = :ProjectNo", new { ProjectNo = projectNo })).ToList();
        }

        public async Task<List<RoadmapActivity>> GetProjectRoadmapActivitiesAsync(string projectNo)
        {
            return (await _dataAccess.QueryAsync<RoadmapActivity>(@"
SELECT roadmapsysid, roadmapactivitysysid, parenttype, parentsysid, activityname, activitydescription,
       estimatedmandays, NVL(isrequired, 1) AS isrequired, orderindex, NVL(isactive, 1) AS isactive,
       createdby, createddate, modifiedby, modifieddate, transactionkey
  FROM projectroadmapactivities
 WHERE projectno = :ProjectNo", new { ProjectNo = projectNo })).ToList();
        }

        public async Task<List<RoadmapActivityPrerequisite>> GetProjectRoadmapActivityPrerequisitesAsync(string projectNo)
        {
            return (await _dataAccess.QueryAsync<RoadmapActivityPrerequisite>(@"
SELECT roadmapactivityprereqsysid, roadmapactivitysysid, prerequisitesysid
  FROM projectroadmapactivityprereqs
 WHERE projectno = :ProjectNo", new { ProjectNo = projectNo })).ToList();
        }

        public async Task<Roadmap> GetProjectRoadmapAsync(string projectNo)
        {
            return (await _dataAccess.QueryAsync<Roadmap>(@"
SELECT roadmapsysid, roadmapname, roadmapdescription, categorycode, isactive, createdby, createddate, modifiedby, modifieddate, transactionkey
  FROM projectroadmaps
 WHERE projectno = :ProjectNo", new { ProjectNo = projectNo })).SingleOrDefault();
        }

        public async Task AddProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId, string roadmapSysId, string maturityCode, string parentType, string parentSysId, string milestoneAlias, string milestoneDescription, int orderIndex, int isActive, int isRequired, string createdBy, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
INSERT INTO projectroadmapmilestones (
    projectno,
    roadmapmilestonesysid,
    roadmapsysid,
    maturitycode,
    parenttype,
    parentsysid,
    milestonealias,
    milestonedescription,
    orderindex,
    isactive,
    createdby,
    createddate,
    modifiedby,
    modifieddate,
    transactionkey,
    isrequired)
VALUES (
    :ProjectNo,
    :RoadmapMilestoneSysId,
    :RoadmapSysId,
    :MaturityCode,
    :ParentType,
    :ParentSysId,
    :MilestoneAlias,
    :MilestoneDescription,
    :OrderIndex,
    :IsActive,
    :CreatedBy,
    SYSTIMESTAMP,
    :ModifiedBy,
    SYSTIMESTAMP,
    SYS_GUID(),
    :IsRequired)",
                new
                {
                    ProjectNo = projectNo,
                    RoadmapMilestoneSysId = roadmapMilestoneSysId,
                    RoadmapSysId = roadmapSysId,
                    MaturityCode = maturityCode,
                    ParentType = parentType,
                    ParentSysId = string.IsNullOrWhiteSpace(parentSysId) ? null : parentSysId,
                    MilestoneAlias = milestoneAlias,
                    MilestoneDescription = milestoneDescription,
                    OrderIndex = orderIndex,
                    IsActive = isActive,
                    CreatedBy = createdBy,
                    ModifiedBy = modifiedBy,
                    IsRequired = isRequired
                });
        }

        public async Task UpdateProjectRoadmapMilestoneAsync(string projectNo, RoadmapMilestone milestone, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapmilestones
   SET roadmapsysid = :RoadmapSysId,
       maturitycode = :MaturityCode,
       parenttype = :ParentType,
       parentsysid = :ParentSysId,
       milestonealias = :MilestoneAlias,
       milestonedescription = :MilestoneDescription,
       orderindex = :OrderIndex,
       isactive = :IsActive,
       isrequired = :IsRequired,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND roadmapmilestonesysid = :RoadmapMilestoneSysId",
                new
                {
                    ProjectNo = projectNo,
                    milestone.RoadmapSysId,
                    milestone.MaturityCode,
                    milestone.ParentType,
                    ParentSysId = string.IsNullOrWhiteSpace(milestone.ParentSysId) ? null : milestone.ParentSysId,
                    milestone.MilestoneAlias,
                    MilestoneDescription = milestone.MilestoneDescription,
                    milestone.OrderIndex,
                    milestone.IsActive,
                    milestone.IsRequired,
                    ModifiedBy = modifiedBy,
                    milestone.RoadmapMilestoneSysId
                });
        }

        public async Task DeleteProjectRoadmapMilestoneAsync(string projectNo, string roadmapMilestoneSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectroadmapmilestones
 WHERE projectno = :ProjectNo
   AND roadmapmilestonesysid = :RoadmapMilestoneSysId",
                new { ProjectNo = projectNo, RoadmapMilestoneSysId = roadmapMilestoneSysId });
        }

        public async Task UpdateProjectRoadmapMilestoneOrderAsync(string projectNo, string roadmapMilestoneSysId, int orderIndex, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapmilestones
   SET orderindex = :OrderIndex,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND roadmapmilestonesysid = :RoadmapMilestoneSysId",
                new { ProjectNo = projectNo, RoadmapMilestoneSysId = roadmapMilestoneSysId, OrderIndex = orderIndex, ModifiedBy = modifiedBy });
        }

        public async Task AddProjectRoadmapActivityAsync(string projectNo, string roadmapActivitySysId, string roadmapSysId, string parentType, string parentSysId, string activityName, string activityDescription, int estimatedManDays, int isRequired, int orderIndex, int isActive, string createdBy, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
INSERT INTO projectroadmapactivities (
    projectno, roadmapactivitysysid, roadmapsysid, parenttype, parentsysid,
    activityname, activitydescription, estimatedmandays, isrequired, orderindex,
    isactive, createdby, createddate, modifiedby, modifieddate, transactionkey)
VALUES (
    :ProjectNo, :RoadmapActivitySysId, :RoadmapSysId, :ParentType, :ParentSysId,
    :ActivityName, :ActivityDescription, :EstimatedManDays, :IsRequired, :OrderIndex,
    :IsActive, :CreatedBy, SYSTIMESTAMP, :ModifiedBy, SYSTIMESTAMP, SYS_GUID())",
                new
                {
                    ProjectNo = projectNo,
                    RoadmapActivitySysId = roadmapActivitySysId,
                    RoadmapSysId = roadmapSysId,
                    ParentType = parentType,
                    ParentSysId = string.IsNullOrWhiteSpace(parentSysId) ? null : parentSysId,
                    ActivityName = activityName,
                    ActivityDescription = activityDescription,
                    EstimatedManDays = estimatedManDays,
                    IsRequired = isRequired,
                    OrderIndex = orderIndex,
                    IsActive = isActive,
                    CreatedBy = createdBy,
                    ModifiedBy = modifiedBy
                });
        }

        public async Task UpdateProjectRoadmapActivityAsync(string projectNo, RoadmapActivity activity, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapactivities
   SET roadmapsysid = :RoadmapSysId,
       parenttype = :ParentType,
       parentsysid = :ParentSysId,
       activityname = :ActivityName,
       activitydescription = :ActivityDescription,
       estimatedmandays = :EstimatedManDays,
       isrequired = :IsRequired,
       orderindex = :OrderIndex,
       isactive = :IsActive,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND roadmapactivitysysid = :RoadmapActivitySysId",
                new
                {
                    ProjectNo = projectNo,
                    activity.RoadmapSysId,
                    activity.ParentType,
                    ParentSysId = string.IsNullOrWhiteSpace(activity.ParentSysId) ? null : activity.ParentSysId,
                    activity.ActivityName,
                    ActivityDescription = activity.ActivityDescription,
                    activity.EstimatedManDays,
                    activity.IsRequired,
                    activity.OrderIndex,
                    activity.IsActive,
                    ModifiedBy = modifiedBy,
                    activity.RoadmapActivitySysId
                });
        }

        public async Task DeleteProjectRoadmapActivityAsync(string projectNo, string roadmapActivitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectroadmapactivities
 WHERE projectno = :ProjectNo
   AND roadmapactivitysysid = :RoadmapActivitySysId",
                new { ProjectNo = projectNo, RoadmapActivitySysId = roadmapActivitySysId });
        }

        public async Task DeleteProjectRoadmapActivityPrerequisitesForActivityAsync(string projectNo, string roadmapActivitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectroadmapactivityprereqs
 WHERE projectno = :ProjectNo
   AND (roadmapactivitysysid = :RoadmapActivitySysId OR prerequisitesysid = :RoadmapActivitySysId)",
                new { ProjectNo = projectNo, RoadmapActivitySysId = roadmapActivitySysId });
        }

        public async Task AddProjectRoadmapActivityPrerequisiteAsync(string projectNo, string roadmapActivityPrereqSysId, string roadmapActivitySysId, string prerequisiteSysId)
        {
            await _dataAccess.ExecuteAsync(@"
INSERT INTO projectroadmapactivityprereqs (
    projectno, roadmapactivityprereqsysid, roadmapactivitysysid, prerequisitesysid)
VALUES (
    :ProjectNo, :RoadmapActivityPrereqSysId, :RoadmapActivitySysId, :PrerequisiteSysId)",
                new
                {
                    ProjectNo = projectNo,
                    RoadmapActivityPrereqSysId = roadmapActivityPrereqSysId,
                    RoadmapActivitySysId = roadmapActivitySysId,
                    PrerequisiteSysId = prerequisiteSysId
                });
        }

        public async Task AddProjectRoadmapFromMasterAsync(string projectNo, string roadmapSysId, string createdBy, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
INSERT INTO projectroadmaps (
    projectno, roadmapsysid, roadmapname, roadmapdescription, versionno, categorycode,
    isactive, createdby, createddate, modifiedby, modifieddate, transactionkey)
SELECT :ProjectNo, roadmapsysid, roadmapname, roadmapdescription, versionno, categorycode,
       isactive, :CreatedBy, SYSTIMESTAMP, :ModifiedBy, SYSTIMESTAMP, SYS_GUID()
  FROM roadmaps
 WHERE roadmapsysid = :RoadmapSysId",
                new { ProjectNo = projectNo, RoadmapSysId = roadmapSysId, CreatedBy = createdBy, ModifiedBy = modifiedBy });
        }

        public async Task ShiftProjectRoadmapSiblingOrderAsync(string projectNo, string parentType, string parentSysId, int insertOrder, string modifiedBy)
        {
            var parameters = new
            {
                ProjectNo = projectNo,
                ParentType = parentType,
                ParentSysId = parentSysId,
                InsertOrder = insertOrder,
                ModifiedBy = modifiedBy
            };

            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapmilestones
   SET orderindex = orderindex + 1,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND parenttype = :ParentType
   AND parentsysid = :ParentSysId
   AND orderindex >= :InsertOrder", parameters);

            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapactivities
   SET orderindex = orderindex + 1,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND parenttype = :ParentType
   AND parentsysid = :ParentSysId
   AND orderindex >= :InsertOrder", parameters);
        }

        public async Task CloseProjectRoadmapMilestoneSiblingGapAsync(string projectNo, string parentType, string parentSysId, int deletedOrderIndex, string modifiedBy)
        {
            await _dataAccess.ExecuteAsync(@"
UPDATE projectroadmapmilestones
   SET orderindex = orderindex - 1,
       modifiedby = :ModifiedBy,
       modifieddate = SYSTIMESTAMP,
       transactionkey = SYS_GUID()
 WHERE projectno = :ProjectNo
   AND parenttype = :ParentType
   AND parentsysid = :ParentSysId
   AND orderindex > :DeletedOrderIndex",
                new
                {
                    ProjectNo = projectNo,
                    ParentType = parentType,
                    ParentSysId = parentSysId,
                    DeletedOrderIndex = deletedOrderIndex,
                    ModifiedBy = modifiedBy
                });
        }

        public async Task<List<FormEntityLink>> GetFormEntityLinksByEntityAsync(string entityType, string entitySysId)
        {
            return (await _dataAccess.QueryAsync<FormEntityLink>(@"
SELECT formentitylinksysid, formsysid, entitytype, entitysysid, orderindex, NVL(isactive, 1) AS isactive,
       createdby, createddate, modifiedby, modifieddate, transactionkey
  FROM formentitylinks
 WHERE LOWER(entitytype) = :EntityType
   AND entitysysid = :EntitySysId",
                new
                {
                    EntityType = (entityType ?? string.Empty).ToLowerInvariant(),
                    EntitySysId = entitySysId
                })).ToList();
        }

        public async Task DeleteProjectFormSubmissionsByFormEntityLinkAsync(string projectNo, string formEntityLinkSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectformsubmissions
 WHERE projectno = :ProjectNo
   AND formentitylinksysid = :FormEntityLinkSysId",
                new { ProjectNo = projectNo, FormEntityLinkSysId = formEntityLinkSysId });
        }

        public async Task DeleteProjectFormSubmissionValuesByFormEntityLinkAsync(string projectNo, string formEntityLinkSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectformsubmissionvalues
 WHERE projectno = :ProjectNo
   AND formentitylinksysid = :FormEntityLinkSysId",
                new { ProjectNo = projectNo, FormEntityLinkSysId = formEntityLinkSysId });
        }

        public async Task DeleteProjectFieldsByMilestoneAsync(string projectNo, string milestoneSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectfields
 WHERE projectno = :ProjectNo
   AND milestonesysid = :MilestoneSysId",
                new { ProjectNo = projectNo, MilestoneSysId = milestoneSysId });
        }

        public async Task DeleteProjectFieldsByTaskAsync(string projectNo, string taskSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectfields
 WHERE projectno = :ProjectNo
   AND tasksysid = :TaskSysId",
                new { ProjectNo = projectNo, TaskSysId = taskSysId });
        }

        public async Task DeleteProjectStatusChangesByEntityAsync(string projectNo, string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectstatuschanges
 WHERE projectno = :ProjectNo
   AND UPPER(entitytype) = :EntityType
   AND entitysysid = :EntitySysId",
                new { ProjectNo = projectNo, EntityType = (entityType ?? string.Empty).ToUpperInvariant(), EntitySysId = entitySysId });
        }

        public async Task DeleteProjectTargetRevisionsForMilestoneAsync(string projectNo, string projectNodeSysId, string roadmapMilestoneSysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projecttargetrevisions
 WHERE projectno = :ProjectNo
   AND (projectnodesysid = :ProjectNodeSysId
        OR (UPPER(entitytype) = 'MILESTONE' AND entitysysid = :RoadmapMilestoneSysId))",
                new { ProjectNo = projectNo, ProjectNodeSysId = projectNodeSysId, RoadmapMilestoneSysId = roadmapMilestoneSysId });
        }

        public async Task DeleteProjectTargetRevisionsForTaskAsync(string projectNo, string projectNodeSysId, string roadmapActivitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projecttargetrevisions
 WHERE projectno = :ProjectNo
   AND (projectnodesysid = :ProjectNodeSysId
        OR (UPPER(entitytype) = 'ACTIVITY' AND entitysysid = :RoadmapActivitySysId))",
                new { ProjectNo = projectNo, ProjectNodeSysId = projectNodeSysId, RoadmapActivitySysId = roadmapActivitySysId });
        }

        public async Task DeleteProjectOwnersByEntityAsync(string projectNo, string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectowners
 WHERE projectno = :ProjectNo
   AND LOWER(parenttype) = :EntityType
   AND parentsysid = :EntitySysId",
                new { ProjectNo = projectNo, EntityType = (entityType ?? string.Empty).ToLowerInvariant(), EntitySysId = entitySysId });
        }

        public async Task DeleteProjectCommentsByEntityAsync(string projectNo, string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectcomments
 WHERE projectno = :ProjectNo
   AND LOWER(entitytype) = :EntityType
   AND entitysysid = :EntitySysId",
                new { ProjectNo = projectNo, EntityType = (entityType ?? string.Empty).ToLowerInvariant(), EntitySysId = entitySysId });
        }

        public async Task DeleteProjectAttachmentsByEntityAsync(string projectNo, string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM projectattachments
 WHERE projectno = :ProjectNo
   AND LOWER(entitytype) = :EntityType
   AND entitysysid = :EntitySysId",
                new { ProjectNo = projectNo, EntityType = (entityType ?? string.Empty).ToLowerInvariant(), EntitySysId = entitySysId });
        }

        public async Task DeleteNotificationViewedByEntityAsync(string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM notificationviewed
 WHERE notificationsysid IN (
        SELECT notificationsysid
          FROM notifications
         WHERE LOWER(entitytype) = :EntityType
           AND entitysysid = :EntitySysId)",
                new { EntityType = (entityType ?? string.Empty).ToLowerInvariant(), EntitySysId = entitySysId });
        }

        public async Task DeleteNotificationsByEntityAsync(string entityType, string entitySysId)
        {
            await _dataAccess.ExecuteAsync(@"
DELETE FROM notifications
 WHERE LOWER(entitytype) = :EntityType
   AND entitysysid = :EntitySysId",
                new { EntityType = (entityType ?? string.Empty).ToLowerInvariant(), EntitySysId = entitySysId });
        }
    }
}
