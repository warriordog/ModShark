DELETE FROM "abuse_user_report"
WHERE comment LIKE 'ModShark: %'
;
DELETE FROM ms_flagged_user WHERE true
;
INSERT INTO ms_queued_user (user_id)
SELECT id as user_id
FROM "user"
ON CONFLICT DO NOTHING
;
DELETE FROM ms_flagged_instance WHERE true
;
INSERT INTO ms_queued_instance (instance_id)
SELECT id as instance_id
FROM "instance"
ON CONFLICT DO NOTHING
;
DELETE FROM ms_flagged_note WHERE true
;
INSERT INTO ms_queued_note (note_id)
SELECT id as note_id
FROM "note"
LIMIT 50000
ON CONFLICT DO NOTHING
;


SELECT * FROM "abuse_user_report";

SELECT COUNT(*) FROM "user"
UNION ALL
SELECT COUNT(*) FROM "instance";