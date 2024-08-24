-- Delete ModShark's native reports
DELETE FROM "abuse_user_report"
WHERE comment LIKE 'ModShark: %'
;

-- Un-flag and re-queue all users 
DELETE FROM ms_flagged_user WHERE true
;
INSERT INTO ms_queued_user (user_id)
SELECT id as user_id
FROM "user"
-- LIMIT 5000
ON CONFLICT DO NOTHING
;

-- Un-flag and re-queue all instances
DELETE FROM ms_flagged_instance WHERE true
;
INSERT INTO ms_queued_instance (instance_id)
SELECT id as instance_id
FROM "instance"
-- LIMIT 20000
ON CONFLICT DO NOTHING
;

-- Un-flag and re-queue all notes
DELETE FROM ms_flagged_note WHERE true
;
INSERT INTO ms_queued_note (note_id)
SELECT id as note_id
FROM "note"
-- LIMIT 50000
ON CONFLICT DO NOTHING
;

-- List all native reports
SELECT * FROM "abuse_user_report";

-- Count all scannable objects in the database
SELECT COUNT(*) FROM "user"
UNION ALL
SELECT COUNT(*) FROM "instance"
UNION ALL
SELECT COUNT(*) FROM "note"
;