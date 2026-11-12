-- =============================================================================
-- Procedure : sp_Reconciliation_DayActivity_GetRange
-- Module    : Reconciliation (Receiving Validator)
-- Purpose   : Answers which days inside a bounded range hold at least one
--             receiving row, so day stepping never runs a full preview.
-- Parameters:
--   p_from DATE  in   range start, inclusive
--   p_to   DATE  in   range end, inclusive
-- Returns   : one row set with a single column, activity_date DATE, one row per
--             distinct active day, ordered ascending.
-- Notes     : No join, no movement count and no Infor Visual access. This is the
--             query that makes day stepping cheap (FR-009, R-SQL-02).
-- =============================================================================

DELIMITER $$

DROP PROCEDURE IF EXISTS `sp_Reconciliation_DayActivity_GetRange` $$

CREATE PROCEDURE `sp_Reconciliation_DayActivity_GetRange`(
    IN p_from DATE,
    IN p_to DATE
)
BEGIN
    SELECT activity_date
    FROM (
        SELECT DISTINCT ld.transaction_date AS activity_date
        FROM receiving_label_data ld
        WHERE ld.transaction_date BETWEEN p_from AND p_to

        UNION

        SELECT DISTINCT h.transaction_date AS activity_date
        FROM receiving_history h
        WHERE h.transaction_date BETWEEN p_from AND p_to
    ) AS activity_days
    ORDER BY activity_date ASC;
END $$

DELIMITER ;

-- TEST BLOCK START: Sample range
-- CALL sp_Reconciliation_DayActivity_GetRange('2026-11-01', '2026-11-12');
-- TEST BLOCK END: Sample range
