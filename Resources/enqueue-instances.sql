-- Queue all known instances.
insert into ms_queued_instance (instance_id)
select id as instance_id
from "instance"
on conflict do nothing;

-- Reset flags to ensure new instances get processed.
delete from ms_flagged_instance f
using ms_queued_instance q
where q.instance_id = f.instance_id;