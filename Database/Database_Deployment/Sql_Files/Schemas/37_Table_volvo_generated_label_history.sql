DROP TABLE IF EXISTS volvo_generated_label_history;

CREATE TABLE IF NOT EXISTS volvo_generated_label_history (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate primary key for one archived generated Volvo label row',
    original_id INT NOT NULL COMMENT 'Original volvo_generated_label_data.id value before archival',
    shipment_id INT NOT NULL COMMENT 'Original Volvo shipment header identifier used when this label row was generated',
    shipment_number INT NOT NULL COMMENT 'Shipment number snapshot preserved from the active label row',
    shipment_date DATE NOT NULL COMMENT 'Shipment date snapshot preserved from the active label row',
    part_number VARCHAR(20) NOT NULL COMMENT 'Volvo part number printed on the label',
    quantity INT NOT NULL COMMENT 'Quantity printed on the label for this skid; this is quantity per skid, not calculated piece count',
    skid_number INT NOT NULL COMMENT '1-based skid sequence for this part within the shipment',
    total_skids INT NOT NULL COMMENT 'Total skid count for this part within the shipment',
    part_description VARCHAR(255) NOT NULL COMMENT 'Part description snapshot preserved from the active label row',
    employee_number INT NULL COMMENT '4-digit employee identifier preserved from the active generated label row',
    source_created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Original created_at value from the active queue row',
    source_updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Original updated_at value from the active queue row',
    archived_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when this label row was moved to history',
    archived_by VARCHAR(100) NOT NULL DEFAULT '' COMMENT 'Employee or workstation identity that archived the generated label rows',
    archive_batch_id CHAR(36) NOT NULL DEFAULT '' COMMENT 'UUID shared by all generated label rows moved in the same archive operation',
    PRIMARY KEY (id),
    INDEX idx_volvo_generated_label_history_original_id (original_id),
    INDEX idx_volvo_generated_label_history_batch_id (archive_batch_id),
    INDEX idx_volvo_generated_label_history_shipment_id (shipment_id),
    INDEX idx_volvo_generated_label_history_part_number (part_number),
    INDEX idx_volvo_generated_label_history_employee_number (employee_number),
    INDEX idx_volvo_generated_label_history_archived_at (archived_at)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Archived Volvo generated label rows cleared from the active queue';