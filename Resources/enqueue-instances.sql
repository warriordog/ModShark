/*
 * === Very important note ===
 * This script uses unlogged writes which are NOT fail-safe!
 * Make sure to stop ModShark and Sharkey before executing.
 * Your instance data is not at risk, but ModShark's queues could be corrupted.
 *  
 * Recommendation: increase checkpoint_segments to 32 or higher.
 * See https://stackoverflow.com/a/52271138 for details.
 *
 * https://www.postgresql.org/docs/current/sql-altertable.html#SQL-ALTERTABLE-DESC-SET-LOGGED-UNLOGGED 
 * https://www.postgresql.org/docs/current/sql-set-constraints.html
 */

begin transaction;

-- Queue all known instances.
set constraints all deferred;
    alter table ms_queued_instance set unlogged;
        insert into ms_queued_instance (instance_id)
        select id as instance_id
        from "instance"
        on conflict do nothing;
    alter table ms_queued_instance set logged;
set constraints all immediate;

-- Reset flags to ensure new instances get processed.
delete from ms_flagged_instance f
using ms_queued_instance q
where q.instance_id = f.instance_id;

commit;