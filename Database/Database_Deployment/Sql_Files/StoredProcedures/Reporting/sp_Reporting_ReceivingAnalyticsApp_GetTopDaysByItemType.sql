DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reporting_ReceivingAnalyticsApp_GetTopDaysByItemType`$$

/*
Receiving Analytics — Record Days dataset (read-only).

Returns the busiest receiving days for each item type, measured in distinct loads. One row is one
day and item type. The item type is derived from the part number exactly as
Translation_ReceivingAnalyticsCategory.FromPartNumber does in C#: contains MMC -> Coil,
contains MMF -> Sheet, anything else -> Standard.

A load is counted once per item type it carried, so a load that carried both a coil and a flat
stock part appears in both rankings. Weekend receipts are excluded for the same reason every other
App Data figure excludes them: the dock does not read weekend work.

Parameters:
    p_start_date    DATETIME    First day in the range, inclusive.
    p_end_date      DATETIME    First day after the range, exclusive.
    p_top_count     INT         Days to keep per item type; NULL or 0 falls back to 10.

Returns:
    day_date        DATE        Calendar date of COALESCE(received_date, transaction_date).
    item_type       VARCHAR     'Coil', 'Sheet' or 'Standard'.
    load_count      INT         Distinct loads for that day and item type.
    label_count     INT         Receipt rows (one row of receiving history is one app label).
*/
CREATE PROCEDURE `sp_Reporting_ReceivingAnalyticsApp_GetTopDaysByItemType`(
    IN p_start_date DATETIME,
    IN p_end_date DATETIME,
    IN p_top_count INT
)
BEGIN
    DECLARE v_top_count INT DEFAULT 10;

    IF p_top_count IS NOT NULL AND p_top_count > 0 THEN
        SET v_top_count = p_top_count;
    END IF;

    DROP TEMPORARY TABLE IF EXISTS tmp_receiving_analytics_top_days;
    DROP TEMPORARY TABLE IF EXISTS tmp_receiving_analytics_busier_days;

    CREATE TEMPORARY TABLE tmp_receiving_analytics_top_days (
        day_date DATE NOT NULL,
        item_type VARCHAR(20) NOT NULL,
        load_count INT NOT NULL,
        label_count INT NOT NULL,
        INDEX idx_top_days_rank (item_type, load_count)
    );

    INSERT INTO tmp_receiving_analytics_top_days (day_date, item_type, load_count, label_count)
    SELECT
        DATE(COALESCE(rh.received_date, rh.transaction_date)) AS day_date,
        CASE
            WHEN UPPER(COALESCE(rh.part_id, '')) LIKE '%MMC%' THEN 'Coil'
            WHEN UPPER(COALESCE(rh.part_id, '')) LIKE '%MMF%' THEN 'Sheet'
            ELSE 'Standard'
        END AS item_type,
        COUNT(DISTINCT COALESCE(rh.load_id, rh.load_guid, CAST(rh.id AS CHAR))) AS load_count,
        COUNT(*) AS label_count
    FROM receiving_history rh
    WHERE
        (
            (rh.received_date IS NOT NULL AND rh.received_date >= p_start_date AND rh.received_date < p_end_date)
            OR
            (rh.received_date IS NULL AND rh.transaction_date >= p_start_date AND rh.transaction_date < p_end_date)
        )
        -- DAYOFWEEK: 1 = Sunday, 7 = Saturday. The dock does not read weekend work.
        AND DAYOFWEEK(COALESCE(rh.received_date, rh.transaction_date)) NOT IN (1, 7)
    GROUP BY
        DATE(COALESCE(rh.received_date, rh.transaction_date)),
        CASE
            WHEN UPPER(COALESCE(rh.part_id, '')) LIKE '%MMC%' THEN 'Coil'
            WHEN UPPER(COALESCE(rh.part_id, '')) LIKE '%MMF%' THEN 'Sheet'
            ELSE 'Standard'
        END;

    -- MySQL 5.7 cannot reference one temporary table twice in the same statement, so the ranking
    -- comparison reads a second copy of the same rows.
    CREATE TEMPORARY TABLE tmp_receiving_analytics_busier_days (
        day_date DATE NOT NULL,
        item_type VARCHAR(20) NOT NULL,
        load_count INT NOT NULL,
        label_count INT NOT NULL,
        INDEX idx_busier_days_rank (item_type, load_count)
    );

    INSERT INTO tmp_receiving_analytics_busier_days (day_date, item_type, load_count, label_count)
    SELECT
        top_days.day_date,
        top_days.item_type,
        top_days.load_count,
        top_days.label_count
    FROM tmp_receiving_analytics_top_days AS top_days;

    SELECT
        top_days.day_date AS day_date,
        top_days.item_type AS item_type,
        top_days.load_count AS load_count,
        top_days.label_count AS label_count
    FROM tmp_receiving_analytics_top_days AS top_days
    WHERE (
        SELECT COUNT(*)
        FROM tmp_receiving_analytics_busier_days AS busier
        WHERE
            busier.item_type = top_days.item_type
            AND busier.load_count > top_days.load_count
    ) < v_top_count
    ORDER BY
        top_days.item_type ASC,
        top_days.load_count DESC,
        top_days.day_date DESC;

    DROP TEMPORARY TABLE IF EXISTS tmp_receiving_analytics_busier_days;
    DROP TEMPORARY TABLE IF EXISTS tmp_receiving_analytics_top_days;
END $$

DELIMITER ;
