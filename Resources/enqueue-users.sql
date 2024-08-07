-- Queue all known users.
insert into ms_queued_user (user_id)
select id as user_id
from "user"
on conflict do nothing;

-- Reset flags to ensure new users get processed.
delete from ms_flagged_user f
using ms_queued_user q
where q.user_id = f.user_id;