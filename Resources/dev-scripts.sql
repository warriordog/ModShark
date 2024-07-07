DELETE FROM ms_flagged_user
;
DELETE FROM ms_flagged_instance
;
DELETE FROM "abuse_user_report"
WHERE comment LIKE 'ModShark: %'
;
INSERT INTO ms_queued_user (user_id)
SELECT id as user_id
FROM "user"
ON CONFLICT DO NOTHING
;
INSERT INTO ms_queued_instance (instance_id)
SELECT id as instance_id
FROM "instance"
ON CONFLICT DO NOTHING
;


SELECT * FROM "abuse_user_report";

SELECT COUNT(*) FROM "user"
UNION ALL
SELECT COUNT(*) FROM "instance";