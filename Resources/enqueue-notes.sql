-- Queue all known notes.
insert into ms_queued_note (note_id)
select id as note_id
from "note"
on conflict do nothing;

-- Reset flags to ensure new notes get processed.
delete from ms_flagged_note f
using ms_queued_note q
where q.note_id = f.note_id;