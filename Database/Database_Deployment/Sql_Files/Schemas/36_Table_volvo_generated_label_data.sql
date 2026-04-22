DROP TABLE IF EXISTS volvo_generated_label_data;

CREATE TABLE IF NOT EXISTS volvo_generated_label_data (
    id INT NOT NULL AUTO_INCREMENT COMMENT 'Surrogate primary key for one generated Volvo label row',
    shipment_id INT NOT NULL COMMENT 'Original Volvo shipment header identifier used when this label row was generated',
    shipment_number INT NOT NULL COMMENT 'Shipment number snapshot for ordering and troubleshooting',
    shipment_date DATE NOT NULL COMMENT 'Shipment date snapshot for ordering and troubleshooting',
    part_number VARCHAR(20) NOT NULL COMMENT 'Volvo part number printed on the label',
    quantity INT NOT NULL COMMENT 'Quantity printed on the label for this skid; this is quantity per skid, not calculated piece count',
    skid_number INT NOT NULL COMMENT '1-based skid sequence for this part within the shipment',
    total_skids INT NOT NULL COMMENT 'Total skid count for this part within the shipment',
    part_description VARCHAR(255) NOT NULL COMMENT 'Part description resolved for the label at generation time',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Timestamp when this generated label row was written',
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Timestamp when this generated label row was last refreshed',
    PRIMARY KEY (id),
    INDEX idx_volvo_generated_label_shipment_id (shipment_id),
    INDEX idx_volvo_generated_label_shipment_number (shipment_number),
    INDEX idx_volvo_generated_label_part_number (part_number),
    INDEX idx_volvo_generated_label_created_at (created_at)
) ENGINE = InnoDB DEFAULT CHARSET = utf8mb4 COLLATE = utf8mb4_unicode_ci COMMENT = 'Active Volvo label rows queued for LabelView generation';