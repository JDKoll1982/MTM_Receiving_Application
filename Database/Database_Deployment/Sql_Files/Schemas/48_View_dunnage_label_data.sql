-- View: view_dunnage_label_data
-- Description: Flat denormalized view of the dunnage_label_data active queue
--              joined to its linked tables so a single LabelView Table Lookup
--              can show the whole row plus the related data:
--                dunnage_parts (home location, image, quantity type, current
--                  udc1..udc10 values, audit fields)
--                dunnage_types (live type name / icon / image)
--                auth_users  (full name, department, shift, active flag)
--                dunnage_quantity_types (created_by / created_date)
--                dunnage_custom_fields (udc1_name..udc10_name display names)
--              dunnage_label_data columns keep their original names so existing
--              LabelView bindings stay stable; linked columns use prefixed
--              aliases.
-- Usage in LabelView 2022 (Table Lookup, ODBC to MySQL):
--   SELECT * FROM view_dunnage_label_data
--   WHERE load_uuid = APPLICATION.DOCUMENT.[LoadUuid]
--
-- Note: this view is read-only (SELECT). It is safe to query from LabelView;
--       the MTM application never writes to it.

DROP VIEW IF EXISTS `view_dunnage_label_data`;

CREATE VIEW `view_dunnage_label_data` AS
SELECT
    dld.id,
    dld.load_uuid,
    dld.part_id,
    dld.dunnage_type_id,
    dld.dunnage_type_name,
    dld.dunnage_type_icon,
    dld.quantity,
    dld.quantity_type,
    dld.po_number,
    dld.received_date,
    dld.user_id,
    dld.employee_number,
    dld.location,
    dld.label_number,
    dld.part_skid_sequence,
    dld.part_skid_total,
    dld.udc1,
    dld.udc2,
    dld.udc3,
    dld.udc4,
    dld.udc5,
    dld.udc6,
    dld.udc7,
    dld.udc8,
    dld.udc9,
    dld.udc10,
    dld.created_at,
    dp.home_location    AS part_home_location,
    dp.image_path       AS part_image_path,
    dp.quantity_type    AS part_quantity_type,
    dp.udc1             AS part_udc1,
    dp.udc2             AS part_udc2,
    dp.udc3             AS part_udc3,
    dp.udc4             AS part_udc4,
    dp.udc5             AS part_udc5,
    dp.udc6             AS part_udc6,
    dp.udc7             AS part_udc7,
    dp.udc8             AS part_udc8,
    dp.udc9             AS part_udc9,
    dp.udc10            AS part_udc10,
    dp.created_by       AS part_created_by,
    dp.created_date     AS part_created_date,
    dp.modified_by      AS part_modified_by,
    dp.modified_date    AS part_modified_date,
    dt.type_name        AS type_name_live,
    dt.icon             AS type_icon_live,
    dt.image_path       AS type_image_path,
    au.full_name        AS employee_full_name,
    au.department       AS employee_department,
    au.shift            AS employee_shift,
    au.is_active        AS employee_active,
    dqt.created_by      AS qty_type_created_by,
    dqt.created_date    AS qty_type_created_date,
    c1.FieldName        AS udc1_name,
    c2.FieldName        AS udc2_name,
    c3.FieldName        AS udc3_name,
    c4.FieldName        AS udc4_name,
    c5.FieldName        AS udc5_name,
    c6.FieldName        AS udc6_name,
    c7.FieldName        AS udc7_name,
    c8.FieldName        AS udc8_name,
    c9.FieldName        AS udc9_name,
    c10.FieldName       AS udc10_name
FROM dunnage_label_data dld
LEFT JOIN dunnage_parts dp
       ON dp.part_id = dld.part_id
LEFT JOIN dunnage_types dt
       ON dt.id = dld.dunnage_type_id
LEFT JOIN auth_users au
       ON au.windows_username = dld.user_id
LEFT JOIN dunnage_quantity_types dqt
       ON dqt.quantity_type = dld.quantity_type
LEFT JOIN dunnage_custom_fields c1
       ON c1.DunnageTypeID = dld.dunnage_type_id AND c1.DisplayOrder = 1
LEFT JOIN dunnage_custom_fields c2
       ON c2.DunnageTypeID = dld.dunnage_type_id AND c2.DisplayOrder = 2
LEFT JOIN dunnage_custom_fields c3
       ON c3.DunnageTypeID = dld.dunnage_type_id AND c3.DisplayOrder = 3
LEFT JOIN dunnage_custom_fields c4
       ON c4.DunnageTypeID = dld.dunnage_type_id AND c4.DisplayOrder = 4
LEFT JOIN dunnage_custom_fields c5
       ON c5.DunnageTypeID = dld.dunnage_type_id AND c5.DisplayOrder = 5
LEFT JOIN dunnage_custom_fields c6
       ON c6.DunnageTypeID = dld.dunnage_type_id AND c6.DisplayOrder = 6
LEFT JOIN dunnage_custom_fields c7
       ON c7.DunnageTypeID = dld.dunnage_type_id AND c7.DisplayOrder = 7
LEFT JOIN dunnage_custom_fields c8
       ON c8.DunnageTypeID = dld.dunnage_type_id AND c8.DisplayOrder = 8
LEFT JOIN dunnage_custom_fields c9
       ON c9.DunnageTypeID = dld.dunnage_type_id AND c9.DisplayOrder = 9
LEFT JOIN dunnage_custom_fields c10
       ON c10.DunnageTypeID = dld.dunnage_type_id AND c10.DisplayOrder = 10;
