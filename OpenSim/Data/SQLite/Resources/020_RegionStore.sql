BEGIN;

ALTER TABLE prims ADD COLUMN MediaURL varchar(255);
ALTER TABLE primshapes ADD COLUMN Media TEXT;
  
COMMIT;